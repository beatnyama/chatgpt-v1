namespace WeatherApi.Models;

public class ShortenUrlRequest
{
    public string? Url { get; set; }

    public int? ExpirationInMinutes { get; set; }

    public ShortenUrlOutputType OutputType { get; set; } = ShortenUrlOutputType.ShortUrl;
}
