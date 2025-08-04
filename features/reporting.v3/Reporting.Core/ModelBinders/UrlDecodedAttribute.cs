using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.Core.ModelBinders;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class UrlDecodedAttribute : ModelBinderAttribute
{
    public UrlDecodedAttribute() : base(typeof(UrlDecodedModelBinder))
    {
    }
}