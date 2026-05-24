using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Untranslatable.Data;

namespace Untranslatable.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services
                // WordsRepository settings (file path to the word list)
                .Configure<WordsRepositorySettings>(configuration.GetSection("words"))
                // Repository lazily loads data from the JSON file on first access
                .AddSingleton<IWordsRepository, WordsRepository>();
        }
    }
}
