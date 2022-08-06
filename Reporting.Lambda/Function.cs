using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Reporting.Lambda;

public class Function
{
    private readonly IServiceProvider _serviceProvider;

    public Function()
    {
        _serviceProvider = Initializer
            .GetServiceCollection()
            .BuildServiceProvider();
    }

    internal Function(Action<IServiceCollection>? configure = null)
    {
        var collection = Initializer.GetServiceCollection();
        configure?.Invoke(collection);

        _serviceProvider = collection.BuildServiceProvider();
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="@event"></param>
    /// <returns></returns>
    public Task FunctionHandler(SQSEvent @event)
    {
       return _serviceProvider
           .GetRequiredService<CommandDispatcher>()
           .DispatchCommandAsync(@event.Records[0].Body, @event.Records[0].MessageAttributes["type"].StringValue);
    }
}