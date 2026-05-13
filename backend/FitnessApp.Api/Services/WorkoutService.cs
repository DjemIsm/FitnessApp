using FitnessApp.Api.DTOs;
using FitnessApp.Api.Data;
using FitnessApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Api.Services;

public sealed class WorkoutService(
    AppDbContext db,
    IYoutubeService youtubeService,
    IEmailService emailService,
    IConfiguration configuration) : IWorkoutService
{
    public async Task<IReadOnlyList<WorkoutResponse>> GetAllAsync(CancellationToken cancellationToken)
        => await db.WorkoutVideos
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

    public async Task<WorkoutResponse> CreateAsync(
        CreateWorkoutRequest request,
        CancellationToken cancellationToken)
    {
        var metadata = await youtubeService.GetMetadataAsync(request.YoutubeUrl, cancellationToken);

        var exists = await db.WorkoutVideos
            .AnyAsync(x => x.YoutubeVideoId == metadata.VideoId, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Workout already exists.");

        var workout = new WorkoutVideo
        {
            Id = Guid.NewGuid(),
            YoutubeVideoId = metadata.VideoId,
            YoutubeUrl = $"https://www.youtube.com/watch?v={metadata.VideoId}",
            Title = metadata.Title,
            ChannelTitle = metadata.ChannelTitle,
            ThumbnailUrl = metadata.ThumbnailUrl,
            DurationIso8601 = metadata.DurationIso8601,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        db.WorkoutVideos.Add(workout);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(workout);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var workout = await db.WorkoutVideos.FindAsync([id], cancellationToken);

        if (workout is null)
            return;

        db.WorkoutVideos.Remove(workout);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<WorkoutVideo> GetRandomActiveWorkoutAsync(CancellationToken cancellationToken)
    {
        var workouts = await db.WorkoutVideos
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);

        if (workouts.Count == 0)
            throw new InvalidOperationException("No active workouts found.");

        return workouts[Random.Shared.Next(workouts.Count)];
    }

    public async Task<SendWorkoutMailResponse> SendRandomWorkoutMailAsync(
        CancellationToken cancellationToken)
    {
        var workout = await GetRandomActiveWorkoutAsync(cancellationToken);

        await emailService.SendDailyWorkoutAsync(workout, cancellationToken);

        workout.LastSentAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var toEmail = configuration["Mail:ToEmail"] ?? "configured recipient";

        return new SendWorkoutMailResponse(workout.Id, workout.Title, toEmail);
    }

    private static WorkoutResponse ToResponse(WorkoutVideo x) => new(
        x.Id,
        x.YoutubeVideoId,
        x.YoutubeUrl,
        x.Title,
        x.ChannelTitle,
        x.ThumbnailUrl,
        x.DurationIso8601,
        x.IsActive,
        x.CreatedAtUtc,
        x.LastSentAtUtc);
}