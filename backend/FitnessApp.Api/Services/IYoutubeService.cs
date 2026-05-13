namespace FitnessApp.Api.Services;
public interface IYoutubeService
{
    Task<YoutubeMetadata> GetMetadataAsync(string youtubeUrl, CancellationToken cancellationToken);
}