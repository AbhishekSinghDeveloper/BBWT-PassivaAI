using Amazon;
using Amazon.Runtime;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

using BBWM.AppConfiguration;
using BBWM.Core.Exceptions;

using Microsoft.Extensions.Options;

using AmazonParameter = Amazon.SimpleSystemsManagement.Model.Parameter;
using Parameter = BBWM.AppConfiguration.Parameter;

namespace BBWM.AWS;

public class AwsAppConfigurationService : IAppConfigurationService
{
    private readonly AwsSettings _awsSettings;


    public AwsAppConfigurationService(IOptionsSnapshot<AwsSettings> awsSettingsOptionsSnapshot) =>
        _awsSettings = awsSettingsOptionsSnapshot.Value;


    public async Task<IEnumerable<Parameter>> GetAll(CancellationToken cancellationToken = default)
    {
        using var client = GetClient();

        var request = new GetParametersByPathRequest
        {
            Path = _awsSettings.ParametersPath,
            WithDecryption = true
        };
        return (await client.GetParametersByPathAsync(request, cancellationToken)).Parameters.Select(ConvertAmazonParameter);
    }

    public async Task<Parameter> GetByName(string name, CancellationToken cancellationToken = default)
    {
        using var client = GetClient();

        var request = new GetParameterRequest
        {
            Name = $"{_awsSettings.ParametersPath}/{name}",
            WithDecryption = true
        };
        return ConvertAmazonParameter((await client.GetParameterAsync(request, cancellationToken)).Parameter);
    }

    public async Task Put(Parameter parameter, CancellationToken cancellationToken = default)
    {
        using var client = GetClient();

        var request = new PutParameterRequest
        {
            Name = $"{_awsSettings.ParametersPath}/{parameter.Name}",
            Value = parameter.Value,
            Type = parameter.Secure ? ParameterType.SecureString : ParameterType.String,
            Overwrite = true
        };
        await client.PutParameterAsync(request, cancellationToken);
    }
    public async Task Delete(string name, CancellationToken cancellationToken = default)
    {
        using var client = GetClient();

        var request = new DeleteParameterRequest
        {
            Name = $"{_awsSettings.ParametersPath}/{name}"
        };
        await client.DeleteParameterAsync(request, cancellationToken);
    }


    private Parameter ConvertAmazonParameter(AmazonParameter amazonParameter) => new Parameter
    {
        Name = RemovePrefixFromName(amazonParameter.Name),
        Value = amazonParameter.Value
    };

    private string RemovePrefixFromName(string parameterName) =>
        parameterName.Substring(_awsSettings.ParametersPath.Length + 1);

    private AmazonSimpleSystemsManagementClient GetClient()
    {
        if (_awsSettings is null ||
            string.IsNullOrEmpty(_awsSettings.AwsRegion) ||
            string.IsNullOrEmpty(_awsSettings.AccessKeyId) ||
            string.IsNullOrEmpty(_awsSettings.SecretAccessKey) ||
            string.IsNullOrEmpty(_awsSettings.ParametersPath))
        {
            throw new ConflictException("AWS settings is not set.");
        }

        return new AmazonSimpleSystemsManagementClient(
            new BasicAWSCredentials(_awsSettings.AccessKeyId, _awsSettings.SecretAccessKey),
            RegionEndpoint.GetBySystemName(_awsSettings.AwsRegion));
    }
}
