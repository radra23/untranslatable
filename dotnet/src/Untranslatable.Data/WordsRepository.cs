using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Options;

namespace Untranslatable.Data
{
    public class WordsRepository : IWordsRepository
    {
        private static readonly Random random = new((int)DateTime.Now.Ticks);

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

        public UntranslatableWord GetRandom(CancellationToken cancellationToken = default)
        {
            var nextIndex = random.Next(DataSource.AllWords.Count());
            return DataSource.AllWords.ElementAt(nextIndex);
        }
    }
}
