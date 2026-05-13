using FitnessApp.Api.DTOs;
using FitnessApp.Api.Models;

namespace FitnessApp.Api.Services;

public interface IWorkoutService
{
    Task<IReadOnlyList<WorkoutResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<WorkoutResponse> CreateAsync(
        CreateWorkoutRequest request,
        CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<WorkoutVideo> GetRandomActiveWorkoutAsync(CancellationToken cancellationToken);

    Task<SendWorkoutMailResponse> SendRandomWorkoutMailAsync(CancellationToken cancellationToken);
}