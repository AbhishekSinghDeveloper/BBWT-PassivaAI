using BBWM.Core.DTO;
using BBWM.Core.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.Core.ModelHashing;

public static class HttpRequestExtensions
{
    public static CreatedResult CreatedResult<TEntityDTO, TKey>(this HttpRequest request,
        TEntityDTO result, IModelHashingService modelHashingService)
        where TEntityDTO : IDTO<TKey>
        where TKey : IEquatable<TKey>
    {
        var idStringValue = Convert.ToString(result.Id);
        var hashedId = modelHashingService.HashProperty(result, nameof(IDTO<TKey>.Id));
        return new CreatedResult($"{request.GetDomainUrl()}{request.Path}/{hashedId ?? idStringValue}", result);
    }
}
