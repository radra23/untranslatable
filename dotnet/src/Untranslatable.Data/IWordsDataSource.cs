using System.Collections.Generic;

namespace Untranslatable.Data
{
    public interface IWordsDataSource
    {
        public IDictionary<string, IEnumerable<UntranslatableWord>> Words { get; }
    }
}
