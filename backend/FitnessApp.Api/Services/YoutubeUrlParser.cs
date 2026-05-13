using Microsoft.AspNetCore.WebUtilities;

namespace FitnessApp.Api.Services;

public static class YoutubeUrlParser
{
    public static string ExtractVideoId(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid YouTube URL.");

        if (uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
        {
            var id = uri.AbsolutePath.Trim('/');
            return Validate(id);
        }

        if (uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase))
        {
            var query = QueryHelpers.ParseQuery(uri.Query);

            if (query.TryGetValue("v", out var values))
                return Validate(values.ToString());

            var parts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2 && parts[0] is "shorts" or "embed")
                return Validate(parts[1]);
        }

        throw new ArgumentException("Could not extract YouTube video id.");
    }

    private static string Validate(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > 32)
            throw new ArgumentException("Invalid YouTube video id.");

        return id;
    }
}