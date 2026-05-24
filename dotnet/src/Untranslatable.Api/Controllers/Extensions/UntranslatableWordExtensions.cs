using Untranslatable.Api.Models;
using Untranslatable.Data;

namespace Untranslatable.Api.Controllers.Extensions
{
    internal static class UntranslatableWordExtensions
    {
        public static UntranslatableWordDto? ToDto(this UntranslatableWord? word) =>
            word is null
                ? null
                : new UntranslatableWordDto
                {
                    Language = word.Language,
                    Word = word.Word,
                    Meaning = word.Meaning,
                };
    }
}
