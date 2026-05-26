using System;

namespace Untranslatable.Api.Monitoring
{
    /// <summary>
    /// No-op implementation of <see cref="IWordsTelemetry"/>.
    /// Used when OTel is disabled or failed to initialise.
    /// Every member is a guaranteed safe no-op — nothing is recorded,
    /// nothing is thrown.
    /// </summary>
    public sealed class NullWordsTelemetry : IWordsTelemetry
    {
        public static readonly NullWordsTelemetry Instance = new();
        private NullWordsTelemetry() { }

        public IDisposable BeginGetByLanguage(string? language) => NullScope.Instance;
        public IDisposable BeginGetRandom() => NullScope.Instance;
        public void RecordWordsReturned(string language, int count) { }
        public void RecordRandomWordReturned(string language) { }
    }

    /// <summary>A disposable that does nothing. Shared singleton; allocation-free.</summary>
    internal sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        private NullScope() { }
        public void Dispose() { }
    }
}
