using FitnessApp.Api.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FitnessApp.Api.Services;

public sealed class EmailService(IOptions<MailOptions> options) : IEmailService
{
    public async Task SendDailyWorkoutAsync(WorkoutVideo workout, CancellationToken cancellationToken)
    {
        var mail = options.Value;

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(mail.FromEmail));
        message.To.Add(MailboxAddress.Parse(mail.ToEmail));
        message.Subject = $"Dein Workout fuer heute: {workout.Title}";

        message.Body = new TextPart("html")
        {
            Text = $"""
            <h2>Dein Workout fuer heute</h2>
            <p><strong>{System.Net.WebUtility.HtmlEncode(workout.Title)}</strong></p>
            <p>{System.Net.WebUtility.HtmlEncode(workout.ChannelTitle ?? "YouTube")}</p>
            <p><a href="{System.Net.WebUtility.HtmlEncode(workout.YoutubeUrl)}">Workout auf YouTube ansehen</a></p>
            {(workout.ThumbnailUrl is null ? "" : $"""<p><img src="{System.Net.WebUtility.HtmlEncode(workout.ThumbnailUrl)}" alt="Thumbnail" /></p>""")}
            """
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(mail.SmtpHost, mail.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await smtp.AuthenticateAsync(mail.UserName, mail.Password, cancellationToken);
        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}