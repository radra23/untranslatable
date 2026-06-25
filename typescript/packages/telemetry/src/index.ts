import { NodeSDK } from '@opentelemetry/sdk-node';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-grpc';
import { OTLPMetricExporter } from '@opentelemetry/exporter-metrics-otlp-grpc';
import { OTLPLogExporter } from '@opentelemetry/exporter-logs-otlp-grpc';
import { Resource } from '@opentelemetry/resources';
import { ATTR_SERVICE_NAME, ATTR_SERVICE_VERSION } from '@opentelemetry/semantic-conventions';
import { PeriodicExportingMetricReader } from '@opentelemetry/sdk-metrics';
import { LoggerProvider, BatchLogRecordProcessor } from '@opentelemetry/sdk-logs';
import { trace, metrics } from '@opentelemetry/api';
import { logs, SeverityNumber } from '@opentelemetry/api-logs';
import type { Instrumentation } from '@opentelemetry/instrumentation';
import type { AnyValueMap } from '@opentelemetry/api-logs';

export interface AppSpan {
  setAttributes(attrs: Record<string, string | number | boolean>): void;
  addEvent(name: string): void;
  recordException(err: Error): void;
  setStatus(status: { code: number }): void;
  end(): void;
}

export interface AppTracer {
  startActiveSpan<T>(name: string, fn: (span: AppSpan) => T): T;
}

export interface AppCounter {
  add(amount: number, attrs?: Record<string, string>): void;
}

export interface AppLogger {
  info(msg: string, attrs?: Record<string, unknown>): void;
  warning(msg: string, attrs?: Record<string, unknown>): void;
  error(msg: string, attrs?: Record<string, unknown>): void;
}

export interface StartTelemetryOptions {
  instrumentations: Instrumentation[];
}

class NullSpan implements AppSpan {
  setAttributes(_: Record<string, string | number | boolean>): void {}
  addEvent(_: string): void {}
  recordException(_: Error): void {}
  setStatus(_: { code: number }): void {}
  end(): void {}
}

export class NullTracer implements AppTracer {
  startActiveSpan<T>(_name: string, fn: (span: AppSpan) => T): T {
    return fn(new NullSpan());
  }
}

export class NullCounter implements AppCounter {
  add(_amount: number, _attrs?: Record<string, string>): void {}
}

export class NullLogger implements AppLogger {
  info(_msg: string, _attrs?: Record<string, unknown>): void {}
  warning(_msg: string, _attrs?: Record<string, unknown>): void {}
  error(_msg: string, _attrs?: Record<string, unknown>): void {}
}

export let tracer: AppTracer = new NullTracer();
export let wordCounter: AppCounter = new NullCounter();
export let logger: AppLogger = new NullLogger();

let started = false;
let _sdk: NodeSDK | null = null;
let _loggerProvider: LoggerProvider | null = null;

export async function stopTelemetry(): Promise<void> {
  if (_sdk) await _sdk.shutdown();
  if (_loggerProvider) await _loggerProvider.shutdown();
}

export function startTelemetry(options: StartTelemetryOptions): void {
  if (started) return;
  started = true;
  try {
    const endpoint = process.env.OTLP_ENDPOINT ?? 'http://localhost:4317';
    const serviceName = process.env.OTEL_SERVICE_NAME ?? 'untranslatable-node';

    const resource = new Resource({
      [ATTR_SERVICE_NAME]: serviceName,
      [ATTR_SERVICE_VERSION]: process.env.OTEL_SERVICE_VERSION ?? '0.1.0',
      'deployment.environment': process.env.OTEL_DEPLOYMENT_ENVIRONMENT ?? 'local',
    });

    // Logs — configure LoggerProvider separately from NodeSDK
    _loggerProvider = new LoggerProvider({ resource });
    _loggerProvider.addLogRecordProcessor(
      new BatchLogRecordProcessor(new OTLPLogExporter({ url: endpoint }))
    );
    logs.setGlobalLoggerProvider(_loggerProvider);

    _sdk = new NodeSDK({
      resource,
      traceExporter: new OTLPTraceExporter({ url: endpoint }),
      metricReader: new PeriodicExportingMetricReader({
        exporter: new OTLPMetricExporter({ url: endpoint }),
      }),
      instrumentations: options.instrumentations,
    });
    _sdk.start();

    tracer = trace.getTracer(serviceName) as unknown as AppTracer;
    wordCounter = metrics
      .getMeter(serviceName)
      .createCounter('words.requests', {
        description: 'Number of words returned, labelled by language.',
      }) as unknown as AppCounter;

    const otelLogger = logs.getLogger(serviceName);
    logger = {
      info: (msg, attrs) =>
        otelLogger.emit({ severityNumber: SeverityNumber.INFO, severityText: 'INFO', body: msg, attributes: attrs as AnyValueMap | undefined }),
      warning: (msg, attrs) =>
        otelLogger.emit({ severityNumber: SeverityNumber.WARN, severityText: 'WARN', body: msg, attributes: attrs as AnyValueMap | undefined }),
      error: (msg, attrs) =>
        otelLogger.emit({ severityNumber: SeverityNumber.ERROR, severityText: 'ERROR', body: msg, attributes: attrs as AnyValueMap | undefined }),
    };

    const handleSignal = () => void stopTelemetry().finally(() => process.exit(0));
    process.once('SIGTERM', handleSignal);
    process.once('SIGINT', handleSignal);

    console.log(`[telemetry] OpenTelemetry initialised (endpoint: ${endpoint})`);
  } catch (err) {
    console.warn('[telemetry] OTel setup failed — running without instrumentation:', err);
  }
}
