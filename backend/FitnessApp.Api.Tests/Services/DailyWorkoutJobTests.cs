using FitnessApp.Api.Services;
using Moq;

namespace FitnessApp.Api.Tests.Services;

public class DailyWorkoutJobTests
{
    [Fact]
    public async Task SendAsync_ShouldCallWorkoutService()
    {
        var workoutService = new Mock<IWorkoutService>();

        var job = new DailyWorkoutJob(workoutService.Object);

        await job.SendAsync(CancellationToken.None);

        workoutService.Verify(
            x => x.SendRandomWorkoutMailAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}