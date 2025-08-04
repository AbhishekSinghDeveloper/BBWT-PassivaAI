namespace BBWM.Core.Filters.Handlers;

public class RelatedHandlerAttribute : Attribute
{
    public RelatedHandlerAttribute(Type handlerType)
    {
        HandlerType = handlerType;
    }

    public Type HandlerType { get; }
}
