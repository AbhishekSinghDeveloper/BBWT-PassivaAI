using Amazon.EventBridge;
using Amazon.EventBridge.Model;

using BBWM.AWS.EventBridge.Interfaces;

using Moq;

namespace BBWM.AWS.EventBridge.Test;

internal static class AmazonEventBridgeMockHelper
{
    public static Mock<IAmazonEventBridge> CreateClient()
     => new Mock<IAmazonEventBridge>();

    public static Mock<IAmazonEventBridge> DescribeRule_ResourceNotFound(this Mock<IAmazonEventBridge> mock)
    {
        var notFound = new ResourceNotFoundException("Resource not found");
        mock
            .Setup(c => c.DescribeRuleAsync(It.IsAny<DescribeRuleRequest>(), It.IsAny<CancellationToken>()))
            .Throws(notFound);

        return mock;
    }

    public static Mock<IAmazonEventBridge> DescribeRule(this Mock<IAmazonEventBridge> mock, string ruleId = default)
    {
        mock
            .Setup(c => c.DescribeRuleAsync(It.IsAny<DescribeRuleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeRuleResponse
            {
                Name = ruleId ?? $"Rule_{Guid.NewGuid():N}",
                State = RuleState.ENABLED,
                ScheduleExpression = "cron(* * * * ? *)",
            });

        return mock;
    }

    public static Mock<IAmazonEventBridge> DescribeDisabledRule(this Mock<IAmazonEventBridge> mock, string ruleId)
    {
        mock
            .Setup(c => c.DescribeRuleAsync(It.IsAny<DescribeRuleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeRuleResponse
            {
                Name = ruleId,
                State = RuleState.DISABLED,
                ScheduleExpression = "cron(* * * * ? *)",
            });

        return mock;
    }

    public static Mock<IAmazonEventBridge> PutRule(this Mock<IAmazonEventBridge> mock)
    {
        mock
            .Setup(c => c.PutRuleAsync(It.IsAny<PutRuleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutRuleResponse { });

        return mock;
    }

    public static Mock<IAmazonEventBridge> PutRule_BadRequest_ScheduleExpression(this Mock<IAmazonEventBridge> mock)
    {
        var badRequest = new AmazonEventBridgeException(
            $"The field {nameof(PutRuleRequest.ScheduleExpression)} is not correct.")
        {
            StatusCode = System.Net.HttpStatusCode.BadRequest,
            ErrorCode = "ValidationException",
        };

        mock
            .Setup(c => c.PutRuleAsync(It.IsAny<PutRuleRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(badRequest);

        return mock;
    }

    public static Mock<IAmazonEventBridge> PutRule_GenericError(this Mock<IAmazonEventBridge> mock)
    {
        mock
            .Setup(c => c.PutRuleAsync(It.IsAny<PutRuleRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unknown error."));

        return mock;
    }

    public static Mock<IAmazonEventBridge> DescribeApiDestination(this Mock<IAmazonEventBridge> mock)
    {
        mock
            .Setup(c => c.DescribeApiDestinationAsync(It.IsAny<DescribeApiDestinationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeApiDestinationResponse
            {
                ApiDestinationArn =
                    "arn:aws:events:eu-west-1:214748364784:api-destination" +
                    "/DummyApiDestination/7b2bdb0c-b25e-4006-9100-572fd3b5a7a1",
            });

        return mock;
    }

    public static Mock<IAmazonEventBridge> DescribeApiDestination_NotFound(this Mock<IAmazonEventBridge> mock)
    {
        var notFound = new ResourceNotFoundException("Api destination not found");
        mock
            .Setup(c => c.DescribeApiDestinationAsync(It.IsAny<DescribeApiDestinationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(notFound);

        return mock;
    }

    public static Mock<IAmazonEventBridge> CreateApiDestination(this Mock<IAmazonEventBridge> mock)
    {
        mock
            .Setup(c => c.CreateApiDestinationAsync(It.IsAny<CreateApiDestinationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateApiDestinationResponse
            {
                ApiDestinationArn =
                    "arn:aws:events:eu-west-1:214748364784:api-destination" +
                    "/DummyApiDestination/7b2bdb0c-b25e-4006-9100-572fd3b5a7a1",
            });

        return mock;
    }

    public static Mock<IAmazonEventBridge> PutTargets(this Mock<IAmazonEventBridge> mock)
    {
        mock
            .Setup(c => c.PutTargetsAsync(It.IsAny<PutTargetsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutTargetsResponse());

        return mock;
    }

    public static Mock<IAmazonEventBridge> ListRules(
        this Mock<IAmazonEventBridge> mock, string ruleName, string cron)
    {
        mock
            .Setup(c => c.ListRulesAsync(It.IsAny<ListRulesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListRulesResponse
            {
                Rules = new List<Rule>
                {
                        new Rule
                        {
                            Name = ruleName,
                            ScheduleExpression = cron,
                            State = RuleState.ENABLED,
                        },
                },
            });

        return mock;
    }

    public static Mock<IAmazonEventBridge> RemoveTargets(
        this Mock<IAmazonEventBridge> mock)
    {
        mock
            .Setup(c => c.RemoveTargetsAsync(It.IsAny<RemoveTargetsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RemoveTargetsResponse());

        return mock;
    }

    public static Mock<IAmazonEventBridge> DeleteRule(
        this Mock<IAmazonEventBridge> mock)
    {
        mock
            .Setup(c => c.DeleteRuleAsync(It.IsAny<DeleteRuleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteRuleResponse());

        return mock;
    }

    public static Mock<IAmazonEventBridge> DescribeConnection_NotFound(
        this Mock<IAmazonEventBridge> mock)
    {
        var notFound = new ResourceNotFoundException("Connection not found");
        mock
            .Setup(c => c.DescribeConnectionAsync(It.IsAny<DescribeConnectionRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(notFound);

        return mock;
    }

    public static Mock<IAmazonEventBridge> CreateConnection(
        this Mock<IAmazonEventBridge> mock)
    {
        mock
            .Setup(c => c.CreateConnectionAsync(It.IsAny<CreateConnectionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateConnectionResponse
            {
                ConnectionArn =
                    "arn:aws:events:eu-west-1:113846440053:connection/" +
                    "Dummy-Connection/5d1d6542-b4b6-41c2-a6f4-66ce8c0bb8d3",
            });

        return mock;
    }

    public static Mock<IAwsEventBridgeClientFactory> BuildFactory(this Mock<IAmazonEventBridge> clientMock)
    {
        var clientFactoryMock = new Mock<IAwsEventBridgeClientFactory>();

        clientFactoryMock.Setup(f => f.CreateClient()).Returns(clientMock.Object);

        return clientFactoryMock;
    }
}
