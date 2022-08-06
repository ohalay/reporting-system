using Microsoft.Extensions.Logging;

internal class CommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : BaseCommand
{
    private readonly ReportGeneratorStrategy<TCommand> _reportGeneratorStrategy;
    private readonly IStorage _storage;
    private readonly INotifiactionPublisher _publisher;

    public CommandHandler(
        ReportGeneratorStrategy<TCommand> reportGeneratorStrategy,
        IStorage storage,
        INotifiactionPublisher publisher)
    {
        _reportGeneratorStrategy = reportGeneratorStrategy;
        _storage = storage;
        _publisher = publisher;
    }

    public async Task HandleAsync(TCommand command, CancellationToken token = default)
    {
        var reportGenerator = _reportGeneratorStrategy.GetReportGenerator(command.ReportType);

        var reportStream = await reportGenerator.GenerateReportAsync(command, token);

        await _storage.SaveAsync(command, reportStream, token);

        var notification = command.ToNotification();
        notification.SuccessMessage = command.ReportName;

        await _publisher.PublishAsync(notification, token);
    }
}

internal class LoggingCommandHandler<TCommand> : ICommandHandler<TCommand>
    where TCommand : BaseCommand
{
    private readonly ICommandHandler<TCommand> _innerCommandHandler;
    private readonly ILogger _logger;

    public LoggingCommandHandler(ICommandHandler<TCommand> innerCommandHandler, ILogger<LoggingCommandHandler<TCommand>> logger)
    {
        _innerCommandHandler = innerCommandHandler;
        _logger = logger;
    }

    public async Task HandleAsync(TCommand command, CancellationToken token = default)
    {
       _logger.LogInformation("Start process command.");

        try
        {
            await _innerCommandHandler.HandleAsync(command, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }

        _logger.LogInformation("Finished process command.");
    }
}

