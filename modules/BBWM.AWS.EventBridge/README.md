# AWS Event Bridge

## Abstract

AWS EventBridge module (front-end is [here](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/tree/develop/project%2FBBWT.Client%2Fsrc%2Fapp%2Fmain%2Faws-event-bridge))
allows us to manage recurring jobs with the help of Amazon EventBridge, which release the server from using resources
for scheduling/triggering them.

Start using this module is as simple as implementing an interface so the module can discover the jobs. All discovered
jobs are eligible for administration through the UI (app/aws-event-bridge/jobs). Whenever the mentioned interface is
implemented, aside from the execution logic, other useful information is provided for successful administration such
as: the job Id, description and metadata about the parameters the job is expecting to receive as input at runtime.

By parameterizing the jobs a good level of abstraction can be achieved, depending on the end-logic of course, as it's
possible to use the same job for different tasks. For example, we can have a job that update database records based on
input parameters. Jobs are integrated with the DI system so you can inject any service you need into your job's
constructor to carry out the task (for example, the database context).

One important thing to note is that all jobs, especially those which are intended for long runs, should be aware that
cancellation can be requested at any time either by the user through the UI or by the module itself (e.g., when the app
is tearing down). The module notifies about cancellation via a cancellation token provided at runtime.

The module keeps track of every job run so it's able to display reports of succeed/failed/cancelled jobs as well as
running jobs. Additional actions can be performed from these reports such as re-start a job or request cancellation for
a running job. Optionally, the module can notify about failed/cancelled jobs so further actions can be taken (e.g.,
sending an email).

## AWS &amp; GitLab Setup

1. Create the AWS Role including the policies below, once you do so, copy the ARN value and put it on a note since you will use it later to
set up the event bridge configuration in the Gitlab variable that is going to be used for deployment


    | Policy Name                                  | Type             |
    |----------------------------------------------|------------------|
    | AdditionalAWSEventBridgePermissionsRequired  | Customer Managed |
    | CloudWatchEventsBuiltInTargetExecutionAccess | AWS Managed      |
    | CloudWatchEventsInvocationAccess             | AWS Managed      |


    **A very important note.**
    While creating the Role, you will have to choose the "Trusted entity type".
    Be sure that you're choosing the _"AWS service"_ and _"CloudWatch Events"_ use case.

    After the Role is created, you can check that it has correct type by looking at the new role in the list.
    The column _"Trusted entities"_ must have _"AWS Service: events"_ value for it.


2. The first policy should be set up as below with the permissions described in the json file below:

    | Service            | Access Level   |
    |--------------------|----------------|
    | EventBridge        | Full Access    |
    | EventBridgeSchemas | Full Access    |
    | IAM                | Limited: Write |
    | KMS                | Limited: Write |
    | System Manager     | Limited: Read  |


    ```json
    {
      "Version": "2012-10-17",
      "Statement": [
        {
          "Sid": "VisualEditor0",
          "Effect": "Allow",
          "Action": [
             "schemas:*",
             "iam:CreateServiceLinkedRole",
             "kms:GenerateDataKey",
             "events:*"
          ],
          "Resource": "*"
        },
        {
          "Sid": "VisualEditor1",
          "Effect": "Allow",
          "Action": "ssm:GetParametersByPath",
          "Resource": "arn:aws:ssm:*:297649722856:parameter/*"
        }
      ]
    }
    ```

3. The second policy should be set up as below:

    | Service                                      | Access Level               |
    |----------------------------------------------|----------------------------|
    | EC2                                          | Limited: List, Read, Write |

    ```json
    {
      "Version": "2012-10-17",
      "Statement": [
        {
          "Sid": "CloudWatchEventsBuiltInTargetExecutionAccess",
          "Effect": "Allow",
          "Action": [
            "ec2:Describe*",
            "ec2:RebootInstances",
            "ec2:StopInstances",
            "ec2:TerminateInstances",
            "ec2:CreateSnapshot"
          ],
          "Resource": "*"
        }
      ]
    }
    ```

