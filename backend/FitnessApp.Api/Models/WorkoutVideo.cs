namespace FitnessApp.Api.Models;
public sealed class WorkoutVideo
{
    public Guid Id {get; set;}
    public required string YoutubeVideoId { get; set; }
    public required string YoutubeUrl { get; set; }
    public required string Title { get; set; }
    public string? ChannelTitle { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? DurationIso8601 { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastSentAtUtc { get; set; }
}