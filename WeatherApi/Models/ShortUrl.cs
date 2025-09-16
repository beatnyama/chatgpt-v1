using System.Threading;

namespace WeatherApi.Models;

public class ShortUrl
{
    private long _accessCount;

    public ShortUrl(string code, string originalUrl, DateTimeOffset createdAt, DateTimeOffset? expiresAt)
    {
        Code = code;
        OriginalUrl = originalUrl;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public string Code { get; }

    public string OriginalUrl { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? ExpiresAt { get; }

    public long AccessCount => Interlocked.Read(ref _accessCount);

    public DateTimeOffset? LastAccessedAt { get; private set; }

    public bool IsExpired(DateTimeOffset now) => ExpiresAt.HasValue && ExpiresAt.Value <= now;

    public void RegisterAccess(DateTimeOffset timestamp)
    {
        Interlocked.Increment(ref _accessCount);
        LastAccessedAt = timestamp;
    }
}
