using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Options;

namespace Untranslatable.Data
{
    public class WordsRepository : IWordsRepository
    {
        private readonly Lazy<WordsDataSource> dataSource;

        public WordsRepository(IOptions<WordsRepositorySettings> wordsRepositorySettings)
        {
            dataSource = new Lazy<WordsDataSource>(
                () => WordsDataSource.LoadFromFile(wordsRepositorySettings.Value.ContentFilePath),
                isThreadSafe: true);
        }

        private WordsDataSource DataSource => dataSource.Value;

        public IEnumerable<UntranslatableWord> GetByLanguage(
            string language = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(language))
                return DataSource.AllWords.ToArray();

            return DataSource.Words.TryGetValue(language, out var words)
                ? words.ToArray()
                : Enumerable.Empty<UntranslatableWord>();
        }

        public UntranslatableWord GetRandom(CancellationToken cancellationToken = default)
        {
            // Materialise once to avoid double-enumeration and use thread-safe Random.Shared.
            var allWords = DataSource.AllWords.ToArray();

            if (allWords.Length == 0)
                throw new InvalidOperationException("No words available in the data source.");

            return allWords[Random.Shared.Next(allWords.Length)];
        }
    }
}
