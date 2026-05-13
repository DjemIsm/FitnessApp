namespace FitnessApp.Api.DTOs;

public sealed record WorkoutResponse(
    Guid Id,
    string YoutubeVideoId,
    string YoutubeUrl,
    string Title,
    string? ChannelTitle,
    string? ThumbnailUrl,
    string? DurationIso8601,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? LastSentAtUtc);
