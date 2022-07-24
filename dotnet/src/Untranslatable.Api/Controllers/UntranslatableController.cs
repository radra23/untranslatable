using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Untranslatable.Api.Controllers.Extensions;
using Untranslatable.Api.Models;
using Untranslatable.Data;

namespace Untranslatable.Api.Controllers
{
    [ApiController]
    [Route("words")]
    [Produces("application/json")]
    public class WordsController : ControllerBase
    {
        private readonly IWordsRepository wordsRepository;

        public WordsController(IWordsRepository wordsRepository)
        {
            this.wordsRepository = wordsRepository;
        }

        [HttpGet]
        public ActionResult<UntranslatableWordDto> Get([FromQuery] string language = null, CancellationToken cancellationToken = default)
        {
            var allWords = wordsRepository.GetByLanguage(language, cancellationToken);
            var result = allWords.Select(w => w.ToDto()).ToArray();
            return Ok(result);
        }

        [HttpGet]
        [Route("random")]
        public ActionResult<UntranslatableWordDto> GetRandom(CancellationToken cancellationToken = default)
        {
            var word = wordsRepository.GetRandom(cancellationToken);

            return Ok(word.ToDto());
        }
    }
}
