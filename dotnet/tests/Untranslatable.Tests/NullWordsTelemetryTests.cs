using Untranslatable.Api.Monitoring;
using Xunit;

namespace Untranslatable.Tests;

/// <summary>
/// Tests that <see cref="NullWordsTelemetry"/> is a true no-op:
/// every method completes without throwing, and the singleton contract holds.
/// </summary>
public class NullWordsTelemetryTests
{
    [Fact]
    public void Instance_IsSingleton()
    {
        Assert.Same(NullWordsTelemetry.Instance, NullWordsTelemetry.Instance);
    }

    [Fact]
    public void BeginGetByLanguage_DoesNotThrow()
    {
        using var scope = NullWordsTelemetry.Instance.BeginGetByLanguage("pt");
        // Disposing must also be silent.
        Assert.NotNull(scope);
    }

    [Fact]
    public void BeginGetByLanguage_NullLanguage_DoesNotThrow()
    {
        using var scope = NullWordsTelemetry.Instance.BeginGetByLanguage(null);
        Assert.NotNull(scope);
    }

    [Fact]
    public void BeginGetRandom_DoesNotThrow()
    {
        using var scope = NullWordsTelemetry.Instance.BeginGetRandom();
        Assert.NotNull(scope);
    }

    [Fact]
    public void RecordWordsReturned_DoesNotThrow()
    {
        // Should silently swallow any inputs.
        NullWordsTelemetry.Instance.RecordWordsReturned("pt", 2);
        NullWordsTelemetry.Instance.RecordWordsReturned("", 0);
    }

    [Fact]
    public void RecordRandomWordReturned_DoesNotThrow()
    {
        NullWordsTelemetry.Instance.RecordRandomWordReturned("de");
        NullWordsTelemetry.Instance.RecordRandomWordReturned("");
    }
}
