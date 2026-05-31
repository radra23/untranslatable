import { describe, it, expect } from 'vitest';
import { NullTracer, NullCounter, NullLogger, startTelemetry } from './index';

describe('NullTracer', () => {
  it('startActiveSpan runs the callback and returns its value', () => {
    const t = new NullTracer();
    const result = t.startActiveSpan('test', span => {
      span.end();
      return 42;
    });
    expect(result).toBe(42);
  });

  it('span methods do not throw', () => {
    const t = new NullTracer();
    t.startActiveSpan('test', span => {
      expect(() => span.setAttributes({ key: 'val', count: 1 })).not.toThrow();
      expect(() => span.addEvent('event')).not.toThrow();
      expect(() => span.recordException(new Error('oops'))).not.toThrow();
      expect(() => span.setStatus({ code: 2 })).not.toThrow();
      expect(() => span.end()).not.toThrow();
    });
  });
});

describe('NullCounter', () => {
  it('add does not throw', () => {
    const c = new NullCounter();
    expect(() => c.add(1, { language: 'da' })).not.toThrow();
    expect(() => c.add(0)).not.toThrow();
  });
});

describe('NullLogger', () => {
  it('methods do not throw', () => {
    const l = new NullLogger();
    expect(() => l.info('hello')).not.toThrow();
    expect(() => l.warning('careful')).not.toThrow();
    expect(() => l.error('oops')).not.toThrow();
    expect(() => l.info('with attrs', { key: 'value' })).not.toThrow();
  });
});

describe('startTelemetry', () => {
  it('does not throw when called without a running collector', () => {
    // In CI/test there is no collector — the SDK starts but exports silently fail
    expect(() => startTelemetry({ instrumentations: [] })).not.toThrow();
  });
});
