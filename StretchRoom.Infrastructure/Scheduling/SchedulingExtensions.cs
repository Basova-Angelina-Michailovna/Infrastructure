using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using StretchRoom.Infrastructure.Services.ExecutedServices;

namespace StretchRoom.Infrastructure.Scheduling;

/// <summary>
///     The <see cref="IServiceCollection" /> extensions class for scheduling processes.
/// </summary>
[PublicAPI]
public static class SchedulingExtensions
{
    /// <summary>
    ///     The <see cref="IServiceCollection" /> extensions.
    /// </summary>
    /// <param name="services"></param>
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds the scheduling services to <paramref name="services" />.
        /// </summary>
        /// <param name="waitForJobsToComplete">True for set wait for scheduled jobs ends until running next.</param>
        /// <param name="configureSchedulerJobs">The jobs' configuration.</param>
        /// <returns>The instance of <paramref name="services" />.</returns>
        public IServiceCollection AddSchedulingServices(
            bool waitForJobsToComplete,
            Func<ISchedulerFactory, Task> configureSchedulerJobs)
        {
            services.AddQuartz(opts =>
            {
                opts.UseInMemoryStore();
                opts.InterruptJobsOnShutdownWithWait = true;
                opts.InterruptJobsOnShutdown = true;
                opts.UseDefaultThreadPool();
            });
            services.AddQuartzHostedService(opts =>
            {
                opts.WaitForJobsToComplete = waitForJobsToComplete;
                opts.AwaitApplicationStarted = false;
            });

            services.AddOptions<ScheduledJobsRunnerOptions>()
                .Configure(opts => { opts.ConfigureSchedulerJobs = configureSchedulerJobs; });

            services.AddBeforeHostingStarted<ScheduledJobsRunner>();

            return services;
        }
    }

    /// <summary>
    /// The <see cref="ISchedulerFactory"/> extensions.
    /// </summary>
    /// <param name="schedulerFactory">The scheduler factory.</param>
    extension(ISchedulerFactory schedulerFactory)
    {
        /// <summary>
        /// Schedules the <typeparamref name="TJob"/> with specified <paramref name="interval"/>.
        /// </summary>
        /// <param name="jobIdentity">The job identity.</param>
        /// <param name="interval">The job execution interval.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <typeparam name="TJob">The job to schedule.</typeparam>
        public async Task ScheduleJobAsync<TJob>(
            string jobIdentity,
            TimeSpan interval,
            TimeSpan delay = default,
            CancellationToken cancellationToken = default)
            where TJob : IJob
        {
            var jobDetail = JobBuilder.Create<TJob>()
                .WithIdentity(jobIdentity)
                .Build();

            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            var trigger = TriggerBuilder.Create()
                .StartAt(DateTimeOffset.Now.Add(delay))
                .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
        }
        
        /// <summary>
        /// Schedules the <typeparamref name="TJob"/> with specified <paramref name="cronExpression"/>.
        /// </summary>
        /// <param name="jobIdentity">The job identity.</param>
        /// <param name="cronExpression">The job execution cron expression.</param>
        /// <param name="delay">The delay.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <typeparam name="TJob">The job to schedule.</typeparam>
        public async Task ScheduleJobAsync<TJob>(
            string jobIdentity,
            string cronExpression,
            TimeSpan delay = default,
            CancellationToken cancellationToken = default)
            where TJob : IJob
        {
            var jobDetail = JobBuilder.Create<TJob>()
                .WithIdentity(jobIdentity)
                .Build();

            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            var trigger = TriggerBuilder.Create()
                .StartAt(DateTimeOffset.Now.Add(delay))
                .WithCronSchedule(cronExpression)
                .Build();

            await scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
        }
    }
}