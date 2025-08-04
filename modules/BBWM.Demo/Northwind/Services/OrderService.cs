using AutoMapper;

using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using BBWM.Core.Services;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Demo.Northwind.Services;

public interface IOrderService :
    IEntityQuery<Order>,
    IEntityPage<OrderDTO>,
    IEntityCreate<OrderDTO>
{
}

public class OrderService : IOrderService
{
    private readonly IDataService<IDemoDataContext> _dataService;
    private readonly IDemoDataContext _dbContext;
    private readonly IMapper _mapper;

    public OrderService(IDataService<IDemoDataContext> dataService, IDemoDataContext dbContext, IMapper mapper)
    {
        _dataService = dataService;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public IQueryable<Order> GetEntityQuery(IQueryable<Order> baseQuery)
        => baseQuery
            .Include(x => x.Customer)
            .Include(x => x.Employee)
            .Include(x => x.OrderDetails)
            .ThenInclude(x => x.Product);

    public Task<PageResult<OrderDTO>> GetPage(QueryCommand command, CancellationToken ct = default)
        => _dataService.GetPage<Order, OrderDTO>(command, GetEntityQuery,
            queryFilter => queryFilter
                .Handle<BooleanFilter>("HasResellerItems",
                    (query, filter) => filter.Value.HasValue ?
                        query.Where(x => filter.Value.Value && x.OrderDetails.Any(y => y.IsReseller)
                            || (!filter.Value.Value && !x.OrderDetails.Any(y => y.IsReseller))) :
                        query)
                .Handle<NumberFilter>(nameof(OrderDetails.Price), ApplyCollectionFilter),
            ct: ct);

    private static IQueryable<Order> ApplyCollectionFilter(IQueryable<Order> query, NumberFilter filter)
    {
        return filter.MatchMode switch
        {
            CountableFilterMatchMode.Equals => query.Where(x => x.OrderDetails.All(y => y.Price == Convert.ToDecimal(filter.Value))),
            CountableFilterMatchMode.GreaterThan => query.Where(x => x.OrderDetails.All(y => y.Price > Convert.ToDecimal(filter.Value))),
            CountableFilterMatchMode.GreaterThanOrEqual => query.Where(x => x.OrderDetails.All(y => y.Price >= Convert.ToDecimal(filter.Value))),
            CountableFilterMatchMode.LessThan => query.Where(x => x.OrderDetails.All(y => y.Price < Convert.ToDecimal(filter.Value))),
            CountableFilterMatchMode.LessThanOrEqual => query.Where(x => x.OrderDetails.All(y => y.Price <= Convert.ToDecimal(filter.Value))),
            _ => query,
        };
    }

    public async Task<OrderDTO> Create(OrderDTO dto, CancellationToken ct = default)
    {
        var customer = await _dbContext.Customers.SingleOrDefaultAsync(c => c.Id == dto.CustomerId);
        var employee = await _dbContext.Employees.SingleOrDefaultAsync(e => e.Id == dto.EmployeeId);

        if (customer is null && dto.Customer is not null)
        {
            customer = _mapper.Map<Customer>(dto.Customer);
            await _dbContext.Customers.AddAsync(customer);
        }

        if (employee is null && dto.Employee is not null)
        {
            employee = _mapper.Map<Employee>(dto.Employee);
            await _dbContext.Employees.AddAsync(employee);
        }

        var order = _mapper.Map<Order>(dto);
        var orderDetails = _mapper.Map<List<OrderDetails>>(dto.OrderDetails);

        order.Customer = customer;
        order.Employee = employee;
        order.OrderDetails = orderDetails;

        await _dbContext.Orders.AddAsync(order, ct);
        await _dbContext.SaveChangesAsync(ct);

        return _mapper.Map<OrderDTO>(order);
    }
}
