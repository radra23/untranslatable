using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Untranslatable.Api.Controllers.Extensions;
using Untranslatable.Api.Models;
using Untranslatable.Data;
using Untranslatable.Shared.Monitoring;

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
            Metrics.Endpoints.WordsCounter.Add(1);
            using (Metrics.Endpoints.WordsTime.StartTimer())
            {
                var allWords = wordsRepository.GetByLanguage(language, cancellationToken);
                
                var result = allWords.Select(w => w.ToDto()).ToArray();
                return Ok(result);
            }
        }

        [HttpGet]
        [Route("random")]
        public ActionResult<UntranslatableWordDto> GetRandom(CancellationToken cancellationToken = default)
        {
            Metrics.Endpoints.WordRandom.Add(1);
            using (Metrics.Endpoints.WordRandomTime.StartTimer())
            {
                var word = wordsRepository.GetRandom(cancellationToken);

                return Ok(word.ToDto());
            }
        }
    }
}
