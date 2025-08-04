using BBWM.AWS.EventBridge.DTO;

namespace BBWM.AWS.EventBridge.Test.Service;

public static class AwsEventBridgeRuleServiceTestData
{
    private static readonly TestParameterInfo Unknown = new TestParameterInfo
    {
        Parameters = new List<JobParameterInfo>(),
        Error = "Unknown parameter MyParam.",
        InputParameters = new List<AwsEventBridgeJobParameterDTO>
            {
                new AwsEventBridgeJobParameterDTO
                {
                    Name = "MyParam",
                    Value = "DummyValue",
                },
            },
    };

    private static readonly TestParameterInfo SameParamRepeated = new TestParameterInfo
    {
        Parameters = new List<JobParameterInfo>
            {
                new JobParameterInfo
                {
                    Name = "P1",
                },
            },
        Error = "Parameter P1 cannot have multiple occurrences.",
        InputParameters = new List<AwsEventBridgeJobParameterDTO>
            {
                new AwsEventBridgeJobParameterDTO
                {
                    Name = "P1",
                    Value = "P1-Value_1",
                },
                new AwsEventBridgeJobParameterDTO
                {
                    Name = "P1",
                    Value = "P1-Value_2",
                },
            },
    };

    private static readonly TestParameterInfo RequiredParamMissing = new TestParameterInfo
    {
        Parameters = new List<JobParameterInfo>
            {
                new JobParameterInfo
                {
                    Name = "P1",
                    Required = true,
                },
            },
        Error = "Some required parameters are mising.",
        InputParameters = null,
    };

    private static readonly TestParameterInfo RequiredParamMissing2 = new TestParameterInfo
    {
        Parameters = new List<JobParameterInfo>
            {
                new JobParameterInfo
                {
                    Name = "P1",
                    Required = true,
                },
                new JobParameterInfo
                {
                    Name = "P2",
                    Required = true,
                },
            },
        Error = "Some required parameters are mising.",
        InputParameters = new List<AwsEventBridgeJobParameterDTO>
            {
                new AwsEventBridgeJobParameterDTO
                {
                    Name = "P1",
                    Value = "P1-Value",
                },
            },
    };

    private static readonly TestParameterInfo RequiredParamEmpty = new TestParameterInfo
    {
        Parameters = new List<JobParameterInfo>
            {
                new JobParameterInfo
                {
                    Name = "P1",
                    Required = true,
                },
            },
        Error = "Parameter P1 is required.",
        InputParameters = new List<AwsEventBridgeJobParameterDTO>
            {
                new AwsEventBridgeJobParameterDTO
                {
                    Name = "P1",
                    Value = "",
                },
            },
    };

    private static readonly TestParameterInfo RequiredParamNull = new TestParameterInfo
    {
        Parameters = new List<JobParameterInfo>
            {
                new JobParameterInfo
                {
                    Name = "P1",
                    Required = true,
                },
            },
        Error = "Parameter P1 is required.",
        InputParameters = new List<AwsEventBridgeJobParameterDTO>
            {
                new AwsEventBridgeJobParameterDTO
                {
                    Name = "P1",
                    Value = null,
                },
            },
    };

    public static List<object[]> ParameterTestData => new List<object[]>
        {
            new object[] // Unknown
            {
                Unknown,
                true,
            },
            new object[] // Unknown
            {
                Unknown,
                false,
            },
            new object[] // Same parameter repeated
            {
                SameParamRepeated,
                true,
            },
            new object[] // Same parameter repeated
            {
                SameParamRepeated,
                false,
            },
            new object[] // Required parameters missing
            {
               RequiredParamMissing,
               true,
            },
            new object[] // Required parameters missing
            {
               RequiredParamMissing,
               false,
            },
            new object[] // Required parameters missing
            {
                RequiredParamMissing2,
                true,
            },
            new object[] // Required parameters missing
            {
                RequiredParamMissing2,
                false,
            },
            new object[] // Required parameter given but empty
            {
                RequiredParamEmpty,
                true,
            },
            new object[] // Required parameter given but empty
            {
                RequiredParamEmpty,
                false,
            },
            new object[] // Required parameter given but null
            {
                RequiredParamNull,
                true,
            },
            new object[] // Required parameter given but null
            {
                RequiredParamNull,
                false,
            },
        };
}

public class TestParameterInfo
{
    public List<JobParameterInfo> Parameters { get; set; }

    public string Error { get; set; }

    public List<AwsEventBridgeJobParameterDTO> InputParameters { get; set; }
}
