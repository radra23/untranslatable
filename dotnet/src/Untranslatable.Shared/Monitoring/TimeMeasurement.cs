using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Untranslatable.Shared.Monitoring
{
    public class TimeMeasurement : IDisposable
    {
        private Stopwatch stopwatch;
        private readonly Histogram<int> histogram;

        public TimeMeasurement(Histogram<int> histogram)
        {
            this.histogram = histogram;
        }

        public int GetTime() => (int)(this.stopwatch?.Elapsed.TotalSeconds ?? 0);

        public IDisposable StartTimer()
        {
            this.stopwatch = Stopwatch.StartNew();
            return this;
        }

        public void Dispose()
        {
            this.stopwatch?.Stop();
            if (this.stopwatch is not null)
                histogram.Record(GetTime());
        }
    }
}
