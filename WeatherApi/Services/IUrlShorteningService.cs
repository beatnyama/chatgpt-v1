using System.Diagnostics.CodeAnalysis;
using WeatherApi.Models;

namespace WeatherApi.Services;

public interface IUrlShorteningService
{
    ShortUrl CreateShortUrl(string originalUrl, TimeSpan? timeToLive = null);

    bool TryGetShortUrl(string code, [NotNullWhen(true)] out ShortUrl? shortUrl);

    bool TryRegisterHit(string code, [NotNullWhen(true)] out ShortUrl? shortUrl);
}
