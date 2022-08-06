using System.Collections;

internal enum ReportType { Csv = 1, Xlsx = 2, Pdf = 3 }
internal class BaseCommand
{
    public ReportType ReportType { get; set; }
    public string UserId { get; set; } 
    public int Version { get; init; }
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime Created { get; init; }

    public virtual string ReportName => $"{GetType().Name.Replace("Command", String.Empty)}-{Created:MMddyyyyHHmm}.{ReportType}";

    public virtual Notification ToNotification(bool success = true)
        => new()
        {
            CommandId = Id,
            UserId = UserId,
            Success = success,
        };
}

internal interface ICommandHandler<TCommand>
    where TCommand : BaseCommand
{
    Task HandleAsync(TCommand command, CancellationToken token = default);
}

internal interface IReportData<out TTable> : IEnumerable
{
    (string Name, string PropertyName)[] Headers { get; init; }
}

internal class ReportData<TTable> : IReportData<TTable>
{
    private readonly TTable[] _table;
    public ReportData(TTable[] table)
        => _table = table;
    public (string Name, string PropertyName)[] Headers { get; init; }

    public IEnumerator GetEnumerator()
        => _table.GetEnumerator();
}

internal interface IDataProvider<TCommand>
    where TCommand : BaseCommand
{
    public Task<IReportData<TTable>?> GetAsync<TTable>(TCommand command, CancellationToken token);
}

internal interface IReportGenerator<TCommand>
    where TCommand : BaseCommand
{
    ReportType ReportType { get; }
    Task<Stream> GenerateReportAsync(TCommand command, CancellationToken token);
}

internal class ReportGeneratorStrategy<TCommand>
    where TCommand : BaseCommand
{
    private readonly IEnumerable<IReportGenerator<TCommand>> _reportGenerators;

    public ReportGeneratorStrategy(IEnumerable<IReportGenerator<TCommand>> reportGenerators)
         => _reportGenerators = reportGenerators;
    public IReportGenerator<TCommand> GetReportGenerator(ReportType reportType)
        => _reportGenerators.LastOrDefault(s => s.ReportType == reportType)
            ?? throw new NotSupportedException();
}

internal interface IStorage
{
    Task SaveAsync(BaseCommand command, Stream stream, CancellationToken token);
}

public class Notification
{
    public Guid CommandId { get; init; }
    public string UserId { get; init; }
    public bool Success { get; init; }
    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
}

internal interface INotifiactionPublisher
{
    Task PublishAsync(Notification notification, CancellationToken token);
}