using System.Reflection;
using BBF.Reporting.Core.Model.Variables;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BBF.Reporting.Core.ModelBinders;

public class EmittedVariableModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType != typeof(EmittedVariable)) return null;

        var binders = new Dictionary<string, IModelBinder>();

        var variableTypes = Assembly.GetAssembly(typeof(EmittedVariable))?.GetTypes()
            .Where(type => type.GetTypeInfo().IsSubclassOf(typeof(EmittedVariable)) && !type.IsAbstract);

        if (variableTypes == null) throw new InvalidOperationException("Cannot find 'EmittedVariable' assembly data.");

        foreach (var type in variableTypes)
        {
            if (type.FullName == null) continue;
            var metadata = context.MetadataProvider.GetMetadataForType(type);
            var binder = context.CreateBinder(metadata);
            binders.Add(type.FullName, binder);
        }

        return new EmittedVariableModelBinder(context.MetadataProvider, binders);
    }
}