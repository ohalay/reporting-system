using ClosedXML.Excel;
using System.Data;

internal class DefaultExcelReportGenerator<TCommand> : IReportGenerator<TCommand>
    where TCommand : BaseCommand
{
    private readonly IDataProvider<TCommand> _dataProvider;

    public DefaultExcelReportGenerator(IDataProvider<TCommand> dataProvider)
        => _dataProvider = dataProvider;

    public ReportType ReportType => ReportType.Xlsx;

    public async Task<Stream> GenerateReportAsync(TCommand command, CancellationToken token)
    {
        var reportData = await _dataProvider.GetAsync<object>(command, token);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");


        using var dt = new DataTable();
        Array.ForEach(reportData.Headers, header => dt.Columns.Add(header.Name));

        foreach (var item in reportData)
        {
            var row = dt.NewRow();
            var itemType = item.GetType();
            Array.ForEach(
                reportData.Headers,
                header => row[header.Name] = itemType.GetProperty(header.PropertyName).GetFormattedValue(item));
            dt.Rows.Add(row);
        }

        worksheet.Cell(1, 1).InsertTable(dt);

        var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }
}
