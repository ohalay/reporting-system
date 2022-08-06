using CsvHelper;
using System.ComponentModel;
using System.Globalization;

internal class DefaultCsvReportGenerator<TCommand> : IReportGenerator<TCommand>
    where TCommand : BaseCommand
{
    private readonly IDataProvider<TCommand> _dataProvider;

    public DefaultCsvReportGenerator(IDataProvider<TCommand> dataProvider)
        => _dataProvider = dataProvider;

    public ReportType ReportType => ReportType.Csv;

    public async Task<Stream> GenerateReportAsync(TCommand command, CancellationToken token)
    {
        var reportData = await _dataProvider.GetAsync<object>(command, token);

        var memoryStream = new MemoryStream();
        var writer = new StreamWriter(memoryStream, leaveOpen: true);
        using var csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);

        Array.ForEach(reportData.Headers, header => csvWriter.WriteField(header.Name));
        csvWriter.NextRecord();

        foreach (var item in reportData)
        {
            var itemType = item.GetType();
            Array.ForEach(
                reportData.Headers,
                header => csvWriter.WriteField(itemType.GetProperty(header.PropertyName).GetFormattedValue(item)));
            csvWriter.NextRecord();
        }

        writer.Flush();
        memoryStream.Position = 0;

        return memoryStream;
    }
}
