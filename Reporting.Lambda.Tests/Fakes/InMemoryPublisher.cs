internal class InMemoryPublisher : INotifiactionPublisher
{
    private readonly Dictionary<Guid, Notification> _store = new Dictionary<Guid, Notification>();
    
    public Task PublishAsync(Notification notification, CancellationToken token)
    {
        _store.Add(notification.CommandId, notification);

        return Task.CompletedTask;
    }

    internal Notification? GetNotification(Guid commandId)
        => _store.TryGetValue(commandId, out var notification)
        ? notification
        : default;

}