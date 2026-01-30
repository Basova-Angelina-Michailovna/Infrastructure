using AwesomeAssertions;
using StretchRoom.Infrastructure.Extensions;
using StretchRoom.Infrastructure.Tests.ScheduledTests.Helpers;

namespace StretchRoom.Infrastructure.Tests.ScheduledTests;

public class SchedulingTests
{
    [Test]
    public async Task When_ServiceStarted_With_ScheduledJob_Result_JobTriggered()
    {
        await Task.WaitUntilAsync(_ => ScheduledTestJob.IsCalled, TimeSpan.FromSeconds(5));

        ScheduledTestJob.IsCalled.Should().BeTrue();
        ScheduledTestJob.Times.Should().BeGreaterThan(0);
    }
}