namespace FitnessApp.Api.DTOs;

public sealed record SendWorkoutMailResponse(
    Guid WorkoutId, 
    string Title, 
    string ToEmail
    );