using AutoMapper;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.DbDoc.Web;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DbDocMetadataValidationFilterAttribute : Attribute, IFilterFactory
{
    private readonly bool _hasMapping;
    private readonly Type _modelType;
    private readonly string _validationRulesSourceFolderId;

    public DbDocMetadataValidationFilterAttribute(bool hasMapping = true, Type modelType = null, string validationRulesSourceFolderId = null)
    {
        _hasMapping = hasMapping;
        _modelType = modelType;
        _validationRulesSourceFolderId = validationRulesSourceFolderId;
    }

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var dbDocService = serviceProvider.GetService<IDbDocService>();
        var mapper = serviceProvider.GetService<IMapper>();

        return new DbDocMetadataValidationFilter(dbDocService, new DbModelValidator(), _hasMapping, mapper, _modelType, _validationRulesSourceFolderId);
    }

    public bool IsReusable => false;
}