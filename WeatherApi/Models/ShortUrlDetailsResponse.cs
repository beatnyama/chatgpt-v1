namespace WeatherApi.Models;

public class ShortUrlDetailsResponse
{
    public required string Code { get; init; }

    public required string OriginalUrl { get; init; }

    public required string ShortUrl { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public bool IsExpired { get; init; }

    public long AccessCount { get; init; }

    public DateTimeOffset? LastAccessedAt { get; init; }

    public ShortenUrlOutputType OutputType { get; init; } = ShortenUrlOutputType.ShortUrl;

    public string? QrCodeImageBase64 { get; init; }

    public string? QrCodeImageDataUrl { get; init; }
}
