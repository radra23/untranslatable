using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Untranslatable.Api.Controllers.Extensions;
using Untranslatable.Api.Models;
using Untranslatable.Api.Monitoring;
using Untranslatable.Data;

namespace Untranslatable.Api.Controllers
{
    [ApiController]
    [Route("words")]
    [Produces("application/json")]
    public class WordsController : ControllerBase
    {
        private readonly IWordsRepository _wordsRepository;
        private readonly IWordsTelemetry _telemetry;

        // IWordsTelemetry is injected — the controller has no knowledge of OTel.
        // If OTel is unavailable, the DI container provides a NullWordsTelemetry.
        public WordsController(IWordsRepository wordsRepository, IWordsTelemetry telemetry)
        {
            _wordsRepository = wordsRepository;
            _telemetry = telemetry;
        }

        [HttpGet]
        public ActionResult<UntranslatableWordDto[]> Get(
            [FromQuery] string? language = null,
            CancellationToken cancellationToken = default)
        {
            using var op = _telemetry.BeginGetByLanguage(language);

            var result = _wordsRepository
                .GetByLanguage(language, cancellationToken)
                .Select(w => w.ToDto())
                .ToArray();

            _telemetry.RecordWordsReturned(language ?? "all", result.Length);
            return Ok(result);
        }

        [HttpGet("random")]
        public ActionResult<UntranslatableWordDto> GetRandom(
            CancellationToken cancellationToken = default)
        {
            using var op = _telemetry.BeginGetRandom();

            var word = _wordsRepository.GetRandom(cancellationToken);
            _telemetry.RecordRandomWordReturned(word?.Language ?? string.Empty);
            return Ok(word.ToDto());
        }
    }
}
