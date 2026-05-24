using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Untranslatable.Shared.Monitoring
{
    public static class Metrics
    {
        public static class App
        {
            public static readonly Counter<int> Start =
                MetricsFactory.Counter("app_start", "Number of times the app started.");

            public static readonly Counter<int> Stop =
                MetricsFactory.Counter("app_stop", "Number of times the app stopped.");
        }

        public static class Endpoints
        {
            public static readonly Counter<int> WordsCounter =
                MetricsFactory.Counter(
                    "app_requests_words_by_language_total",
                    "Number of calls to GET /words.");

            public static readonly TimeMeasurement WordsTime =
                MetricsFactory.TimeMeasurement(
                    "app_requests_words_by_language_duration_ms",
                    "Duration of GET /words calls in milliseconds.");

            public static readonly Counter<int> WordRandom =
                MetricsFactory.Counter(
                    "app_requests_word_random_total",
                    "Number of calls to GET /words/random.");

            public static readonly TimeMeasurement WordRandomTime =
                MetricsFactory.TimeMeasurement(
                    "app_requests_word_random_duration_ms",
                    "Duration of GET /words/random calls in milliseconds.");
        }

        // Exposed so Program.cs can register this meter with the OTel MeterProvider.
        public static string MeterName => MetricsFactory.MeterName;

        private static class MetricsFactory
        {
            private static readonly Assembly Assemb = Assembly.GetExecutingAssembly();
            private static readonly Meter _meter =
                new(Assemb.FullName!, Assemb.GetName().Version?.ToString());

            private static readonly ConcurrentDictionary<string, object> _instruments = new();

            public static string MeterName => Assemb.FullName!;

            public static Counter<int> Counter(string name, string description) =>
                (Counter<int>)_instruments.GetOrAdd(
                    name,
                    _ => _meter.CreateCounter<int>(name, "{calls}", description));

            public static TimeMeasurement TimeMeasurement(string name, string description) =>
                new(Histogram(name, description));

            private static Histogram<double> Histogram(string name, string description) =>
                (Histogram<double>)_instruments.GetOrAdd(
                    name,
                    _ => _meter.CreateHistogram<double>(name, "ms", description));
        }
    }
}
