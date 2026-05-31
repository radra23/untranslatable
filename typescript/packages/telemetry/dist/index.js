"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.logger = exports.wordCounter = exports.tracer = exports.NullLogger = exports.NullCounter = exports.NullTracer = void 0;
exports.startTelemetry = startTelemetry;
const sdk_node_1 = require("@opentelemetry/sdk-node");
const exporter_trace_otlp_grpc_1 = require("@opentelemetry/exporter-trace-otlp-grpc");
const exporter_metrics_otlp_grpc_1 = require("@opentelemetry/exporter-metrics-otlp-grpc");
const exporter_logs_otlp_grpc_1 = require("@opentelemetry/exporter-logs-otlp-grpc");
const resources_1 = require("@opentelemetry/resources");
const semantic_conventions_1 = require("@opentelemetry/semantic-conventions");
const sdk_metrics_1 = require("@opentelemetry/sdk-metrics");
const sdk_logs_1 = require("@opentelemetry/sdk-logs");
const api_1 = require("@opentelemetry/api");
const api_logs_1 = require("@opentelemetry/api-logs");
// ---- No-op stubs -------------------------------------------------------
class NullSpan {
    setAttributes(_) { }
    addEvent(_) { }
    recordException(_) { }
    setStatus(_) { }
    end() { }
}
class NullTracer {
    startActiveSpan(_name, fn) {
        return fn(new NullSpan());
    }
}
exports.NullTracer = NullTracer;
class NullCounter {
    add(_amount, _attrs) { }
}
exports.NullCounter = NullCounter;
class NullLogger {
    info(_msg, _attrs) { }
    warning(_msg, _attrs) { }
    error(_msg, _attrs) { }
}
exports.NullLogger = NullLogger;
// ---- Module-level facades (start as no-ops) ----------------------------
exports.tracer = new NullTracer();
exports.wordCounter = new NullCounter();
exports.logger = new NullLogger();
// ---- Setup -------------------------------------------------------------
function startTelemetry(options) {
    try {
        const endpoint = process.env.OTLP_ENDPOINT ?? 'http://localhost:4317';
        const serviceName = process.env.OTEL_SERVICE_NAME ?? 'untranslatable-node';
        const resource = new resources_1.Resource({
            [semantic_conventions_1.ATTR_SERVICE_NAME]: serviceName,
            [semantic_conventions_1.ATTR_SERVICE_VERSION]: process.env.OTEL_SERVICE_VERSION ?? '0.1.0',
            'deployment.environment': process.env.OTEL_DEPLOYMENT_ENVIRONMENT ?? 'local',
        });
        // Logs — configure LoggerProvider separately from NodeSDK
        const loggerProvider = new sdk_logs_1.LoggerProvider({ resource });
        loggerProvider.addLogRecordProcessor(new sdk_logs_1.BatchLogRecordProcessor(new exporter_logs_otlp_grpc_1.OTLPLogExporter({ url: endpoint })));
        api_logs_1.logs.setGlobalLoggerProvider(loggerProvider);
        // Traces + Metrics via NodeSDK
        const sdk = new sdk_node_1.NodeSDK({
            resource,
            traceExporter: new exporter_trace_otlp_grpc_1.OTLPTraceExporter({ url: endpoint }),
            metricReader: new sdk_metrics_1.PeriodicExportingMetricReader({
                exporter: new exporter_metrics_otlp_grpc_1.OTLPMetricExporter({ url: endpoint }),
            }),
            instrumentations: options.instrumentations,
        });
        sdk.start();
        // Reassign facades to live OTel objects
        exports.tracer = api_1.trace.getTracer(serviceName);
        exports.wordCounter = api_1.metrics
            .getMeter(serviceName)
            .createCounter('words.requests', {
            description: 'Number of words returned, labelled by language.',
        });
        const otelLogger = api_logs_1.logs.getLogger(serviceName);
        exports.logger = {
            info: (msg, attrs) => otelLogger.emit({ severityNumber: api_logs_1.SeverityNumber.INFO, severityText: 'INFO', body: msg, attributes: attrs }),
            warning: (msg, attrs) => otelLogger.emit({ severityNumber: api_logs_1.SeverityNumber.WARN, severityText: 'WARN', body: msg, attributes: attrs }),
            error: (msg, attrs) => otelLogger.emit({ severityNumber: api_logs_1.SeverityNumber.ERROR, severityText: 'ERROR', body: msg, attributes: attrs }),
        };
        console.log(`[telemetry] OpenTelemetry initialised (endpoint: ${endpoint})`);
    }
    catch (err) {
        console.warn('[telemetry] OTel setup failed — running without instrumentation:', err);
    }
}
//# sourceMappingURL=index.js.map