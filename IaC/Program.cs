using Amazon.CDK;
using IaC.Reporting;

namespace IaC
{
    sealed class Program
    {
        public static void Main()
        {
            // Debugger.Launch();

            new App()
                .AddReportingApp()
                .Synth();
        }
    }
}
