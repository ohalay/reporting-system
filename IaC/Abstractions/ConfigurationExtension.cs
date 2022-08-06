using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace IaC.Abstractions;

internal static class ConfigurationExtension
{
    internal static TConfig GetConfiguration<TConfig>(this Stack stack, EnvStackProps props)
        where TConfig : class
    {
        var feature = stack.Node.TryGetContext(props.FeatureName);
        if (feature is null || feature is not Dictionary<string, object> pair || !pair.ContainsKey(props.EnvironmentName))
            throw new ArgumentNullException($"Context variable '{props.FeatureName}' -> '{props.EnvironmentName}' missing in cdk.json");

        var configAsString = JsonSerializer.Serialize(pair[props.EnvironmentName]);

        return JsonSerializer.Deserialize<TConfig>(configAsString);
    }
}
