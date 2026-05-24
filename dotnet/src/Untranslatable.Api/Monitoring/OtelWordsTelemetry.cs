using System;
using OpenTelemetry.Trace;
using Untranslatable.Shared.Monitoring;

namespace Untranslatable.Api.Monitoring
{
    /// <summary>
    /// OTel-backed implementation of <see cref="IWordsTelemetry"/>.
    ///
    /// Every public method is wrapped in try/catch so that an unexpected
    /// SDK exception (e.g. a buggy exporter, a mis-configured sampler) is
    /// swallowed and logged rather than surfaced to the caller. Business
    /// logic must never fail because of instrumentation.
    /// </summary>
    internal sealed class OtelWordsTelemetry : IWordsTelemetry
    {
        private readonly Tracer _tracer;

        public OtelWordsTelemetry(Tracer tracer) => _tracer = tracer;

        public IDisposable BeginGetByLanguage(string? language)
        {
            try
            {
                Metrics.Endpoints.WordsCounter.Add(1);
                var span = _tracer.StartActiveSpan("words.get_by_language");
                span.SetAttribute("language", language ?? "all");
                return new OperationScope(span, Metrics.Endpoints.WordsTime.StartTimer());
            }
            catch
            {
                return NullScope.Instance;
            }
        }

        public IDisposable BeginGetRandom()
        {
            try
            {
                Metrics.Endpoints.WordRandom.Add(1);
                var span = _tracer.StartActiveSpan("words.get_random");
                return new OperationScope(span, Metrics.Endpoints.WordRandomTime.StartTimer());
            }
            catch
            {
                return NullScope.Instance;
            }
        }

        public void RecordWordsReturned(string language, int count)
        {
            // Placeholder for future per-language result-count histogram.
            // Kept here so callers don't need to change when the metric is added.
        }

        public void RecordRandomWordReturned(string language)
        {
            // Placeholder for future per-language random-pick distribution metric.
        }

        /// <summary>
        /// Disposes the timer first (records the measurement) then the span
        /// (closes the trace). Each disposal is individually guarded.
        /// </summary>
        private sealed class OperationScope : IDisposable
        {
            private readonly IDisposable _span;
            private readonly IDisposable _timer;

            internal OperationScope(IDisposable span, IDisposable timer)
            {
                _span = span;
                _timer = timer;
            }

            public void Dispose()
            {
                try { _timer.Dispose(); } catch { /* telemetry must not throw */ }
                try { _span.Dispose(); } catch { /* telemetry must not throw */ }
            }
        }
    }
}
