using Amazon.EventBridge;
using Amazon.EventBridge.Model;

using AutoMapper;

using BBWM.AWS.EventBridge.Api;
using BBWM.AWS.EventBridge.AwsCron;
using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace BBWM.AWS.EventBridge.Service;

public class AwsEventBridgeRuleService : IAwsEventBridgeRuleService
{
    private static readonly string ebTargetController =
        Regex.Replace(nameof(AwsEventBridgeJobStarterController), "Controller$", "");

    private readonly AwsEventBridgeSettings eventBridgeSettings;
    private readonly IMapper _mapper;
    private readonly IUrlHelperFactory urlHelperFactory;
    private readonly IActionContextAccessor actionContextAccessor;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IWebHostEnvironment environment;
    private readonly IAwsEventBridgeJobService awsEventBridgeJobService;
    private readonly IAwsEventBridgeClientFactory clientFactory;
    private readonly IDataService dataService;

    public AwsEventBridgeRuleService(
        IMapper mapper,
        IDataService dataService,
        IOptionsSnapshot<AwsEventBridgeSettings> eventBridgeSettings,
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment environment,
        IAwsEventBridgeJobService awsEventBridgeJobService,
        IAwsEventBridgeClientFactory clientFactory)
    {
        this.eventBridgeSettings = eventBridgeSettings.Value;
        this.dataService = dataService;
        _mapper = mapper;
        this.urlHelperFactory = urlHelperFactory;
        this.actionContextAccessor = actionContextAccessor;
        this.httpContextAccessor = httpContextAccessor;
        this.environment = environment;
        this.awsEventBridgeJobService = awsEventBridgeJobService;
        this.clientFactory = clientFactory;
    }

    public Task<AwsEventBridgeRuleDTO> Create(AwsEventBridgeRuleDTO dto, CancellationToken ct)
        => Save(dto, ct);

    public Task<AwsEventBridgeRuleDTO> Update(AwsEventBridgeRuleDTO dto, CancellationToken ct)
        => Save(dto, ct);

    public async Task<PageResult<AwsEventBridgeRuleDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
    {
        using var client = clientFactory.CreateClient();

        var loadAwsEbRules = client.ListRulesAsync(new ListRulesRequest(), ct);
        var loadAllJobs = awsEventBridgeJobService.GetAllAsync(default, ct);

        await Task.WhenAll(loadAwsEbRules, loadAllJobs);

        var ruleToJobMap = loadAllJobs.Result.ToDictionary(j => j.RuleId);
        var rules = _mapper.Map<List<AwsEventBridgeRuleDTO>>(
            loadAwsEbRules.Result.Rules.Where(r => ruleToJobMap.ContainsKey(r.Name)));

        var query = rules.AsQueryable();
        var total = rules.Count();

        ExtendRules(query, ruleToJobMap);

        if (command is not null)
        {
            query = DataFilterSorter<AwsEventBridgeRuleDTO>.ApplyFilter(query, command);
            query = DataFilterSorter<AwsEventBridgeRuleDTO>.ApplySorting(query, command);

            total = query.Count();

            if (command.Skip is not null)
                query = query.Skip(command.Skip.Value);
            if (command.Take is not null)
                query = query.Take(command.Take.Value);
        }

        return new PageResult<AwsEventBridgeRuleDTO>
        {
            Items = query.ToList(),
            Total = total
        };
    }

    public async Task<bool> RuleExistsAsync(string name, CancellationToken ct = default)
    {
        if (await Exists(name, ct))
            return true;

        return await awsEventBridgeJobService.FindByRuleAsync(name, ct) is not null;
    }

    public async Task Delete(string id, CancellationToken ct = default)
    {
        using var client = clientFactory.CreateClient();

        await ClearRuleTargetAsync(id, client, ct);

        await client.DeleteRuleAsync(
            new DeleteRuleRequest { Name = id }, ct);

        var ourJob = await awsEventBridgeJobService.FindByRuleAsync(id, ct);
        if (ourJob is not null)
        {
            await dataService.Delete<EventBridgeJob>(ourJob.Id, ct);
        }
    }

    // private methods
    private async Task<AwsEventBridgeRuleDTO> Save(AwsEventBridgeRuleDTO dto, CancellationToken cancellationToken)
    {
        if (!awsEventBridgeJobService.IsJobRegistered(dto.TargetJobId))
        { throw new ObjectNotExistsException("The given target job doesn't exist."); }

        await CheckRuleNameAsync(dto, cancellationToken);
        CheckJobParameters(dto);

        using var client = clientFactory.CreateClient();
        try
        {
            var request = new PutRuleRequest();

            _mapper.Map(dto, request);

            await client.PutRuleAsync(request, cancellationToken);
            await SetupRuleTargetAsync(dto.Name, client, cancellationToken);
            await SaveJobInfoAsync(dto, cancellationToken);

            return dto;
        }
        catch (Exception e)
        { return await HandleSaveRuleException(e, dto); }
    }

