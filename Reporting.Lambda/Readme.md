# Reporting system AWS Lambda

## Implementation

![reporting-system](../docs/reporting-system-infra.drawio.svg "Reporting system diagram")

## Add new report

  To add new report we need:
1. Create new command, for example `PaymentReportCommand`
2. Implement command data provider interface `IDataProvider<PaymentReportCommand>`
3. Register command data provider `AddSingleton<IDataProvider<PaymentReportCommand>, PaymentDataProvider>()`
4. Create and register pdf generator, in case, default one is not good enough.

## Tests

  We using integration tests to test reports. Project responsible for testing `Reporting.Lambda.Tests`.
Basically, test run full flows and instead of saving to S3 - save stream to local machine, verify report and delete it.

**Hack**: for debugging porpoise we may disable removing file from local machine `DeleteReport = false` and open by file viewer.

## How to run locally

  We have two option to run locally
1. Debug using integration tests (fast way)
2. Debug using `Mock Lambda Test Tool` and use `SQS` or saved `test-request`

## How to deploy

### Pre requirements
1. AWS lambda deployed to VPC and don't have access to internet. In order to access to aws resources (SNS and S3) [VPC Endpoint](https://docs.aws.amazon.com/vpc/latest/privatelink/concepts.html) should be configured 
2. When you deploy it in a first you should confirm sns http subscribe from aws portal or manually
3. To run `cdk deploy`

### Regular deployment
We will using aws cdk command navigate to `IaC` folder and `cdk deploy --app 'cdk.out/' reporting-<environment> `, **where environment = dev | prod**


