using System.Collections.Generic;
using System.Threading;

namespace Untranslatable.Data
{
    public interface IWordsRepository
    {
        public IEnumerable<UntranslatableWord> GetByLanguage(string language, CancellationToken cancellationToken = default);
        UntranslatableWord GetRandom(CancellationToken cancellationToken = default);
    }
}
