using Microsoft.AspNetCore.Mvc;

namespace BBWM.Core.Web.ModelBinders;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class HashedKeyBinderAttribute : ModelBinderAttribute
{
    public HashedKeyBinderAttribute() : base(typeof(HashedKeyBinder))
    {
    }

    public HashedKeyBinderAttribute(Type hashingModelType) : this() => HashingDtoType = hashingModelType;

    public HashedKeyBinderAttribute(Type hashingModelType, string hashingDtoPropertyName) : this(hashingModelType) =>
        HashingDtoPropertyName = hashingDtoPropertyName;


    public Type HashingDtoType { get; set; }

    public string HashingDtoPropertyName { get; set; } = "Id";
}
