using Microsoft.Extensions.DependencyInjection.Extensions;
using Oleander.Extensions.Configuration;
using Oleander.Extensions.Logging.Abstractions;
using Oleander.Extensions.Logging.Providers;

namespace Oleander.GpioBoard.WorkerService
{
    public class Program
    {
        public  static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureServices(services =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                    services
                        .AddSingleton((IConfigurationRoot)configuration)
                        .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                        .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));

                    services.AddHostedService<Worker>();

                }).ConfigureLogging((hostingContext, logging) =>
                {
                    logging
                        .ClearProviders()
                        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                        .Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerSinkProvider>());
                }).Build();

            host.Services.InitLoggerFactory();
            await host.RunAsync();
        }
    }
}