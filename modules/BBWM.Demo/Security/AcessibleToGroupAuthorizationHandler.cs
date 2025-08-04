using BBWM.Core.Extensions;
using BBWM.Core.Membership.Model;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;

using Microsoft.AspNetCore.Authorization;

using System.Linq.Expressions;

namespace BBWM.Demo.Security;

public abstract class AcessibleToGroupAuthorizationHandler<TResourceInfo> : AuthorizationHandler<AccessibleToGroupRequirement, TResourceInfo>
    where TResourceInfo : class
{ }

public class AcessibleToGroupForListAuthorizationHandler : AcessibleToGroupAuthorizationHandler<AccessibleToGroupForListResourceInfo>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccessibleToGroupRequirement requirement,
        AccessibleToGroupForListResourceInfo resource)
    {
        if (resource is not null)
        {
            var userGroups = context.User.Claims.Where(claim => claim.Type == ClaimTypes.BelongsToGroup).Select(a => a.Value);

            // By default any authenticated user has access to paid orders
            Expression<Func<Order, bool>> expression = order => order.IsPaid;

            foreach (var group in userGroups)
            {
                Expression<Func<Order, bool>> expr = null;
                // change filter
                // The filter below is a "toy" filter example.
                // A typical group filter might check the group Id of the claim against a group Id on the resource
                var groupId = int.Parse(group);

                if (groupId == Groups.IdGroupA)
                {
                    expr = order => order.OrderDate.HasValue;
                }
                else if (groupId == Groups.IdGroupB)
                {
                    expr = order => order.ShippedDate.HasValue;
                }

                // If user belongs both to A and B that it's needs to get orders with OrderDate OR ShippedDate (and also paid ones)
                if (expr is not null)
                {
                    expression = expression is not null ? expression.Or(expr) : expr;
                }
            }

            if (expression is not null)
            {
                // TODO: recover for CRUD
                //resource.DataService.ConfigureDataContext(ctx =>
                //    ctx.Filter<Order>(qf => qf.Where(expression)));
            }
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public class AccessibleToGroupByIdAuthorizationHandler : AcessibleToGroupAuthorizationHandler<AccessibleToGroupForIdResourceInfo>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessibleToGroupRequirement requirement, AccessibleToGroupForIdResourceInfo resource)
    {
        if (resource?.Order is null) return Task.CompletedTask;

        var userGroups = context.User.Claims.Where(claim => claim.Type == ClaimTypes.BelongsToGroup).Select(a => a.Value);

        var res = resource.Order.IsPaid;

        foreach (var group in userGroups)
        {
            var groupId = int.Parse(group);

            if (groupId == Groups.IdGroupA)
            {
                res = res || resource.Order.OrderDate.HasValue;
            }
            else if (groupId == Groups.IdGroupB)
            {
                res = res || resource.Order.ShippedDate.HasValue;
            }

            if (res) break;
        }

        if (res)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class AccessibleToGroupRequirement : IAuthorizationRequirement
{
}

public class AccessibleToGroupForListResourceInfo
{
    // TODO: recover for CRUD

    public object DataService { get; }

    public AccessibleToGroupForListResourceInfo(object dataService)
    {
        DataService = dataService;
    }
}

public class AccessibleToGroupForIdResourceInfo
{
    public OrderDTO Order { get; }

    public AccessibleToGroupForIdResourceInfo(OrderDTO order)
    {
        Order = order;
    }
}