    private async Task<bool> Exists(string id, CancellationToken ct = default)
    {
        using var client = clientFactory.CreateClient();
        try
        {
            await client.DescribeRuleAsync(new DescribeRuleRequest { Name = id }, ct);
            return true;
        }
        catch (ResourceNotFoundException)
        { return false; }
    }

    private async Task SetupRuleTargetAsync(
        string ruleName, IAmazonEventBridge client, CancellationToken cancellationToken)
    {
        var apiDestinationARN = await GetApiDestinationARNAsync(client, cancellationToken);
        await client.PutTargetsAsync(
            new PutTargetsRequest
            {
                Rule = ruleName,
                Targets = new List<Target>
                {
                        new Target
                        {
                            Id = GetRuleTargetId(ruleName),
                            Arn = apiDestinationARN,
                            RoleArn = eventBridgeSettings.TargetRoleArn,
                            HttpParameters = new HttpParameters
                            { PathParameterValues = new List<string> { ruleName } }
                        }
                }
            },
            cancellationToken);
    }

    private static async Task ClearRuleTargetAsync(
        string ruleName, IAmazonEventBridge client, CancellationToken cancellationToken)
        => await client.RemoveTargetsAsync(
            new RemoveTargetsRequest
            {
                Ids = new List<string> { GetRuleTargetId(ruleName) },
                Rule = ruleName
            },
            cancellationToken);

    private static string GetRuleTargetId(string ruleName) => $"{ruleName}ApiTarget";

    private async Task<string> GetApiDestinationARNAsync(
        IAmazonEventBridge client, CancellationToken cancellationToken)
    {
        var request = new DescribeApiDestinationRequest { Name = eventBridgeSettings.ApiDestinationName };

        try
        {
            var apiDest = await client
                .DescribeApiDestinationAsync(request, cancellationToken);
            return apiDest.ApiDestinationArn;
        }
        catch (ResourceNotFoundException)
        {
            var invocationEndpointTemplate = GetApiDestinationURL();
            var connectionArn = await GetApiConnectionARNAsync(client, cancellationToken);
            var apiDest = await client.CreateApiDestinationAsync(
               new CreateApiDestinationRequest
               {
                   ConnectionArn = connectionArn,
                   HttpMethod = ApiDestinationHttpMethod.POST,
                   InvocationEndpoint = invocationEndpointTemplate,
                   Name = eventBridgeSettings.ApiDestinationName
               },
               cancellationToken);

            return apiDest.ApiDestinationArn;
        }
    }

    private string GetApiDestinationURL()
    {
        var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        var targetUrl = urlHelper.RouteUrl(
            "StartJob", new { ruleId = "*" }, httpContextAccessor.HttpContext.Request.Scheme);

        if (environment.IsDevelopment())
        {
            var devTargetUrl = Environment.GetEnvironmentVariable("EB_TARGET_URL");
            if (!string.IsNullOrEmpty(devTargetUrl))
            {
                var targetUrlUri = new Uri(targetUrl);
                var uriBuilder = new UriBuilder(devTargetUrl)
                { Path = targetUrlUri.AbsolutePath };
                targetUrl = uriBuilder.Uri.ToString();
            }
        }

        return targetUrl;
    }

