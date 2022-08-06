using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

public class FunctionTests : IDisposable
{
    private BaseCommand? _command;
    private const bool DeleteReport = true;
    public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            Generator.PingReportCommand(ReportType.Pdf),
            Generator.PingReportCommand(ReportType.Xlsx)
        };

    [Theory]
    [MemberData(nameof(Data))]
    internal async Task TestLambda(BaseCommand command)
    {
        // Arrange
        _command = command;
        var publisher = new InMemoryPublisher();

        var sut = new Function(container => container
            .AddSingleton<IStorage, FileSystemStore>()
            .AddSingleton<INotifiactionPublisher>(publisher));

        // Act
        await sut.FunctionHandler(command.ToSqsEvent());

        // Assert
        var fileInfo = new FileInfo(command.ReportName);
        fileInfo.Exists.Should().BeTrue();
        fileInfo.Length.Should().BeGreaterThan(0);

        var notification = publisher.GetNotification(command.Id);
        notification.Should().NotBeNull();
        notification.Success.Should().BeTrue();
    }

    public void Dispose()
    {
        if (DeleteReport && _command is not null)
        {
            File.Delete(_command.ReportName);
        }
    }
}
