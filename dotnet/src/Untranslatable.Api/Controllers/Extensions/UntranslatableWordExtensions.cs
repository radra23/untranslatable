using Untranslatable.Api.Models;
using Untranslatable.Data;

namespace Untranslatable.Api.Controllers.Extensions
{
    internal static class UntranslatableWordExtensions
    {
        public static UntranslatableWordDto ToDto(this UntranslatableWord untranslatableWord)
        {
            if (untranslatableWord is null)
                return null;

            return new UntranslatableWordDto
            {
                Language = untranslatableWord.Language,
                Meaning = untranslatableWord.Meaning,
                Word = untranslatableWord.Word
            };
        }
    }
}