4. The third policy should be set up as below:

    | Service | Access Level   |
    |---------|----------------|
    | Kinesis | Limited: Write |

    ```json
    {
      "Version": "2012-10-17",
      "Statement": [
        {
          "Sid": "CloudWatchEventsInvocationAccess",
          "Effect": "Allow",
          "Action": [
            "kinesis:PutRecord"
          ],
          "Resource": "*"
        }
      ]
    }
    ```


5. GitLab Setup

    Open the Gitlab project on which you would like to set up AWS Event Bridge for and edit the variable with the environments (e.g. **ENV\_TEST\_LINUX**) by adding the following lines (settings required):

    AwsEventBridgeSettings\_\_APIKey=CC55F3F5\-F55F\-43F8\-A90E\-3F8D34211FC5
    AwsEventBridgeSettings\_\_TargetRoleArn=arn:aws:iam::297649722856:role/SeveralEventBridgeRequiredPermissions
    AwsEventBridgeSettings\_\_ApiConnectionName=MyProject\-test\-linux\-RunJob\-Connection
    AwsEventBridgeSettings\_\_ApiDestinationName=MyProject\-test\-linux\-RunJob\-ApiDestination

    In the appsettings.json file the above settings would look like:

    ```json
    {
        "AwsEventBridgeSettings": {
            "APIKey": "CC55F3F5-F55F-43F8-A90E-3F8D34211FC5",
            "TargetRoleArn": "arn:aws:iam::297649722856:role/SeveralEventBridgeRequiredPermissions",
            "ApiConnectionName": "MyProject-test-linux-RunJob-Connection",
            "ApiDestinationName": "MyProject-test-linux-RunJob-ApiDestination"
        }
    }
    ```

    Note that:

    + The above values/settings are a just **examples**
    + The **APIKey** is what it's used to authenticate requests coming from AWS (to start jobs) so this value should be hard-to-guess. We can use a GUID generator tool like [this one](https://www.guidgenerator.com/online-guid-generator.aspx) for this purpose
    + **TargetRoleArn** is the value we noted down in step one
    + **ApiConnectionName** and **ApiDestinationName** are objects that will be reused whenever possible by the module, so these names should be guaranteed to be unique for each project (specially when several projects are using the same account information). We propose to use the following name pattern: \<project\-name\>\-\<environment\>\-\<object\-name\> for example: BBWT3\-test\-linux\-RunJob\-ApiDestination and BBWT3\-test\-linux\-RunJob\-Connection
    + A brief description of all settings used by the module can be found at [appsettings.schema.json](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/blob/develop/project/BBWT.Server/appsettings.schema.json) under the section **AwsEventBridgeSettings**.


    We can optionally set up an additional setting with the authorization header name where the **APIKey** will come in the request sent by AWS. This setting is **AwsEventBridgeSettings\_\_AuthHeader** and it defaults to **X\-Aws\-Event\-Bridge\-Api\-Key**.

6. Grant the permissions below to the S3 user specified in the appsettings.env file associated to the environment that it is being modified

    | Policy Name                                 | Policy Type        |
    |---------------------------------------------|--------------------|
    | AdditionalAWSEventBridgePermissionsRequired | Managed Policy     |
    | AmazonEventBridgeFullAccess                 | AWS Managed Policy |
    | AmazonEventBridgeSchemasFullAccess          | AWS Managed Policy |

7. Run the pipeline associated to the environment you are modifying and check on the web server that the /mnt/data/webroot
/bbwt3-app/appsettings.env file gets populated correctly with the settings you have specified in the GitLab variable


## Development and local testing

The following procedure relies on an existing AWS account with the EventBridge already set up as explained above. Other solutions for local testing like localstack might work but we haven't used them. For local development and testing the shared account details under BitWarden items **BBWT3 - Development AWS settings** and **BBWT3 - AWS Event Bridge DEVELOPMENT details** can be used.


