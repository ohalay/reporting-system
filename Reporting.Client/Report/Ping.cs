using Amazon.SQS;
using Amazon.SQS.Model;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

public class PingSender : EndpointBaseAsync
    .WithRequest<PingReportCommand>
    .WithActionResult
{
    private readonly IAmazonSQS _sqsClient;
    private readonly Config _config;

    public PingSender(IAmazonSQS amazonSQS, IOptions<Config> options)
    {
        _sqsClient = amazonSQS;
        _config = options.Value;
    }

    [HttpPost("api/ping-command")]
    public override async Task<ActionResult> HandleAsync(PingReportCommand request, CancellationToken cancellationToken = default)
    {
        await _sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = _config.SqsUrl,
            MessageAttributes = new Dictionary<string, MessageAttributeValue> 
            {
                ["type"] = new MessageAttributeValue { StringValue = request.GetType().Name, DataType = "String" }
            },
            MessageBody = JsonSerializer.Serialize(request)
        }, cancellationToken);

        return new StatusCodeResult(StatusCodes.Status201Created);
    }
}

public enum ReportType { Csv = 1, Xlsx = 2, Pdf = 3 }

public class BaseCommand
{
    public ReportType ReportType { get; set; } = ReportType.Csv;
    public string UserId { get; set; }
    public int Version { get; init; } = 1;
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime Created { get; init; } = DateTime.UtcNow;
}

public class PingReportCommand : BaseCommand {}
