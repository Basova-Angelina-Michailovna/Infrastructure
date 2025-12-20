using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using StretchRoom.Infrastructure.Services.ExecutedServices;

namespace StretchRoom.Infrastructure.Scheduling;

internal class ScheduledJobsRunner(
    ISchedulerFactory schedulerFactory,
    IOptions<ScheduledJobsRunnerOptions> options,
    ILogger<ScheduledJobsRunner> logger)
    : IBeforeHostingStartedService
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Scheduling jobs configuring...");
        return options.Value.ConfigureSchedulerJobs(schedulerFactory);
    }
}

internal class ScheduledJobsRunnerOptions
{
    public required Func<ISchedulerFactory, Task> ConfigureSchedulerJobs { get; internal set; }
}