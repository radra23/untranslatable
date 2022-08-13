using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
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
        private readonly Tracer tracer;

        public WordsController(Tracer tracer, IWordsRepository wordsRepository)
        {
            this.wordsRepository = wordsRepository;
            this.tracer = tracer;
        }

        [HttpGet]
        public ActionResult<UntranslatableWordDto> Get([FromQuery] string language = null, CancellationToken cancellationToken = default)
        {
            using var span = this.tracer?.StartActiveSpan("GetWordByLanguage");

            Metrics.Endpoints.WordsCounter.Add(1);
            using (Metrics.Endpoints.WordsTime.StartTimer())
            {
                var allWords = Enumerable.Empty<UntranslatableWord>();
                using (var childSpan1 = tracer.StartActiveSpan("GetByLanguageFromRepository"))
                {
                    childSpan1.AddEvent("Started loading words from file...");
                    allWords = wordsRepository.GetByLanguage(language, cancellationToken);
                    childSpan1.AddEvent("Finished loading words from file...");
                }
                using (tracer.StartActiveSpan("WordsToArray"))
                {
                    var result = allWords.Select(w => w.ToDto()).ToArray();
                    return Ok(result);
                }
            }
        }

        [HttpGet]
        [Route("random")]
        public ActionResult<UntranslatableWordDto> GetRandom(CancellationToken cancellationToken = default)
        {
            using var span = this.tracer?.StartActiveSpan("GetRandomWord");

            Metrics.Endpoints.WordRandom.Add(1);
            using (Metrics.Endpoints.WordRandomTime.StartTimer())
            {
                span.AddEvent("GetRandomWord");
                var word = wordsRepository.GetRandom(cancellationToken);
                span.AddEvent("Done select Random Word");

                return Ok(word.ToDto());
            }
        }
    }
}
