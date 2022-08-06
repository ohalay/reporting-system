using Amazon.CDK;
using IaC.Abstractions;

namespace IaC.Reporting;

internal static class ReportingApp
{
    private const string FEATURE_NAME = "demo-reporting";

    public static App AddReportingApp(this App app)
    {
        var props = new EnvStackProps(FEATURE_NAME);

        new ReportingStack(app, $"{props.FeatureName}-{Env.dev}", props.WithEnvironment(Env.dev));

        return app;
    }
}
