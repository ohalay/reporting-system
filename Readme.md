# Reporting demo
Presentation of serverless reporting system

## Business problem
- Big report is not working because of HTTP timeout
- Reports should be automatically deleted after 24h

## Architecture
![reporting-system-infra](docs/reporting-system-infra.drawio.svg "Reporting system infra diagram")

## Implementation
![reporting-system-flow](docs/reporting-system-flow.drawio.svg "Reporting system flow diagram")

- [Lambda](Reporting.Lambda/Readme.md)
- [Lambda Tests](Reporting.Lambda.Tests/FunctionTests.cs)
- [Client](Reporting.Client/requests.http)
- [IaC](IaC/Readme.md)

## Deployment

- [IaC](IaC/Readme.md)

## Demo

```
ngrok http 5182 --host-header=localhost
```