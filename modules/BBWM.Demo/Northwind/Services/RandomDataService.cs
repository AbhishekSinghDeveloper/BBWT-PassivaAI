using BBWM.Core.Data;
using BBWM.Demo.Northwind.Model;
using BBWM.Demo.Northwind.Web;

using Bogus;

using Microsoft.AspNetCore.SignalR;

namespace BBWM.Demo.Northwind.Services;

public interface IRandomDataService
{
    Task GenerateOrders(int count, CancellationToken ct = default);
    Task GenerateCustomers(int count, CancellationToken ct = default);
    Task GenerateProducts(int count, CancellationToken ct = default);
    Task GenerateEmployees(int count, CancellationToken ct = default);
}

public class RandomDataService : IRandomDataService
{
    private readonly IDemoDataContext _dataContext;
    private readonly IHubContext<RandomDataHub> _hubContext;

    public RandomDataService(IDemoDataContext dataContext, IHubContext<RandomDataHub> hubContext)
    {
        _dataContext = dataContext;
        _hubContext = hubContext;
    }

    public async Task GenerateCustomers(int count, CancellationToken ct = default)
    {
        var generator = new Faker<Customer>()
            .RuleFor(p => p.Code, s => s.Random.AlphaNumeric(5).ToUpperInvariant())
            .RuleFor(p => p.CompanyName, s => s.Company.CompanyName());

        var list = generator.Generate(count).ToList();
        await SaveGeneratedData(list, ct);
    }

    public async Task GenerateProducts(int count, CancellationToken ct = default)
    {
        var generator = new Faker<Product>()
            .RuleFor(p => p.Title, s => s.Commerce.ProductName());

        var list = generator.Generate(count).ToList();
        await SaveGeneratedData(list, ct);
    }

    public async Task GenerateOrders(int count, CancellationToken ct = default)
    {
        var products = _dataContext.Set<Product>().OrderByDescending(o => o.Id).Take(100).ToList();
        var customers = _dataContext.Set<Customer>().OrderByDescending(o => o.Id).Take(100).ToList();
        var employees = _dataContext.Set<Employee>().OrderByDescending(o => o.Id).Take(100).ToList();

        var orderDetailsGenerator = new Faker<OrderDetails>()
                    .RuleFor(p => p.Product, s => s.PickRandom(products))
                    .RuleFor(p => p.Quantity, s => s.Random.Int(1, 100))
                    .RuleFor(p => p.Price, s => (decimal)0.01 * s.Random.Int(1, 100000))
                    .RuleFor(p => p.IsReseller, s => s.Random.Int(1, 10) % 2 == 0);

        var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, DateTimeKind.Utc);

        var ordersGenerator = new Faker<Order>()
            .RuleFor(p => p.Customer, s => s.PickRandom(customers))
            .RuleFor(p => p.Employee, s => s.PickRandom(employees))
            .RuleFor(p => p.OrderDate, s => s.Date.Recent(2, startDate).Date)
            .RuleFor(p => p.RequiredDate, s => s.Date.Future(1, startDate).Date)
            .RuleFor(p => p.ShippedDate, s => s.Date.Future(1, startDate).Date)
            .RuleFor(p => p.OrderDetails, s => orderDetailsGenerator.Generate(s.Random.Int(1, 10)).ToList())
            .RuleFor(p => p.IsPaid, s => s.Random.Int(1, 10) % 2 == 0);

        var list = ordersGenerator.Generate(count).ToList();
        await SaveGeneratedData(list, ct);
    }

    public async Task GenerateEmployees(int count, CancellationToken ct = default)
    {
        string[] jobRoles = { "Administrator", "Developer", "Manager", "Tester" };

        var generator = new Faker<Employee>()
                .RuleFor(p => p.Name, s => s.Person.FullName)
                .RuleFor(p => p.Age, s => s.Random.Int(18, 65))
                .RuleFor(p => p.Phone, s => s.Person.Phone)
                .RuleFor(p => p.Email, s => s.Person.Email)
                .RuleFor(p => p.RegistrationDate, s => s.Date.Recent(1000).Date)
                .RuleFor(p => p.JobRole, s => jobRoles[s.Random.Int(0, jobRoles.Length - 1)]);

        var list = generator.Generate(count).ToList();
        await SaveGeneratedData(list, ct);
    }

    /// <summary>
    /// Saving generated entities to data context
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="data">A list of generated entities</param>
    /// <param name="ct">
    /// If the cancellation token is set then we suppose the data hub is used then send progress updates.
    /// If not set then we do a single bulk save.
    /// </param>
    /// <returns></returns>
    private async Task SaveGeneratedData<TEntity>(IList<TEntity> data, CancellationToken ct = default)
        where TEntity : class, IEntity
    {
        if (ct == default)
        {
            await _dataContext.Set<TEntity>().AddRangeAsync(data, ct);
            await _dataContext.SaveChangesAsync(ct);
        }
        else
        {
            var counter = 0;
            var dataCount = data.Count;
            int percentCounter = -1;
            int bulkSize = dataCount < 1000 ? 100 : 1000;

            while (counter < dataCount)
            {
                var bulk = data.Skip(counter).Take(bulkSize);
                counter += bulk.Count();

                await _dataContext.Set<TEntity>().AddRangeAsync(bulk, ct);
                await _dataContext.SaveChangesAsync(ct);

                if (ct.IsCancellationRequested) break;

                var percent = decimal.ToInt32((decimal)counter / dataCount * 100);
                if (percent > percentCounter)
                {
                    await _hubContext.Clients.All.SendAsync("Update", percent, ct);
                    percentCounter = percent;
                }
            }

            await _hubContext.Clients.All.SendAsync("Result", ct);
        }
    }
}
