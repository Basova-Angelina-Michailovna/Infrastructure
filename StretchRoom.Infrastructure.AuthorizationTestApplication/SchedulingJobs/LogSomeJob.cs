using Quartz;

namespace StretchRoom.Infrastructure.AuthorizationTestApplication.SchedulingJobs;

public class LogSomeJob(ILogger<LogSomeJob> logger) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("LogSomeJob");
        return Task.CompletedTask;
    }
}