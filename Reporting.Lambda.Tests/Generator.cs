using Amazon.Lambda.SQSEvents;
using System.Text.Json;

internal static class Generator
{
    public static object[] PingReportCommand(ReportType reportType)
       => new PingReportCommand
       {
           ReportType = reportType,
           UserId = "this_is_my_user_id",
       }.AsArray();

    internal static SQSEvent ToSqsEvent(this BaseCommand command)
        => new()
        {
            Records = new[]
            {
                new SQSEvent.SQSMessage
                {
                    Body = JsonSerializer.Serialize<object>(command),
                    MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>
                    {
                        ["type"] = new SQSEvent.MessageAttribute { StringValue = command.GetType().Name }
                    }
                }
            }.ToList(),
        };

    private static object[] AsArray(this BaseCommand command)
        => new object[] { command };


}
