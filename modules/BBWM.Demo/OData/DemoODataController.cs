using BBWM.Core.Web.OData;
using BBWM.Demo.Northwind.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace BBWM.Demo.OData;

[Route("odata")]
public class DemoODataController : ODataController
{
    private readonly IDemoDataContext _context;


    public DemoODataController(IDemoDataContext context) => _context = context;


    [EnableQuery(EnableConstantParameterization = false, EnableCorrelatedSubqueryBuffering = true, MaxExpansionDepth = 3)]
    [HttpGet("Customers")]
    public IQueryable<Customer> GetAllCustomers() => _context.Customers;

    [OrdersEnableQuery(EnsureStableOrdering = false)]
    [HttpGet("Orders")]
    public IQueryable<Order> GetAllOrders() => _context.Orders;

    public class OrdersEnableQueryAttribute : CustomEnableQueryAttribute
    {
        private const string OrderDetailsFieldName = "orderDetails"; // Custom filter's name
        private const string CustomerCodeFieldName = "customer.code";

        // Here we may provide all security checks.
        /*public override void OnActionExecuting(ActionExecutingContext context, ODataQueryString queryString)
        {
            if (!context.HttpContext.User.IsInRole(Roles.SuperAdminRole))
                context.Result = new ForbidResult();

            OnActionExecuting(context);
        }*/

        public override IDictionary<string, Expression<Func<IQueryable, string, ODataQueryString, IQueryable>>> GetCustomizations()
        {
            Func<IQueryable, string, ODataQueryString, IQueryable> orderDetailsCustomization = (queryable, fieldName, queryString) =>
            {
                if (queryable is not IQueryable<Order> ordersQueryable) return queryable;

                if (queryString.ContainsFilter(OrderDetailsFieldName))
                {
                    var orderDetailsFilterValue = queryString.GetFilterValues<int>(OrderDetailsFieldName)[0];
                    ordersQueryable = ordersQueryable
                        .Include(x => x.OrderDetails)
                        .Where(x => x.OrderDetails.Count > orderDetailsFilterValue);
                }

                var orderStatement = queryString.GetStatement("orderby");
                if (orderStatement is not null)
                {
                    var statementArray = orderStatement.Split(" ");
                    if (statementArray[0] == OrderDetailsFieldName)
                    {
                        ordersQueryable = ordersQueryable.Include(x => x.OrderDetails);
                        ordersQueryable = statementArray.Length > 1 &&
                                          statementArray[1].Equals("desc", StringComparison.InvariantCultureIgnoreCase)
                            ? ordersQueryable.OrderByDescending(x => x.OrderDetails.Count)
                            : ordersQueryable.OrderBy(x => x.OrderDetails.Count);
                    }
                }

                return ordersQueryable;
            };
            Func<IQueryable, string, ODataQueryString, IQueryable> customerCodeCustomization = (queryable, fieldName, queryString) =>
            {
                if (queryable is not IQueryable<Order> ordersQueryable) return queryable;

                if (queryString.ContainsFilter(CustomerCodeFieldName))
                {
                    var customerCodeFilterValue = queryString.GetFilterValues<string>(CustomerCodeFieldName)[0];
                    ordersQueryable = ordersQueryable
                        .Include(x => x.Customer)
                        .Where(x => x.Customer.Code == customerCodeFilterValue);
                }

                var orderStatement = queryString.GetStatement("orderby");
                if (orderStatement is not null)
                {
                    var statementArray = orderStatement.Split(" ");
                    if (statementArray[0] == CustomerCodeFieldName)
                    {
                        ordersQueryable = ordersQueryable.Include(x => x.Customer);
                        ordersQueryable = statementArray.Length > 1 &&
                                          statementArray[1].Equals("desc", StringComparison.InvariantCultureIgnoreCase)
                            ? ordersQueryable.OrderByDescending(x => x.Customer.Code)
                            : ordersQueryable.OrderBy(x => x.Customer.Code);
                    }
                }

                return ordersQueryable;
            };

            return new Dictionary<string, Expression<Func<IQueryable, string, ODataQueryString, IQueryable>>>
            {
                [OrderDetailsFieldName] = (queryable, s, arg3) => orderDetailsCustomization(queryable, s, arg3),
                [CustomerCodeFieldName] = (queryable, s, arg3) => customerCodeCustomization(queryable, s, arg3)
            };
        }
    }
}