    private async Task<string> GetApiConnectionARNAsync(
        IAmazonEventBridge client, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await client.DescribeConnectionAsync(
                new DescribeConnectionRequest { Name = eventBridgeSettings.ApiConnectionName },
                cancellationToken);
            return connection.ConnectionArn;
        }
        catch (ResourceNotFoundException)
        {
            var connection = await client.CreateConnectionAsync(
                new CreateConnectionRequest
                {
                    Name = eventBridgeSettings.ApiConnectionName,
                    AuthorizationType = ConnectionAuthorizationType.API_KEY,
                    AuthParameters = new CreateConnectionAuthRequestParameters
                    {
                        ApiKeyAuthParameters = new CreateConnectionApiKeyAuthRequestParameters
                        {
                            ApiKeyName = eventBridgeSettings.AuthHeader,
                            ApiKeyValue = eventBridgeSettings.APIKey
                        }
                    }
                },
                cancellationToken);
            return connection.ConnectionArn;
        }
    }

    private async Task SaveJobInfoAsync(AwsEventBridgeRuleDTO dto, CancellationToken cancellationToken)
    {
        var dbJob = await awsEventBridgeJobService.FindByRuleAsync(dto.Name, cancellationToken);
        if (dbJob is null)
        { dbJob = _mapper.Map<AwsEventBridgeJobDTO>(dto); }

        _mapper.Map(dto, dbJob);
        dbJob.NextExecutionTime = dto.IsEnabled
            ? AwsCronExpression.Parse(dto.GetAwsCron()).GetNextOccurrence(DateTime.UtcNow)
            : default;

        if (dbJob.Id == 0)
            await dataService.Create<EventBridgeJob, AwsEventBridgeJobDTO>(dbJob, cancellationToken);
        else
            await dataService.Update<EventBridgeJob, AwsEventBridgeJobDTO>(dbJob, cancellationToken);
    }

    private void ExtendRules(
        IEnumerable<AwsEventBridgeRuleDTO> items,
        Dictionary<string, AwsEventBridgeJobDTO> ruleToJobMap)
    {
        foreach (var item in items)
        {
            if (ruleToJobMap.TryGetValue(item.Name, out var job))
            { _mapper.Map(job, item); }
        }
    }

    private static Task<AwsEventBridgeRuleDTO> HandleSaveRuleException(Exception e, AwsEventBridgeRuleDTO dto)
    {
        if (e is AmazonEventBridgeException awsException &&
            (awsException.StatusCode == HttpStatusCode.BadRequest && awsException.ErrorCode == "ValidationException") &&
            e.Message.Contains(nameof(PutRuleRequest.ScheduleExpression))
            )
        {
            throw new ValidationException(
              new ValidationResult(
                  $"The expression \"{dto.Cron}\" is not correct.",
                  new[] { nameof(dto.Cron) }),
              null,
              dto);
        }

        throw new ApiException($"An error has occurred while saving the job \"{dto.Name}\".");
    }

    private void CheckJobParameters(AwsEventBridgeRuleDTO dto)
    {
        var paramsInfo = awsEventBridgeJobService
            .GetJobInfo(dto.TargetJobId)
            .Parameters
            .ToDictionary(p => p.Name);

        var parameters = dto.Parameters ?? new List<AwsEventBridgeJobParameterDTO>();

        var inputParams = parameters
            .GroupBy(p => p.Name)
            .Select(gi => new
            {
                Name = gi.Key,
                HasMany = gi.Count() > 1,
                Values = gi.Select(r => r.Value).ToList()
            });
        var allInputParams = new HashSet<string>();

        foreach (var param in inputParams)
        {
            allInputParams.Add(param.Name);
            if (paramsInfo.TryGetValue(param.Name, out var info))
            {
                if (param.HasMany)
                {
                    ThrowParamValidationException(
                        $"Parameter {param.Name} cannot have multiple occurrences.",
                        dto);
                }
                if (info.Required &&
                    (param.Values.Count == 0 || param.Values.Any(v => string.IsNullOrEmpty(v))))
                { ThrowParamValidationException($"Parameter {param.Name} is required.", dto); }
            }
            else
            { ThrowParamValidationException($"Unknown parameter {param.Name}.", dto); }
        }

        paramsInfo
            .Where(kv => kv.Value.Required)
            .ToList()
            .ForEach(kv =>
            {
                if (!allInputParams.Contains(kv.Key))
                { ThrowParamValidationException($"Some required parameters are mising.", dto); }
            });
    }

    private static void ThrowParamValidationException(string errorMessage, AwsEventBridgeRuleDTO dto)
    {
        throw new ValidationException(
            new ValidationResult(
                errorMessage,
                new[] { nameof(AwsEventBridgeRuleDTO.Parameters) }),
            null,
            dto);
    }

    private async Task CheckRuleNameAsync(AwsEventBridgeRuleDTO dto, CancellationToken cancellationToken = default)
    {
        var job = await awsEventBridgeJobService.FindByRuleAsync(dto.Name, cancellationToken);
        var editingRule = string.Compare(
            dto.Name, job?.RuleId, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase) == 0;

        if (await RuleExistsAsync(dto.Name, cancellationToken) && !editingRule)
        {
            throw new ValidationException(new ValidationResult(
                "Name is already taken by another rule.",
                new[] { nameof(AwsEventBridgeRuleDTO.Name) }),
            null,
            dto);
        }
    }
}
