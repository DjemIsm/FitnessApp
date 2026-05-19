using FitnessApp.Api.Data;
using FitnessApp.Api.DTOs;
using FitnessApp.Api.Models;
using FitnessApp.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FitnessApp.Api.Tests.Services;

public class WorkoutServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mail:ToEmail"] = "test@example.com"
            })
            .Build();
    }

    private static YoutubeMetadata Metadata(string videoId = "abc123")
    {
        return new YoutubeMetadata(
            videoId,
            "Test Workout",
            "Test Channel",
            "https://example.com/thumb.jpg",
            "PT10M"
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldSaveWorkout_WhenWorkoutDoesNotExist()
    {
        await using var db = CreateDbContext();

        var youtube = new Mock<IYoutubeService>();
        youtube
            .Setup(x => x.GetMetadataAsync(
                "https://www.youtube.com/watch?v=abc123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Metadata());

        var email = new Mock<IEmailService>();

        var service = new WorkoutService(
            db,
            youtube.Object,
            email.Object,
            CreateConfiguration());

        var result = await service.CreateAsync(
            new CreateWorkoutRequest("https://www.youtube.com/watch?v=abc123"),
            CancellationToken.None);

        result.Title.Should().Be("Test Workout");
        result.YoutubeVideoId.Should().Be("abc123");

        db.WorkoutVideos.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldNormalizeYoutubeUrl()
    {
        await using var db = CreateDbContext();

        var youtube = new Mock<IYoutubeService>();
        youtube
            .Setup(x => x.GetMetadataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Metadata("xyz789"));

        var service = new WorkoutService(
            db,
            youtube.Object,
            Mock.Of<IEmailService>(),
            CreateConfiguration());

        var result = await service.CreateAsync(
            new CreateWorkoutRequest("https://youtu.be/xyz789"),
            CancellationToken.None);

        result.YoutubeUrl.Should().Be("https://www.youtube.com/watch?v=xyz789");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenWorkoutAlreadyExists()
    {
        await using var db = CreateDbContext();

        db.WorkoutVideos.Add(new WorkoutVideo
        {
            Id = Guid.NewGuid(),
            YoutubeVideoId = "abc123",
            YoutubeUrl = "https://www.youtube.com/watch?v=abc123",
            Title = "Existing Workout",
            ChannelTitle = "Channel",
            ThumbnailUrl = "https://example.com/thumb.jpg",
            DurationIso8601 = "PT10M",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var youtube = new Mock<IYoutubeService>();
        youtube
            .Setup(x => x.GetMetadataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Metadata("abc123"));

        var service = new WorkoutService(
            db,
            youtube.Object,
            Mock.Of<IEmailService>(),
            CreateConfiguration());

        Func<Task> act = () => service.CreateAsync(
            new CreateWorkoutRequest("https://www.youtube.com/watch?v=abc123"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Workout already exists.");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnWorkoutsNewestFirst()
    {
        await using var db = CreateDbContext();

        db.WorkoutVideos.AddRange(
            new WorkoutVideo
            {
                Id = Guid.NewGuid(),
                YoutubeVideoId = "old",
                YoutubeUrl = "https://www.youtube.com/watch?v=old",
                Title = "Old",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
            },
            new WorkoutVideo
            {
                Id = Guid.NewGuid(),
                YoutubeVideoId = "new",
                YoutubeUrl = "https://www.youtube.com/watch?v=new",
                Title = "New",
                CreatedAtUtc = DateTime.UtcNow
            });

        await db.SaveChangesAsync();

        var service = new WorkoutService(
            db,
            Mock.Of<IYoutubeService>(),
            Mock.Of<IEmailService>(),
            CreateConfiguration());

        var result = await service.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("New");
        result[1].Title.Should().Be("Old");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveExistingWorkout()
    {
        await using var db = CreateDbContext();

        var id = Guid.NewGuid();

        db.WorkoutVideos.Add(new WorkoutVideo
        {
            Id = id,
            YoutubeVideoId = "abc123",
            YoutubeUrl = "https://www.youtube.com/watch?v=abc123",
            Title = "Workout"
        });

        await db.SaveChangesAsync();

        var service = new WorkoutService(
            db,
            Mock.Of<IYoutubeService>(),
            Mock.Of<IEmailService>(),
            CreateConfiguration());

        await service.DeleteAsync(id, CancellationToken.None);

        db.WorkoutVideos.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenWorkoutDoesNotExist()
    {
        await using var db = CreateDbContext();

        var service = new WorkoutService(
            db,
            Mock.Of<IYoutubeService>(),
            Mock.Of<IEmailService>(),
            CreateConfiguration());

        Func<Task> act = () => service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetRandomActiveWorkoutAsync_ShouldThrow_WhenNoActiveWorkoutExists()
    {
        await using var db = CreateDbContext();

        var service = new WorkoutService(
            db,
            Mock.Of<IYoutubeService>(),
            Mock.Of<IEmailService>(),
            CreateConfiguration());

        Func<Task> act = () => service.GetRandomActiveWorkoutAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No active workouts found.");
    }

    [Fact]
    public async Task GetRandomActiveWorkoutAsync_ShouldReturnOnlyActiveWorkout()
    {
        await using var db = CreateDbContext();

        db.WorkoutVideos.AddRange(
            new WorkoutVideo
            {
                Id = Guid.NewGuid(),
                YoutubeVideoId = "inactive",
                YoutubeUrl = "https://www.youtube.com/watch?v=inactive",
                Title = "Inactive",
                IsActive = false
            },
            new WorkoutVideo
            {
                Id = Guid.NewGuid(),
                YoutubeVideoId = "active",
                YoutubeUrl = "https://www.youtube.com/watch?v=active",
                Title = "Active",
                IsActive = true
            });

        await db.SaveChangesAsync();

        var service = new WorkoutService(
            db,
            Mock.Of<IYoutubeService>(),
            Mock.Of<IEmailService>(),
            CreateConfiguration());

        var result = await service.GetRandomActiveWorkoutAsync(CancellationToken.None);

        result.Title.Should().Be("Active");
    }

    [Fact]
    public async Task SendRandomWorkoutMailAsync_ShouldSendEmailAndUpdateLastSentAt()
    {
        await using var db = CreateDbContext();

        var workoutId = Guid.NewGuid();

        db.WorkoutVideos.Add(new WorkoutVideo
        {
            Id = workoutId,
            YoutubeVideoId = "abc123",
            YoutubeUrl = "https://www.youtube.com/watch?v=abc123",
            Title = "Workout",
            IsActive = true
        });

        await db.SaveChangesAsync();

        var email = new Mock<IEmailService>();

        var service = new WorkoutService(
            db,
            Mock.Of<IYoutubeService>(),
            email.Object,
            CreateConfiguration());

        var result = await service.SendRandomWorkoutMailAsync(CancellationToken.None);

        result.WorkoutId.Should().Be(workoutId);
        result.Title.Should().Be("Workout");
        result.ToEmail.Should().Be("test@example.com");

        email.Verify(
            x => x.SendDailyWorkoutAsync(
                It.Is<WorkoutVideo>(w => w.Id == workoutId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        var savedWorkout = await db.WorkoutVideos.SingleAsync();
        savedWorkout.LastSentAtUtc.Should().NotBeNull();
    }
}