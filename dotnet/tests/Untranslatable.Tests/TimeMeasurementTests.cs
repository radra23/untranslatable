using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using Untranslatable.Shared.Monitoring;
using Xunit;

namespace Untranslatable.Tests
{
    public class TimeMeasurementTests
    {
        [Fact]
        public void OverlappingTimers_EachRecordTheirOwnDuration()
        {
            using var meter = new Meter("TimeMeasurementTests.Overlap");
            var histogram = meter.CreateHistogram<double>("test_duration_ms", "ms");
            var recorded = new List<double>();

            using var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter == meter) l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<double>((_, value, _, _) => recorded.Add(value));
            listener.Start();

            var measurement = new TimeMeasurement(histogram);

            var first = measurement.StartTimer();
            Thread.Sleep(100);
            var second = measurement.StartTimer(); // overlaps the first, as concurrent requests do
            first.Dispose();
            second.Dispose();

            Assert.Equal(2, recorded.Count);
            Assert.True(recorded[0] >= 90, $"first timer recorded {recorded[0]}ms, expected ~100ms");
            Assert.True(recorded[1] < 90, $"second timer recorded {recorded[1]}ms, expected ~0ms");
        }

        [Fact]
        public void DisposingATimerTwice_RecordsOnce()
        {
            using var meter = new Meter("TimeMeasurementTests.DoubleDispose");
            var histogram = meter.CreateHistogram<double>("test_duration_ms", "ms");
            var count = 0;

            using var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter == meter) l.EnableMeasurementEvents(instrument);
            };
            listener.SetMeasurementEventCallback<double>((_, _, _, _) => count++);
            listener.Start();

            var timer = new TimeMeasurement(histogram).StartTimer();
            timer.Dispose();
            timer.Dispose();

            Assert.Equal(1, count);
        }
    }
}
