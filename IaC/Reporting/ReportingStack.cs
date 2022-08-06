using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SQS;
using Constructs;
using IaC.Abstractions;
using System.Collections.Generic;

namespace IaC.Reporting
{
    public class ReportingStack : Stack
    {
        public ReportingStack(Construct scope, string id, EnvStackProps props)
            : base(scope, id, props)
        {
            var config = this.GetConfiguration<ReportingConfig>(props);

            var sns = new Topic(
                this,
                $"{props}-topic",
                new TopicProps
                {
                    TopicName = $"{props}-notifications",
                });

            var subscription = new Subscription
                (
                    this,
                    $"{props}-portal-sub",
                    new SubscriptionProps
                    {
                        Topic = sns,
                        Endpoint = config.NotificationUrl,
                        Protocol = SubscriptionProtocol.HTTPS,
                    });

            var bucket = new Bucket(
                this,
                $"{props}-store-bucket",
                new BucketProps
                {
                    BucketName = $"{props}-store",
                    LifecycleRules = new[] { new LifecycleRule { Expiration = Duration.Hours(24) } },
                    RemovalPolicy = RemovalPolicy.DESTROY,
                });

            var commandDlq = new Queue(
                this,
                $"{props}-commands-dlq",
                new QueueProps
                {
                    QueueName = $"{props}-commands-dlq",
                    RetentionPeriod = Duration.Days(12),
                });

            var commandQueue = new Queue(
                this,
                $"{props}-commands",
                new QueueProps
                {
                    QueueName = $"{props}-commands",
                    RetentionPeriod = Duration.Days(1),
                    DeadLetterQueue = new DeadLetterQueue
                    {
                        MaxReceiveCount = 3,
                        Queue = commandDlq
                    },
                    VisibilityTimeout = Duration.Minutes(10),
                });

            var functionName = $"{props}-command-handler";
            var lambdaLogGroup = new LogGroup(
                this,
                $"{props}-command-handler-log",
                new LogGroupProps
                {
                    LogGroupName = $"/aws/lambda/{functionName}",
                    Retention = RetentionDays.ONE_MONTH,
                    RemovalPolicy = RemovalPolicy.DESTROY,
                });

            var dockerImageCode = DockerImageCode.FromImageAsset("../Reporting.Lambda");
            var dockerImageFunction = new DockerImageFunction(
                this,
                $"{props}-lambda",
                new DockerImageFunctionProps
                {
                    Code = dockerImageCode,
                    Description = "Generate pdf, xlsx, csv reports and save it to S3",
                    FunctionName = functionName,
                    Timeout = Duration.Minutes(10),
                    MemorySize = 512,
                    Environment = new Dictionary<string, string>
                    {
                        ["LogGroupName"] = lambdaLogGroup.LogGroupName,
                        ["BucketName"] = bucket.BucketName,
                        ["SsnTopicArn"] = sns.TopicArn,
                        ["NotificationApiKey"] = config.NotificationApiKey
                    },
                });


            bucket.GrantWrite(dockerImageFunction);
            lambdaLogGroup.GrantWrite(dockerImageFunction);
            sns.GrantPublish(dockerImageFunction);

            var eventSource = new SqsEventSource(
                commandQueue,
                new SqsEventSourceProps
                {
                    BatchSize = 1
                });
            dockerImageFunction.AddEventSource(eventSource);
        }
    }
}
