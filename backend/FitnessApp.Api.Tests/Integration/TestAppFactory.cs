using FitnessApp.Api.Data;
using FitnessApp.Api.Models;
using FitnessApp.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FitnessApp.Api.Tests.Integration;

public sealed class TestAppFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public Mock<IYoutubeService> YoutubeServiceMock { get; } = new();
    public Mock<IEmailService> EmailServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext))
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            var youtubeDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IYoutubeService));

            if (youtubeDescriptor is not null)
            {
                services.Remove(youtubeDescriptor);
            }

            var emailDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IEmailService));

            if (emailDescriptor is not null)
            {
                services.Remove(emailDescriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.AddSingleton(YoutubeServiceMock.Object);
            services.AddSingleton(EmailServiceMock.Object);
        });
    }

    public async Task SeedWorkoutAsync(WorkoutVideo workout)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.WorkoutVideos.Add(workout);
        await db.SaveChangesAsync();
    }
}