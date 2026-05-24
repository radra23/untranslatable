using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Untranslatable.Shared.Monitoring
{
    /// <summary>
    /// Wraps a <see cref="Stopwatch"/> and records elapsed milliseconds into
    /// an OTel <see cref="Histogram{T}"/> on <see cref="Dispose"/>.
    /// </summary>
    public sealed class TimeMeasurement : IDisposable
    {
        private Stopwatch? _stopwatch;
        private readonly Histogram<double> _histogram;

        public TimeMeasurement(Histogram<double> histogram) =>
            _histogram = histogram;

        /// <summary>Start the timer and return <c>this</c> for use in a <c>using</c> block.</summary>
        public IDisposable StartTimer()
        {
            _stopwatch = Stopwatch.StartNew();
            return this;
        }

        /// <summary>Elapsed time in milliseconds, or 0 if the timer was never started.</summary>
        public double ElapsedMs => _stopwatch?.Elapsed.TotalMilliseconds ?? 0;

        public void Dispose()
        {
            _stopwatch?.Stop();
            if (_stopwatch is not null)
                _histogram.Record(ElapsedMs);
        }
    }
}
