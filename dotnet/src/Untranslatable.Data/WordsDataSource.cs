using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Untranslatable.Data
{
    public class WordsDataSource : IWordsDataSource
    {
        public WordsDataSource(IDictionary<string, IEnumerable<UntranslatableWord>> words)
        {
            Words = words ?? throw new ArgumentNullException(nameof(words));
        }

        public IDictionary<string, IEnumerable<UntranslatableWord>> Words { get; }

        public IEnumerable<UntranslatableWord> AllWords => Words.Values.SelectMany(x => x);

        public static WordsDataSource LoadFromFile(string filePath)
        {
            var words = JsonDocument.Parse(File.ReadAllText(filePath)).Deserialize<IEnumerable<UntranslatableWord>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var wordsByLanguage = words.GroupBy(w => w.Language)
                .ToDictionary(g => g.Key, g => (IEnumerable<UntranslatableWord>)g.ToArray(), StringComparer.OrdinalIgnoreCase);

            return new WordsDataSource(wordsByLanguage);
        }
    }
}
