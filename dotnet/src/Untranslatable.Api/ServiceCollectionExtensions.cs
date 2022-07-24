using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Untranslatable.Data;

namespace Untranslatable.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllApplicationServices(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            return serviceCollection
                .AddSingleton<IWordsRepository, WordsRepository>()
                .Configure<WordsRepositorySettings>(configuration.GetSection("words"))
                .AddSingleton<IWordsDataSource, WordsDataSource>(sp => WordsDataSource.LoadFromFile(sp.GetRequiredService<WordsRepositorySettings>().ContentFilePath));
        }
    }
}
