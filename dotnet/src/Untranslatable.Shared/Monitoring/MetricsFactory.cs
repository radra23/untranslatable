using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Untranslatable.Shared.Monitoring
{
    public static class Metrics
    {
        public static class App
        {
            public static Counter<int> Start = MetricsFactory.Counter("app_start", "The number of times the app started.");
            public static Counter<int> Stop = MetricsFactory.Counter("app_stop", "The number of times the app stopped.");
        }

        public static class Endpoints
        {
            public static Counter<int> WordsCounter = MetricsFactory.Counter("app_requests_words_by_language_counter", "The number of times the endpoint /words is called.");
            public static TimeMeasurement WordsTime = MetricsFactory.TimeMeasurement("app_requests_words_by_language_time", "The number of times the endpoint /words is called.");
            public static Counter<int> WordRandom = MetricsFactory.Counter("app_requests__word_random_counter", "The number of times the endpoint /words/random is called.");
            public static TimeMeasurement WordRandomTime = MetricsFactory.TimeMeasurement("app_requests_word_random_time", "The number of times the endpoint /words/random is called.");
        }

        private static class MetricsFactory
        {
            private static Assembly Assemb => Assembly.GetExecutingAssembly();
            private static readonly Meter _meter = new(MeterName, Assemb.GetName().Version.ToString());

            private static readonly ConcurrentDictionary<string, object> _collectors = new();

            public static string MeterName => Assemb.FullName;

            public static Counter<int> Counter(string name, string description)
            {
                return (Counter<int>)_collectors.GetOrAdd(name, n => _meter.CreateCounter<int>(name, "times", description));
            }

            public static TimeMeasurement TimeMeasurement(string name, string description) => new(Histogram(name, description));

            public static Histogram<int> Histogram(string name, string description)
            {
                return (Histogram<int>)_collectors.GetOrAdd(name, n => _meter.CreateHistogram<int>(name, "seconds", description));
            }
        }
    }
}
