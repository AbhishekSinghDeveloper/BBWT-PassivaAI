AggregatedLogs.Lambda can be deployed as AWS lambda function and used in subscription to CloudWatch log groups. It saves CloudWatch events to aggregated logs in database.

Use DatabaseSettings in appsettings.json to set up database connection for lambda. This file has to be copied to AWS when function is published.

Permission for CloudWatch Logs to execute this lambda has to be granted, then a subscription to log group can be created. Pattern for subscription:

[timestamp=*Z, request_id="*-*", type, event]

AppName for lambdas is the name of a log group.

You can use files in TestLambda folder to create a project and deploy a test lambda function. It will genetate CloudWatch logs.

IMPORTANT: use ILambdaContext in lambda functions to log with event type instead of static LambdaLogger.
