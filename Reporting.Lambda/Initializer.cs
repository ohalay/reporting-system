using Amazon.S3;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Formatting.Compact;

internal class Initializer
{
    internal static IServiceCollection GetServiceCollection()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var collection = new ServiceCollection();

        return collection
            .Configure<Config>(options => configuration.Bind(options))
            .AddSingleton(typeof(ReportGeneratorStrategy<>))
            .AddSingleton(typeof(IReportGenerator<>), typeof(DefaultCsvReportGenerator<>))
            .AddSingleton(typeof(IReportGenerator<>), typeof(DefaultExcelReportGenerator<>))
            .AddSingleton(typeof(IReportGenerator<>), typeof(DefaultPdfReportGenerator<>))
            .AddSingleton<IStorage, S3Store>()
            .AddSingleton<INotifiactionPublisher, SnsPublisher>()
            .AddSingleton(typeof(ICommandHandler<>), typeof(CommandHandler<>))
            .AddSingleton(typeof(LoggingCommandHandler<>))
            .AddSingleton<IDataProvider<PingReportCommand>, PingReportDataProvider>()
            .AddSingleton<CommandDispatcher>()
            .AddSingleton<IAmazonS3>(_ => new AmazonS3Client())
            .AddSingleton<IAmazonSimpleNotificationService>(_ => new AmazonSimpleNotificationServiceClient())
            .AddLogging(logBuilder =>
            {
                var logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console(new CompactJsonFormatter())
                    .Enrich.WithProperty("Application", typeof(Initializer).Namespace)
                    .CreateLogger();

                logBuilder.AddSerilog(logger);
            });
    }
}

