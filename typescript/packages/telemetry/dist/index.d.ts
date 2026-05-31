import type { Instrumentation } from '@opentelemetry/instrumentation';
export interface AppSpan {
    setAttributes(attrs: Record<string, string | number | boolean>): void;
    addEvent(name: string): void;
    recordException(err: Error): void;
    setStatus(status: {
        code: number;
    }): void;
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
declare class NullSpan implements AppSpan {
    setAttributes(_: Record<string, string | number | boolean>): void;
    addEvent(_: string): void;
    recordException(_: Error): void;
    setStatus(_: {
        code: number;
    }): void;
    end(): void;
}
export declare class NullTracer implements AppTracer {
    startActiveSpan<T>(_name: string, fn: (span: NullSpan) => T): T;
}
export declare class NullCounter implements AppCounter {
    add(_amount: number, _attrs?: Record<string, string>): void;
}
export declare class NullLogger implements AppLogger {
    info(_msg: string, _attrs?: Record<string, unknown>): void;
    warning(_msg: string, _attrs?: Record<string, unknown>): void;
    error(_msg: string, _attrs?: Record<string, unknown>): void;
}
export declare let tracer: AppTracer;
export declare let wordCounter: AppCounter;
export declare let logger: AppLogger;
export declare function startTelemetry(options: StartTelemetryOptions): void;
export {};
//# sourceMappingURL=index.d.ts.map