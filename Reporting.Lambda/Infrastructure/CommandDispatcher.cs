using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class CommandDispatcher
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<CommandDispatcher> _logger;

    public CommandDispatcher(IServiceProvider provider, ILogger<CommandDispatcher> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public Task DispatchCommandAsync(string message, string typeName)
    {
        var commandType = Type.GetType(typeName);
        if (commandType is null)
        {
            throw new NotSupportedException($"Command type '{typeName}' not sported");
        }

        if (JsonSerializer.Deserialize(message, commandType, getJsonSettings()) is not BaseCommand command)
        {
            throw new NotSupportedException($"Message can't be deserialized to command '{typeName}'.");
        }

        using var scope = _logger.BeginScope("CommandId = '{commandId}'", command.Id);

        var commandHandlerType = typeof(LoggingCommandHandler<>).MakeGenericType(commandType);

        var handler = _provider.GetRequiredService(commandHandlerType);
        var method = handler.GetType().GetMethod(nameof(ICommandHandler<BaseCommand>.HandleAsync));
        var result = method!.Invoke(handler, new object[] { command, CancellationToken.None }) as Task;

        return result!;

        static JsonSerializerOptions getJsonSettings()
        {
            var settings = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            settings.Converters.Add(new JsonStringEnumConverter());

            return settings;
        }

    }
}
