"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vitest_1 = require("vitest");
const index_1 = require("./index");
(0, vitest_1.describe)('NullTracer', () => {
    (0, vitest_1.it)('startActiveSpan runs the callback and returns its value', () => {
        const t = new index_1.NullTracer();
        const result = t.startActiveSpan('test', span => {
            span.end();
            return 42;
        });
        (0, vitest_1.expect)(result).toBe(42);
    });
    (0, vitest_1.it)('span methods do not throw', () => {
        const t = new index_1.NullTracer();
        t.startActiveSpan('test', span => {
            (0, vitest_1.expect)(() => span.setAttributes({ key: 'val', count: 1 })).not.toThrow();
            (0, vitest_1.expect)(() => span.addEvent('event')).not.toThrow();
            (0, vitest_1.expect)(() => span.recordException(new Error('oops'))).not.toThrow();
            (0, vitest_1.expect)(() => span.setStatus({ code: 2 })).not.toThrow();
            (0, vitest_1.expect)(() => span.end()).not.toThrow();
        });
    });
});
(0, vitest_1.describe)('NullCounter', () => {
    (0, vitest_1.it)('add does not throw', () => {
        const c = new index_1.NullCounter();
        (0, vitest_1.expect)(() => c.add(1, { language: 'da' })).not.toThrow();
        (0, vitest_1.expect)(() => c.add(0)).not.toThrow();
    });
});
(0, vitest_1.describe)('NullLogger', () => {
    (0, vitest_1.it)('methods do not throw', () => {
        const l = new index_1.NullLogger();
        (0, vitest_1.expect)(() => l.info('hello')).not.toThrow();
        (0, vitest_1.expect)(() => l.warning('careful')).not.toThrow();
        (0, vitest_1.expect)(() => l.error('oops')).not.toThrow();
        (0, vitest_1.expect)(() => l.info('with attrs', { key: 'value' })).not.toThrow();
    });
});
(0, vitest_1.describe)('startTelemetry', () => {
    (0, vitest_1.it)('does not throw when called without a running collector', () => {
        // In CI/test there is no collector — the SDK starts but exports silently fail
        (0, vitest_1.expect)(() => (0, index_1.startTelemetry)({ instrumentations: [] })).not.toThrow();
    });
});
//# sourceMappingURL=index.test.js.map