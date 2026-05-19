using System.Net;
using System.Net.Http.Json;
using FitnessApp.Api.DTOs;
using FitnessApp.Api.Models;
using FitnessApp.Api.Services;
using FluentAssertions;
using Moq;

namespace FitnessApp.Api.Tests.Integration;

public class WorkoutApiTests
{
    [Fact]
    public async Task Health_ShouldReturnOk()
    {
        await using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetWorkouts_ShouldReturnEmptyArray_WhenNoWorkoutsExist()
    {
        await using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/workouts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var workouts = await response.Content.ReadFromJsonAsync<List<WorkoutResponse>>();
        workouts.Should().NotBeNull();
        workouts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWorkouts_ShouldReturnSavedWorkout()
    {
        await using var factory = new TestAppFactory();

        await factory.SeedWorkoutAsync(new WorkoutVideo
        {
            Id = Guid.NewGuid(),
            YoutubeVideoId = "abc123",
            YoutubeUrl = "https://www.youtube.com/watch?v=abc123",
            Title = "Seeded Workout",
            ChannelTitle = "Seeded Channel",
            ThumbnailUrl = "https://example.com/thumb.jpg",
            DurationIso8601 = "PT10M",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/workouts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var workouts = await response.Content.ReadFromJsonAsync<List<WorkoutResponse>>();
        workouts.Should().ContainSingle();
        workouts![0].Title.Should().Be("Seeded Workout");
    }

    [Fact]
    public async Task PostWorkout_ShouldCreateWorkout_WhenRequestIsValid()
    {
        await using var factory = new TestAppFactory();

        factory.YoutubeServiceMock
            .Setup(x => x.GetMetadataAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new YoutubeMetadata(
                "abc123",
                "Created Workout",
                "Created Channel",
                "https://example.com/thumb.jpg",
                "PT20M"));

        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/workouts",
            new CreateWorkoutRequest("https://www.youtube.com/watch?v=abc123"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var workout = await response.Content.ReadFromJsonAsync<WorkoutResponse>();
        workout.Should().NotBeNull();
        workout!.Title.Should().Be("Created Workout");
    }

    [Fact]
    public async Task PostWorkout_ShouldReturnBadRequest_WhenYoutubeServiceThrowsArgumentException()
    {
        await using var factory = new TestAppFactory();

        factory.YoutubeServiceMock
            .Setup(x => x.GetMetadataAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid YouTube URL."));

        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/workouts",
            new CreateWorkoutRequest("invalid-url"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteWorkout_ShouldReturnNoContent()
    {
        await using var factory = new TestAppFactory();

        var id = Guid.NewGuid();

        await factory.SeedWorkoutAsync(new WorkoutVideo
        {
            Id = id,
            YoutubeVideoId = "abc123",
            YoutubeUrl = "https://www.youtube.com/watch?v=abc123",
            Title = "Workout"
        });

        var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/workouts/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SendRandomWorkout_ShouldReturnBadRequest_WhenNoWorkoutExists()
    {
        await using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/workouts/send-random", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendRandomWorkout_ShouldReturnOk_WhenWorkoutExists()
    {
        await using var factory = new TestAppFactory();

        await factory.SeedWorkoutAsync(new WorkoutVideo
        {
            Id = Guid.NewGuid(),
            YoutubeVideoId = "abc123",
            YoutubeUrl = "https://www.youtube.com/watch?v=abc123",
            Title = "Workout",
            IsActive = true
        });

        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/workouts/send-random", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        factory.EmailServiceMock.Verify(
            x => x.SendDailyWorkoutAsync(
                It.IsAny<WorkoutVideo>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}