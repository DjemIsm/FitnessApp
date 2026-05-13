using FitnessApp.Api;
using FitnessApp.Api.DTOs;
using FitnessApp.Api.Data;
using FitnessApp.Api.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = RenderDatabaseUrl.ToNpgsqlConnectionString(
        Environment.GetEnvironmentVariable("DATABASE_URL"));
}

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Database connection string is missing.");

builder.Services.Configure<YoutubeOptions>(
    builder.Configuration.GetSection("YouTube"));

builder.Services.Configure<MailOptions>(
    builder.Configuration.GetSection("Mail"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpClient<IYoutubeService, YoutubeService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddScoped<DailyWorkoutJob>();

var allowedOrigins =
    builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseCors("Frontend");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/workouts", async (
    IWorkoutService service,
    CancellationToken ct) =>
    Results.Ok(await service.GetAllAsync(ct)));

app.MapPost("/api/workouts", async (
    CreateWorkoutRequest request,
    IWorkoutService service,
    CancellationToken ct) =>
{
    try
    {
        var created = await service.CreateAsync(request, ct);
        return Results.Created($"/api/workouts/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/api/workouts/{id:guid}", async (
    Guid id,
    IWorkoutService service,
    CancellationToken ct) =>
{
    await service.DeleteAsync(id, ct);
    return Results.NoContent();
});

app.MapPost("/api/workouts/send-random", async (
    IWorkoutService service,
    CancellationToken ct) =>
{
    try
    {
        return Results.Ok(await service.SendRandomWorkoutMailAsync(ct));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

recurringJobManager.AddOrUpdate<DailyWorkoutJob>(
    "daily-workout-email",
    job => job.SendAsync(CancellationToken.None),
    builder.Configuration["Hangfire:DailyWorkoutCron"] ?? "0 8 * * *",
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            builder.Configuration["Hangfire:TimeZone"] ?? "Europe/Berlin")
    });

app.Run();