using FitnessApp.Api.Services;
using FluentAssertions;

namespace FitnessApp.Api.Tests.Services;

public class YoutubeUrlParserTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=abc123", "abc123")]
    [InlineData("https://www.youtube.com/watch?v=abc123&t=30s", "abc123")]
    [InlineData("https://youtu.be/abc123", "abc123")]
    [InlineData("https://www.youtube.com/embed/abc123", "abc123")]
    [InlineData("https://www.youtube.com/shorts/abc123", "abc123")]
    public void ExtractVideoId_ShouldReturnVideoId_ForValidYoutubeUrls(
        string url,
        string expected)
    {
        var result = YoutubeUrlParser.ExtractVideoId(url);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("https://example.com/watch?v=abc123")]
    [InlineData("https://www.youtube.com/watch")]
    [InlineData("https://www.youtube.com/watch?v=")]
    public void ExtractVideoId_ShouldThrowArgumentException_ForInvalidUrls(string url)
    {
        Action act = () => YoutubeUrlParser.ExtractVideoId(url);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExtractVideoId_ShouldThrowArgumentException_WhenIdIsTooLong()
    {
        var longId = new string('a', 33);
        var url = $"https://www.youtube.com/watch?v={longId}";

        Action act = () => YoutubeUrlParser.ExtractVideoId(url);

        act.Should().Throw<ArgumentException>();
    }
}