using Microsoft.AspNetCore.SignalR;

namespace BBWM.Scheduler;

    public class JobStatusHub : Hub
    {
        public async Task SendJobStatusUpdate(string status, string jobName, DateTime executionTime)
        {
            await Clients.All.SendAsync("ReceiveJobStatusUpdate", status, jobName, executionTime);
        }
    }

