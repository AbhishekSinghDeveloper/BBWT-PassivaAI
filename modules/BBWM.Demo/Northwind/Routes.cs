using BBWM.Core.Web;

namespace BBWM.Demo.Northwind;

public static class Routes
{
    private static readonly RouteBuilder NorthwindBuilder = new("/app/demo/northwind");

    public static readonly Route Customers = NorthwindBuilder.Build("customers", "Customers");
    public static readonly Route Employees = NorthwindBuilder.Build("employees", "Employees");
    public static readonly Route Orders = NorthwindBuilder.Build("orders", "Orders");
    public static readonly Route OrderDetails = NorthwindBuilder.Build("order-details", "Order Details");
    public static readonly Route Products = NorthwindBuilder.Build("products", "Products");
}
