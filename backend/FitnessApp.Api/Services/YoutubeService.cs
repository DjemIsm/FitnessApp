using System.Text.Json;
using Microsoft.Extensions.Options;

namespace FitnessApp.Api.Services;

public sealed class YoutubeService(
    HttpClient httpClient,
    IOptions<YoutubeOptions> options) : IYoutubeService
{
    public async Task<YoutubeMetadata> GetMetadataAsync(string youtubeUrl, CancellationToken cancellationToken)
    {
        var videoId = YoutubeUrlParser.ExtractVideoId(youtubeUrl);
        var apiKey = options.Value.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("YouTube API key is missing.");

        var requestUrl =
            $"https://www.googleapis.com/youtube/v3/videos?part=snippet,contentDetails&id={Uri.EscapeDataString(videoId)}&key={Uri.EscapeDataString(apiKey)}";

        using var response = await httpClient.GetAsync(requestUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var items = json.RootElement.GetProperty("items");

        if (items.GetArrayLength() == 0)
            throw new InvalidOperationException("YouTube video was not found or is not accessible.");

        var item = items[0];
        var snippet = item.GetProperty("snippet");
        var contentDetails = item.GetProperty("contentDetails");

        string? thumbnail = null;

        if (snippet.TryGetProperty("thumbnails", out var thumbnails))
        {
            if (thumbnails.TryGetProperty("medium", out var medium))
                thumbnail = medium.GetProperty("url").GetString();
            else if (thumbnails.TryGetProperty("default", out var def))
                thumbnail = def.GetProperty("url").GetString();
        }

        return new YoutubeMetadata(
            videoId,
            snippet.GetProperty("title").GetString() ?? "Untitled video",
            snippet.GetProperty("channelTitle").GetString(),
            thumbnail,
            contentDetails.GetProperty("duration").GetString());
    }
}