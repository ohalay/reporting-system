using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

internal class SnsPublisher : INotifiactionPublisher
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly Config _config;

    public SnsPublisher(
        IAmazonSimpleNotificationService snsClient,
        IOptions<Config> options)
    {
        _snsClient = snsClient;
        _config = options.Value;
    }

    public async Task PublishAsync(Notification notification, CancellationToken token)
    {
        var request = new PublishRequest
        {
            TopicArn = _config.SsnTopicArn,
            Message = JsonSerializer.Serialize(notification),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["api-key"] = new MessageAttributeValue 
                {
                    DataType = "String",
                    StringValue = _config.NotificationApiKey
                }
            }
        };

        await _snsClient.PublishAsync(request, token);
    }
}