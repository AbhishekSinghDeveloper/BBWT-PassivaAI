using BBWM.Core.Filters;
using BBWM.Scheduler.DTO;
using BBWM.Scheduler.Service;
using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace BBWM.Scheduler.Controllers;

[Route("api/scheduler")]
public class SchedulerJobController : Controller
{
    private readonly ISchedulerJobService _jobService;

    public SchedulerJobController(ISchedulerJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpGet("overview/{view}")]
    public async Task<IActionResult> GetOverview(string? view, CancellationToken ct = default)
    {
        var overview = await _jobService.GetOverviewAsync(view, ct);
        return Ok(overview);
    }

    [HttpGet("{status}")]
    public async Task<IActionResult> GetJobsByStatus(string status, [FromQuery] QueryCommand command, CancellationToken ct = default)
    {
        var jobs = await _jobService.GetJobsByStatusAsync(status, command, ct);
        return Ok(jobs);
    }

    [HttpGet("details/{jobId}")]
    public async Task<IActionResult> GetJobDetails(int jobId, CancellationToken ct = default)
    {
        var job = await _jobService.GetJobDetailsAsync(jobId, ct);
        if (job == null)
            return NotFound();

        return Ok(job);
    }

    [HttpGet("retried")]
    public async Task<IActionResult> GetRetriedJobs([FromQuery] QueryCommand command, CancellationToken ct = default)
    {
        var retriedJobs = await _jobService.GetRetriedJobsAsync(command, ct);
        return Ok(retriedJobs);
    }

    [HttpGet("servers")]
    public async Task<IActionResult> GetServers([FromQuery] QueryCommand command, CancellationToken ct = default)
    {
        var servers = await _jobService.GetServersAsync(command, ct);

        return Ok(servers);
    }

    [HttpGet("recurring")]
    public async Task<IActionResult> GetRecurringJobs([FromQuery] QueryCommand command, CancellationToken ct = default)
    {
        var jobStatuses = await _jobService.GetRecurringJobsAsync(command, ct);
        return Ok(jobStatuses);
    }

    [HttpPost("pause/{jobId}")]
    public async Task<IActionResult> PauseJob(int jobId, CancellationToken ct = default)
    {
        var result = await _jobService.PauseJobAsync(jobId, ct);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpPost("resume/{jobId}")]
    public async Task<IActionResult> ResumeJob(int jobId, CancellationToken ct = default)
    {
        var result = await _jobService.ResumeJobAsync(jobId, ct);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpPost("retries/{jobId}")]
    public async Task<IActionResult> RetryJob(int jobId, CancellationToken ct = default)
    {
        var result = await _jobService.RetryJobAsync(jobId, ct);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpPost("trigger/{jobId}")]
    public async Task<IActionResult> TriggerJob(int jobId, CancellationToken ct = default)
    {
        var result = await _jobService.TriggerJobAsync(jobId, null, ct);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpDelete("{jobId}")]
    public async Task<IActionResult> DeleteJob(int jobId, CancellationToken ct = default)
    {
        var result = await _jobService.DeleteJobAsync(jobId, ct);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpGet("exists/{name}")]
    public async Task<IActionResult> RuleExistsAsync(string name, CancellationToken cancellationToken = default)
      => Ok(await _jobService.RuleExistsAsync(name, cancellationToken));


    [HttpPost("saveJob")]
    public async Task<IActionResult> SaveJob([FromBody] JobScheduleDTO request, CancellationToken cancellationToken = default)
    {
        if (!CronExpression.IsValidExpression(request.CronExpression))
        {
            return BadRequest("Invalid cron expression");
        }
        
        return Ok(await _jobService.SaveJobAsync(request.JobName, request.CronExpression, cancellationToken));
    }
}