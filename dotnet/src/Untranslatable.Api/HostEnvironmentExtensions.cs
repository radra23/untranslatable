using System.IO;
using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace Untranslatable.Api
{
    public static class HostEnvironmentExtensions
    {
        public static void ConfigureContentPath(this IHostEnvironment hostEnvironment)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            hostEnvironment.ContentRootPath = path;
            Directory.SetCurrentDirectory(path);
        }
    }
}
