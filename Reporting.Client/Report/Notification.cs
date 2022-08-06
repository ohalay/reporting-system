using Amazon.S3;
using Amazon.S3.Model;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

public class NotificationHandler : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult
{
    private readonly IAmazonS3 _s3Client;
    private readonly Config _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationHandler> _logger;

    public NotificationHandler(
        IAmazonS3 amazonS3,
        IOptions<Config> options,
        IHttpClientFactory httpClientFactory,
        ILogger<NotificationHandler> logger)
    {
        _s3Client = amazonS3;
        _config = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost("api/notification")]
    public override async Task<ActionResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var snsMessage = JsonSerializer.Deserialize<SnsMessage>(Request.BodyReader.AsStream());
        if (snsMessage.Type == "SubscriptionConfirmation")
        {
            using var client = _httpClientFactory.CreateClient();
            await client.GetAsync(snsMessage.SubscribeURL, cancellationToken);

            _logger.LogInformation("Subscription confirmed");
        }

        else if (snsMessage.Type == "Notification" 
            && snsMessage.MessageAttributes.TryGetValue("api-key", out var attributeValue)
            && attributeValue.Value == _config.NotificationApiKey)
        {
            var model = JsonSerializer.Deserialize<NotificationModel>(snsMessage.Message);

            var url = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _config.BucketName,
                Expires = DateTime.UtcNow.AddDays(1),
                Key = $"{model.UserId}/{model.SuccessMessage}"
            });

            _logger.LogInformation("Notification received Url: {url}", url);
        }
        else
        {
            throw new InvalidOperationException($"Operation '{snsMessage.Type}' is not supported.");
        }

        return new StatusCodeResult(StatusCodes.Status202Accepted);
    }
}

public class SnsMessage
{
    public string Type { get; set; }
    public string Message { get; set; }
    public string SubscribeURL { get; set; }
    public Dictionary<string, AttributeValue> MessageAttributes { get; set; } = new Dictionary<string, AttributeValue>();

    public class AttributeValue
    {
        public string Value { get; set; }
    }
}

public class NotificationModel
{
    public Guid CommandId { get; set; }
    public string UserId { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
}
