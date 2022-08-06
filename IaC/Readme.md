# IaC deployment with CDK --language csharp

AWS has a concept of stack - a collection of AWS resources that you can manage as a single unit. In other words, you can create, update, or delete a collection of resources by creating, updating, or deleting stacks. All the resources in a stack are defined by the stack's AWS CloudFormation template.

We are split our features + environment to stacks

## Prerequisites

 - [Install Docker](https://docs.docker.com/desktop/windows/install/)
 - [Install AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
 - [Install Node.js & npm](https://nodejs.org/en/download)
 - [Install AWS CDK](https://docs.aws.amazon.com/cdk/v2/guide/getting_started.html#getting_started_prerequisites)
 - [Configure AWS CLI with MFA](https://aws.amazon.com/premiumsupport/knowledge-center/authenticate-mfa-cli/)


## Useful commands
* `cdk list` - get list of stack that we can deploy
* `cdk synth` - emits the synthesized CloudFormation template (convert C# to cloud formation)
* `cdk deploy --app 'cdk.out/' <stack-name>` deploy feature (resources with code) to environment 

**Note:** if you want to run `cdk deploy` command a few times you may need need to remove `cdk.out` folder.

### How to debug
To debug cdk project we need inject debugger to to `Program.cs` and run `cdk synth` command.
For example
```
 public static void Main()
        {
            // Debugger.Launch();

            new App()
                .AddReportingApp()
                .Synth();
        }
```

## Deployment

For deploy feature to appropriate environment, for example **prod** for **reporting** we should run commands:
1. `cdk list` - will return all stacks that we have. Stack template `<feature>-<environment>`. 
2. `cdk deploy --app 'cdk.out/' reporting-dev` - deploy aws resources with code for feature **reporting** to **dev** environment  .All specific configuration defined in `cdk.json`.

**!Important** : Please make sure that all configurations in `cdk.json` are filled before deploying.