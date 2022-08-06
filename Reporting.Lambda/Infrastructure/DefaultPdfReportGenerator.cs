using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

internal class DefaultPdfReportGenerator<TCommand> : IReportGenerator<TCommand>
    where TCommand : BaseCommand
{
    private const string FONT = "Helvetica";
    private const byte COLUMNS_AMOUNT = 6; // maximum amount of accepted columns in a4 format
    private const float COLUMN_WIDTH = 120.3f;

    private readonly IDataProvider<TCommand> _dataProvider;

    public DefaultPdfReportGenerator(IDataProvider<TCommand> dataProvider)
        => _dataProvider = dataProvider;

    public ReportType ReportType => ReportType.Pdf;

    public async Task<Stream> GenerateReportAsync(TCommand command, CancellationToken token)
    {
        var reportData = await _dataProvider.GetAsync<object>(command, token);

        var memoryStream = new MemoryStream();

        FontManager.RegisterFont(File.OpenRead($"assets/{FONT}.ttf"));
        FontManager.RegisterFont(File.OpenRead($"assets/{FONT}-Bold.ttf"));


        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(
                    width: PageSizes.A4.Landscape().Width + (reportData!.Headers.Length > COLUMNS_AMOUNT ? COLUMN_WIDTH * (reportData!.Headers.Length - COLUMNS_AMOUNT) : 0),
                    height: PageSizes.A4.Landscape().Height,
                    unit: Unit.Point);

                page.Margin(50);
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        Array.ForEach(reportData.Headers, _ => columns.RelativeColumn());
                    });

                    table.Header(header =>
                    {
                        Array.ForEach(reportData.Headers, prop => header.Cell().Element(CellStyle).Text(prop.Name));

                        static IContainer CellStyle(IContainer container)
                            => container
                            .DefaultTextStyle(x => x.FontFamily(FONT).SemiBold())
                            .PaddingVertical(5)
                            .BorderBottom(1)
                            .BorderColor(Colors.Black);
                    });

                    foreach (var item in reportData)
                    {
                        var itemType = item.GetType();

                        Array.ForEach(
                            reportData.Headers,
                            header => table.Cell().Element(CellStyle).Text(itemType.GetProperty(header.PropertyName).GetFormattedValue(item)));
                    }
                    static IContainer CellStyle(IContainer container)
                        => container
                        .BorderBottom(1)
                        .DefaultTextStyle(s => s.FontFamily(FONT))
                        .BorderColor(Colors.Grey.Lighten2)
                        .PaddingVertical(5);
                });
            });
        }).GeneratePdf(memoryStream);


        memoryStream.Position = 0;

        return memoryStream;
    }
}
