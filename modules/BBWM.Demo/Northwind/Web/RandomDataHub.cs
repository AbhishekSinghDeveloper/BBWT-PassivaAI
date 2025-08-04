using BBWM.Core.Tasks;
using BBWM.Demo.Northwind.Services;

using Microsoft.AspNetCore.SignalR;

namespace BBWM.Demo.Northwind.Web;

public class RandomDataHub : Hub
{
    private readonly IRandomDataService _randomDataService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public RandomDataHub(IRandomDataService randomDataService, IBackgroundTaskQueue backgroundTaskQueue)
    {
        _randomDataService = randomDataService;
        _backgroundTaskQueue = backgroundTaskQueue;
    }

    public void GenerateCustomers(int number)
    {
        _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
        {
            await _randomDataService.GenerateCustomers(number, token);
            return null;
        });
    }

    public void GenerateEmployees(int number)
    {
        _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
        {
            await _randomDataService.GenerateEmployees(number, token);
            return null;
        });
    }

    public void GenerateOrders(int number)
    {
        _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
        {
            await _randomDataService.GenerateOrders(number, token);
            return null;
        });
    }
    public void GenerateProducts(int number)
    {
        _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
        {
            await _randomDataService.GenerateProducts(number, token);
            return null;
        });
    }

    public void StopGeneration()
    {
        _backgroundTaskQueue.CancelWorkItem();
        // await _backgroundService.StopAsync(CancellationToken.None);
    }
}