> Before going further we suggest to use the same pattern for naming the AWS objects. For example, **BBWT3\-Development\-RunJob\-ApiDestination** and **BBWT3\-Development\-RunJob\-Connection**

### Forwarding Internet requests into the app

As we are going to be receiving requests from AWS for starting jobs we need a way to forward those Internet requests into our local machines. For this we used the `ngrok` tool. Create a free account on their [site](https://dashboard.ngrok.com/signup) and then copy the AuthToken to set up `ngrok` locally:

```cmd
ngrok config add-authtoken <auth-token>
```

Once we have `ngrok` configured we can start forwarding Internet requests into our application with the following command:

```cmd
ngrok http http://localhost:50815 --host-header="localhost:50815"
```

After you run this command  `ngrok` will provide you with two URLs, use the HTTPS URL for configuring the environment variable **EB_TARGET_URL** at project\\BBWT.Server\\Properties\\launchSettings.json and then start the app.

_Note: Given that the module will reuse AWS objects you need to drop previous objects manually before starting the app because `ngrok` will provide different URLs in the free version everytime it's run._

### Coding a sample job

To create a new job we need to implement the interface `IEventBridgeJob`. By implmenting this interface we provide metadata about the job like name/id, description and parameters expected.

Suppose we want a job for updating all orders in the database by increasing **SomeColumn** by the given value. We can implement a class/job like the following:

```c#
public class UpdateOrdersJob : IEventBridgeJob
{
    private readonly IDataContext dataContext;

    public string JobId => "Update Orders' SomeColumn Job";

    public string JobDescription => "This job will increment all orders' SomeColumn by the provided value";

    public List<JobParameterInfo> Parameters => new List<JobParameterInfo> {
            new JobParameterInfo {
                Name = "Increment",
                Required = true,
                Description = "How much an order's SomeColumn will be increased"
            }
        };

    public UpdateOrdersJob(IDataContext dataContext) => this.dataContext = dataContext;

    public async Task RunAsync(
        IEnumerable<AwsEventBridgeJobParameterDTO> @params,
        CancellationToken ct)
    {
        var incrementParam = @params.Where(p => p.Name == "Increment").Select(p => p.Value).FirstOrDefault();

        if (!int.TryParse(incrementParam, out var incrementValue))
            throw new Exception("Increment param is missing or invalid.");

        await dataContext.Orders.UpdateFromQueryAsync(order => new()
        {
            SomeColumn = order.SomeColumn + incrementParam
        });
    }
}

```

The job metadata is collected at app startup so now we just need to set up a rule/job in the administration UI. Note that as the implemented job provide parameters it's possible to create several rules targetting the same job.

To create a rule for the job go to the **Operational Admin \-\> AWS Scheduler \-\> Jobs** page and click the **Add** button. This will show a dialog asking for Name, Target, Cron, Is Enabled and Parameters. These are:

- Name: Unique value that will identify the job's rule. AWS naming rules applies for this field
- Target: Available job implementations (e.g., **Update Orders' SomeColumn Job**)
- Cron: An AWS Event Bridge cron expression that will tell when to trigger the rule and therefore run the job ([Cron Expressions](https://docs.aws.amazon.com/eventbridge/latest/userguide/eb-create-rule-schedule.html#eb-cron-expressions))
- Is Enabled: Whether the rule is enabled to trigger
- Parameters: Here we set up any parameters provided by the job's metadata

After we fill all the above information and click the **Save** button we start receiving requests from AWS Event Bridge everytime the rule triggers.

Note that the job will get a `CancellationToken` when it's run so if the job takes too long to finish it should periodically check whether the token has been requested for cancellation (either by the user or by the application). For example:

```c#
public async Task RunAsync(
    IEnumerable<AwsEventBridgeJobParameterDTO> @params,
    CancellationToken ct = default)
{
    // time-consuming logic
    while (some condition isn't met)
    {
        ct.ThrowIfCancellationRequested();
        // do something that doesn't take too much time
    }
}
```
