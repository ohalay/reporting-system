internal class PingReportCommand : BaseCommand
{
}

internal class PingReportDataProvider : IDataProvider<PingReportCommand>
{
    public Task<IReportData<TData>?> GetAsync<TData>(PingReportCommand command, CancellationToken token)
    {
        var data = new PingReportData { UserId = command.UserId, Message = "Pong", Created = command.Created };

        var result = new ReportData<PingReportData>(new[] { data })
        {
            Headers = new[] { ("UserId", "UserId"), ("Message", "Message"), ("Time", "Created") } 
        };

        return Task.FromResult(result as IReportData<TData>);
    }
}

internal class PingReportData
{
    public string UserId { get; set; }
    public string Message { get; set; }

    [Format("{0:hh:mm}")]
    public DateTime Created { get; set; }
}
