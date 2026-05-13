namespace FitnessApp.Api.Services;

public sealed class DailyWorkoutJob(IWorkoutService workoutService)
{
    public Task SendAsync(CancellationToken cancellationToken)
        => workoutService.SendRandomWorkoutMailAsync(cancellationToken);
}