using FitnessApp.Api.Models;

namespace FitnessApp.Api.Services;

public interface IEmailService
{
    Task SendDailyWorkoutAsync(WorkoutVideo workout, CancellationToken cancellationToken);
}