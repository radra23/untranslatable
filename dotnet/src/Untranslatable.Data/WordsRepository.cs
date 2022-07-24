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
            dataSource = new Lazy<WordsDataSource>(() => WordsDataSource.LoadFromFile(wordsRepositorySettings.Value.ContentFilePath), true);
        }

        private WordsDataSource DataSource => dataSource.Value;

        public IEnumerable<UntranslatableWord> GetByLanguage(string language = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(language))
                return DataSource.AllWords.ToArray();

            return DataSource.Words.ContainsKey(language)
                ? DataSource.Words[language].ToArray()
                : Enumerable.Empty<UntranslatableWord>();
        }
    }
}
