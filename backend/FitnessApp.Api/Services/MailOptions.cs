namespace FitnessApp.Api.Services;

public sealed class MailOptions
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string FromEmail { get; set; }
    public required string ToEmail { get; set; }
}