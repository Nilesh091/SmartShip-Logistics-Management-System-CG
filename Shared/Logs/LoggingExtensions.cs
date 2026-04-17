using Serilog;
using Microsoft.Extensions.Hosting;

namespace Shared.Logs
{
    public static class LoggingExtensions
    {
        public static IHostBuilder ConfigureSerilog(this IHostBuilder host, string serviceName)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", serviceName)
                .WriteTo.Console()
                .WriteTo.Seq("http://host.docker.internal:8081")
                .CreateLogger();

            return host.UseSerilog();
        }
    }
}