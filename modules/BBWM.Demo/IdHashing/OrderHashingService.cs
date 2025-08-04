using BBWM.Core.Services;
using BBWM.Demo.Northwind.Model;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Demo.IdHashing;

public interface IOrderHashingService : IEntityQuery<Order>
{ }

public class OrderHashingService : IOrderHashingService
{
    public IQueryable<Order> GetEntityQuery(IQueryable<Order> baseQuery)
        => baseQuery
            .Include(order => order.Customer)
            .Include(order => order.OrderDetails);
}
