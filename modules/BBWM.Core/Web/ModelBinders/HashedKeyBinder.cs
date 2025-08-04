using BBWM.Core.Exceptions;
using BBWM.Core.ModelHashing;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BBWM.Core.Web.ModelBinders;

public class HashedKeyBinder : IModelBinder
{
    private readonly Regex _regex = new Regex("-[0-9A-F]{16}$");
    private readonly IModelHashingService _modelHashingService;


    public HashedKeyBinder(ILoggerFactory loggerFactory, IModelHashingService modelHashingService)
    {
        LoggerFactory = loggerFactory;
        _modelHashingService = modelHashingService;
    }


    public ILoggerFactory LoggerFactory { get; }


    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var actionParameterName = GetActionParameterName(bindingContext);
        var actionDescriptor = (bindingContext.ActionContext as ControllerContext)?.ActionDescriptor;
        var actionParameter = actionDescriptor?.MethodInfo?.GetParameters()
            ?.SingleOrDefault(x => x.Name.Equals(actionParameterName));

        var hashedKeyBinderAttribute = actionParameter?.GetCustomAttribute<HashedKeyBinderAttribute>();

        if (hashedKeyBinderAttribute is null || actionParameter.ParameterType != typeof(int))
        {
            var binder = new SimpleTypeModelBinder(bindingContext.ModelType, LoggerFactory);
            await binder.BindModelAsync(bindingContext);
            return;
        }

        var valueProviderResult = bindingContext.ValueProvider.GetValue(actionParameterName);
        if (valueProviderResult == ValueProviderResult.None) return;

        if (!_regex.IsMatch(valueProviderResult.FirstValue))
        {
            throw new EntityNotFoundException();
        }

        Type hashingDtoType = null;
        string hashingDtoPropertyName = null;

        // Primary data source for the hashing is an attribute that provides ability to set data directly for the parameter.
        if (hashedKeyBinderAttribute?.HashingDtoType is not null)
        {
            hashingDtoType = hashedKeyBinderAttribute.HashingDtoType;
            hashingDtoPropertyName = hashedKeyBinderAttribute.HashingDtoPropertyName;
        }

        // Secondary place is the parameter type itself.
        if (hashingDtoType is null)
        {
            if (actionParameter is not null &&
                actionParameter.ParameterType.Name.EndsWith("DTO", StringComparison.InvariantCultureIgnoreCase))
            {
                hashingDtoType = actionParameter.ParameterType;
            }
        }

        // The third way is to determine controller's generic DTO parameter.
        if (hashingDtoType is null)
        {
            var baseType = actionDescriptor?.ControllerTypeInfo?.BaseType;
            hashingDtoType = baseType?.GetGenericArguments().FirstOrDefault(x =>
                x.Name.EndsWith("DTO", StringComparison.InvariantCultureIgnoreCase));
        }

        // Try to match action parameter name with any DTO property.
        if (hashingDtoType is not null && string.IsNullOrEmpty(hashingDtoPropertyName))
        {
            if (hashingDtoType.GetProperties().Any(x =>
                x.Name.Equals(actionParameterName, StringComparison.InvariantCultureIgnoreCase)))
            {
                hashingDtoPropertyName = actionParameterName;
            }
        }

        if (hashingDtoType is null || string.IsNullOrEmpty(hashingDtoPropertyName))
        {
            throw new ConflictException("Unable to determine a DTO type to unhash the key.");
        }

        var unhashedProperty = _modelHashingService.UnHashProperty(
            GetPropertyDeclaringType(hashingDtoType, hashingDtoPropertyName),
            GetPropertyOfDeclaringType(hashingDtoPropertyName),
            valueProviderResult.FirstValue);

        bindingContext.Result = unhashedProperty.HasValue
            ? ModelBindingResult.Success(unhashedProperty.Value)
            : ModelBindingResult.Failed();
    }


    private static string GetActionParameterName(ModelBindingContext bindingContext)
    {
        // The "Name" property of the ModelBinder attribute can be used to specify the
        // route parameter name when the action parameter name is different from the route parameter name.
        if (!string.IsNullOrEmpty(bindingContext.BinderModelName))
        {
            return bindingContext.BinderModelName;
        }
        return bindingContext.ModelName;
    }

    private static Type GetPropertyDeclaringType(Type dtoType, string propertyName)
    {
        var propertyContrainerType = dtoType;
        var propertyNames = propertyName.Split(".");

        PropertyInfo propertyInfo = null;

        foreach (var property in propertyNames)
        {
            propertyInfo = propertyContrainerType.GetProperties().SingleOrDefault(x => x.Name.Equals(property, StringComparison.InvariantCultureIgnoreCase));

            if (propertyInfo is null)
            {
                throw new ValidationException(new ValidationResult($"{property} not found in {propertyContrainerType.Name}", new[] { propertyName }), null, null);
            }

            propertyContrainerType = propertyInfo?.PropertyType;
        }

        return propertyInfo.ReflectedType;
    }

    private static string GetPropertyOfDeclaringType(string propertyName)
    {
        return propertyName.Split(".").Last();
    }
}
