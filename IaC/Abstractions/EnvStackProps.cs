using Amazon.CDK;
using System.Collections.Generic;

namespace IaC.Abstractions;

public enum Env { dev, prod }

public class EnvStackProps: StackProps
{
    public EnvStackProps(string featureName)
        => FeatureName = featureName;

    public string FeatureName { get; private set; }
    public string EnvironmentName { get; private set; }


    public EnvStackProps WithEnvironment(Env environmentName)
    {
        EnvironmentName = environmentName.ToString();

        if (Tags is null)
            Tags = new Dictionary<string, string>(); 

        Tags.Add("feature", FeatureName);
        Tags.Add("env", EnvironmentName);

        return this;
    }
    public override string ToString()
        =>$"{FeatureName}-{EnvironmentName}";
}
