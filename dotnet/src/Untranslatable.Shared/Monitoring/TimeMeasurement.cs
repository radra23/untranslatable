using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;

namespace Untranslatable.Shared.Monitoring
{
    /// <summary>
    /// Records elapsed milliseconds into an OTel <see cref="Histogram{T}"/>.
    ///
    /// One instance is shared by every request to an endpoint, so it holds no
    /// timing state itself: each <see cref="StartTimer"/> call returns its own
    /// timer, and overlapping requests cannot overwrite each other's start time.
    /// </summary>
    public sealed class TimeMeasurement
    {
        private readonly Histogram<double> _histogram;

        public TimeMeasurement(Histogram<double> histogram) =>
            _histogram = histogram;

        /// <summary>Start a new timer; disposing it records the elapsed time once.</summary>
        public IDisposable StartTimer() => new Timer(_histogram);

        private sealed class Timer : IDisposable
        {
            private readonly Histogram<double> _histogram;
            private readonly long _startTimestamp = Stopwatch.GetTimestamp();
            private int _disposed;

            internal Timer(Histogram<double> histogram) => _histogram = histogram;

            public void Dispose()
            {
                // Record exactly once, even if Dispose is called again.
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                    return;

                _histogram.Record(Stopwatch.GetElapsedTime(_startTimestamp).TotalMilliseconds);
            }
        }
    }
}
