using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Untranslatable.Data;
using Xunit;

namespace Untranslatable.Tests;

/// <summary>
/// Unit tests for <see cref="WordsRepository"/>.
/// Uses the small fixture dataset (3 words: 2 PT, 1 DE).
/// </summary>
public class WordsRepositoryTests
{
    private static string FixturePath =>
        Path.Combine(AppContext.BaseDirectory, "content", "words.json");

    private static WordsRepository CreateRepository(string? path = null)
    {
        var settings = Options.Create(new WordsRepositorySettings
        {
            ContentFilePath = path ?? FixturePath,
        });
        return new WordsRepository(settings);
    }

    // -------------------------------------------------------------------------
    // GetByLanguage — no filter (returns all words)
    // -------------------------------------------------------------------------

    [Fact]
    public void GetByLanguage_NoFilter_ReturnsAllThreeWords()
    {
        var repo = CreateRepository();
        Assert.Equal(3, repo.GetByLanguage(null!).Count());
    }

    [Fact]
    public void GetByLanguage_NoFilter_WordsHaveAllProperties()
    {
        var repo = CreateRepository();
        foreach (var word in repo.GetByLanguage(null!))
        {
            Assert.NotNull(word.Language);
            Assert.NotNull(word.Word);
            Assert.NotNull(word.Meaning);
        }
    }

    // -------------------------------------------------------------------------
    // GetByLanguage — with language filter
    // -------------------------------------------------------------------------

    [Fact]
    public void GetByLanguage_ReturnsOnlyMatchingWords()
    {
        var repo = CreateRepository();
        var words = repo.GetByLanguage("pt").ToArray();
        Assert.Equal(2, words.Length);
        Assert.All(words, w => Assert.Equal("pt", w.Language));
    }

    [Fact]
    public void GetByLanguage_UnknownCode_ReturnsEmptySequence()
    {
        var repo = CreateRepository();
        Assert.Empty(repo.GetByLanguage("xx"));
    }

    [Fact]
    public void GetByLanguage_IsCaseSensitive()
    {
        var repo = CreateRepository();
        Assert.Empty(repo.GetByLanguage("PT"));
    }

    [Fact]
    public void GetByLanguage_SingleWordLanguage_ReturnsThatWord()
    {
        var repo = CreateRepository();
        var words = repo.GetByLanguage("de").ToArray();
        Assert.Single(words);
        Assert.Equal("Fernweh", words[0].Word);
    }

    // -------------------------------------------------------------------------
    // GetRandom
    // -------------------------------------------------------------------------

    [Fact]
    public void GetRandom_ReturnsNonNullWord()
    {
        var repo = CreateRepository();
        var word = repo.GetRandom();
        Assert.NotNull(word);
    }

    [Fact]
    public void GetRandom_ReturnedWordHasAllProperties()
    {
        var repo = CreateRepository();
        var word = repo.GetRandom();
        Assert.NotNull(word.Language);
        Assert.NotNull(word.Word);
        Assert.NotNull(word.Meaning);
    }

    [Fact]
    public void GetRandom_WithEmptyDataset_Throws()
    {
        var tmpFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tmpFile, "[]");
            var repo = CreateRepository(tmpFile);
            Assert.Throws<InvalidOperationException>(() => repo.GetRandom());
        }
        finally
        {
            File.Delete(tmpFile);
        }
    }
}
