using AutoMapper;
using AutoMapper.Internal;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Reflection;

namespace BBWM.DbDoc.Web;

public class DbDocMetadataValidationFilter : IAsyncActionFilter
{
    private readonly IDbDocService _dbDocService;
    private readonly DbModelValidator _modelValidator;
    private readonly bool _hasMapping;
    private readonly IMapper _mapper;
    private readonly Type _modelType;
    private readonly Guid _validationRulesSourceFolderId;

    public DbDocMetadataValidationFilter(IDbDocService dbDbDocService, DbModelValidator modelValidator, bool hasMapping, IMapper mapper, Type modelType, string validationRulesSourceFolderId)
    {
        _dbDocService = dbDbDocService;
        _modelValidator = modelValidator;
        _hasMapping = hasMapping;
        _mapper = mapper;
        _modelType = modelType;
        Guid.TryParse(validationRulesSourceFolderId, out _validationRulesSourceFolderId);
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        if (method == "PUT" || method == "POST")
        {
            var fromBodyMethodParameter = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo.GetParameters()
                .FirstOrDefault(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(FromBodyAttribute)));

            if (fromBodyMethodParameter != null)
            {
                var value = context.ActionArguments.Single(x => x.Key == fromBodyMethodParameter.Name).Value;

                var destinationType = fromBodyMethodParameter.ParameterType;
                if (_hasMapping)
                {
                    if (_modelType != null)
                    {
                        value = _mapper.Map(value, fromBodyMethodParameter.ParameterType, _modelType);
                        destinationType = _modelType;
                    }
                    else
                    {
                        var map = _mapper.ConfigurationProvider.Internal().GetAllTypeMaps()
                            .FirstOrDefault(x => x.SourceType == fromBodyMethodParameter.ParameterType);

                        if (map != null)
                        {
                            value = _mapper.Map(value, fromBodyMethodParameter.ParameterType, map.DestinationType);
                            destinationType = map.DestinationType;
                        }
                    }
                }

                var rules = await _dbDocService.GetValidationRulesForModel(destinationType, _validationRulesSourceFolderId, context.HttpContext.RequestAborted);
                foreach (var pair in rules)
                {
                    var property = destinationType.GetProperty(pair.Key, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

                    if (property == null) continue;

                    var propertyValue = property.GetValue(value);
                    foreach (var rule in pair.Value)
                    {
                        if (!_modelValidator.Validate(rule, propertyValue))
                        {
                            context.ModelState.AddModelError(property.Name, rule.ErrorMessage);
                        }
                    }
                }
            }
        }

        if (context.ModelState.IsValid)
            await next();
        else
            context.Result = new BadRequestObjectResult(context.ModelState);
    }
}
