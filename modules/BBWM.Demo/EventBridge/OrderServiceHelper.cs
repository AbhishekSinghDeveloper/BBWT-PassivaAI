using BBWM.Core.Services;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;

namespace BBWM.Demo.EventBridge;

internal static class OrderServiceHelper
{
    public static async Task<OrderDTO> FindOrCreateOrderAsync(
        IDataService<IDemoDataContext> service, DateTime orderDate, CancellationToken cancellationToken)
    {
        if (service is null)
        { return null; }

        var order = await service.Get<Order, OrderDTO>(
            query => query.Where(o => o.OrderDate >= orderDate && o.OrderDate <= orderDate),
            cancellationToken);

        if (order is null)
        {
            order = await service.Create<Order, OrderDTO>(
              new OrderDTO
              {
                  OrderDate = orderDate,
                  ShippedDate = orderDate
              },
              cancellationToken);
        }

        return order;
    }

    public static async Task IncreaseShippedDateAndSaveAsync(
        IDataService<IDemoDataContext> service, OrderDTO order, CancellationToken cancellationToken)
    {
        order.ShippedDate = order.ShippedDate.Value.AddDays(1);
        await service.Update<Order, OrderDTO>(order, cancellationToken);
    }
}
