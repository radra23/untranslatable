using System;

namespace Untranslatable.Api.Monitoring
{
    /// <summary>
    /// Telemetry facade for word-endpoint operations.
    ///
    /// Controllers depend on this interface, not on any OTel type directly.
    /// Implementations are free to use OTel, StatsD, no-op stubs, or anything
    /// else — the controller never needs to change when the provider changes.
    ///
    /// Every method must be safe to call even when the underlying provider is
    /// unavailable; implementations must never let a telemetry failure
    /// propagate to the caller.
    /// </summary>
    public interface IWordsTelemetry
    {
        /// <summary>
        /// Begins a traced and timed scope for a GET /words request.
        /// Dispose the returned value when the request completes.
        /// </summary>
        IDisposable BeginGetByLanguage(string? language);

        /// <summary>
        /// Begins a traced and timed scope for a GET /words/random request.
        /// Dispose the returned value when the request completes.
        /// </summary>
        IDisposable BeginGetRandom();

        /// <summary>Records how many words were returned and for which language.</summary>
        void RecordWordsReturned(string language, int count);

        /// <summary>Records that a random word was returned for a given language.</summary>
        void RecordRandomWordReturned(string language);
    }
}
