using System;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Untranslatable.Api.Models;
using Untranslatable.Api.Monitoring;
using Xunit;

namespace Untranslatable.Tests;

/// <summary>
/// HTTP integration tests for the Words controller.
/// Boots the real application via <see cref="WebApplicationFactory{TEntryPoint}"/>
/// with two substitutions:
///   1. <see cref="IWordsTelemetry"/> → <see cref="NullWordsTelemetry"/> (no collector needed).
///   2. Words content file → fixture dataset (3 words: 2 PT, 1 DE).
/// </summary>
public class WordsControllerTests : IClassFixture<WordsControllerTests.Factory>
{
    public class Factory : WebApplicationFactory<Program>
    {
        private static readonly string FixturePath =
            Path.Combine(AppContext.BaseDirectory, "content", "words.json");

        protected override void ConfigureWebHost(
            Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace OTel telemetry with the no-op so no collector is required.
                services.Replace(ServiceDescriptor.Singleton<IWordsTelemetry>(
                    NullWordsTelemetry.Instance));
            });

            builder.ConfigureAppConfiguration((_, config) =>
            {
                // Point the repository at the test fixture file.
                config.AddInMemoryCollection(new[]
                {
                    new System.Collections.Generic.KeyValuePair<string, string?>(
                        "words:ContentFilePath", FixturePath),
                });
            });
        }
    }

    private readonly HttpClient _client;

    public WordsControllerTests(Factory factory)
    {
        _client = factory.CreateClient();
    }

    // -------------------------------------------------------------------------
    // GET /words — no filter
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetWords_NoFilter_Returns200()
    {
        var response = await _client.GetAsync("/words");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWords_NoFilter_ReturnsAllWords()
    {
        var words = await _client.GetFromJsonAsync<UntranslatableWordDto[]>("/words");
        Assert.NotNull(words);
        Assert.Equal(3, words.Length);
    }

    [Fact]
    public async Task GetWords_NoFilter_ResponseIsJson()
    {
        var response = await _client.GetAsync("/words");
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    // -------------------------------------------------------------------------
    // GET /words?language=pt
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetWords_FilterByLanguage_Returns200()
    {
        var response = await _client.GetAsync("/words?language=pt");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWords_FilterByLanguage_ReturnsOnlyMatchingWords()
    {
        var words = await _client.GetFromJsonAsync<UntranslatableWordDto[]>("/words?language=pt");
        Assert.NotNull(words);
        Assert.Equal(2, words.Length);
        Assert.All(words, w => Assert.Equal("pt", w.Language));
    }

    [Fact]
    public async Task GetWords_UnknownLanguage_ReturnsEmptyArray()
    {
        // .NET controller returns 200 + [] for unknown language (by design).
        var words = await _client.GetFromJsonAsync<UntranslatableWordDto[]>("/words?language=xx");
        Assert.NotNull(words);
        Assert.Empty(words);
    }

    // -------------------------------------------------------------------------
    // GET /words/random
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetRandomWord_Returns200()
    {
        var response = await _client.GetAsync("/words/random");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRandomWord_ReturnsWordWithAllFields()
    {
        var word = await _client.GetFromJsonAsync<UntranslatableWordDto>("/words/random");
        Assert.NotNull(word);
        Assert.NotNull(word.Language);
        Assert.NotNull(word.Word);
        Assert.NotNull(word.Meaning);
    }

    [Fact]
    public async Task GetRandomWord_ResponseIsJson()
    {
        var response = await _client.GetAsync("/words/random");
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    // -------------------------------------------------------------------------
    // GET /healthz
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Healthz_Returns200()
    {
        var response = await _client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
