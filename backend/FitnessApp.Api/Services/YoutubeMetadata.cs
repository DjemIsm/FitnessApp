namespace FitnessApp.Api.Services;

public sealed record YoutubeMetadata(
    string VideoId,
    string Title,
    string? ChannelTitle,
    string? ThumbnailUrl,
    string? DurationIso8601);
