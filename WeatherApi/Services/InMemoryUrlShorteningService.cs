using System.Collections.Concurrent;
using System.Security.Cryptography;
using WeatherApi.Models;

namespace WeatherApi.Services;

public class InMemoryUrlShorteningService : IUrlShorteningService
{
    private static readonly char[] CodeAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
    private readonly ConcurrentDictionary<string, ShortUrl> _urls = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<InMemoryUrlShorteningService> _logger;
    private readonly int _codeLength;
    private readonly int _maxGenerationAttempts;

    public InMemoryUrlShorteningService(ILogger<InMemoryUrlShorteningService> logger, int codeLength = 8, int maxGenerationAttempts = 20)
    {
        if (codeLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(codeLength), "Code length must be greater than zero.");
        }

        if (maxGenerationAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxGenerationAttempts), "Maximum generation attempts must be greater than zero.");
        }

        _logger = logger;
        _codeLength = codeLength;
        _maxGenerationAttempts = maxGenerationAttempts;
    }

    public ShortUrl CreateShortUrl(string originalUrl, TimeSpan? timeToLive = null)
    {
        if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("The provided value is not a valid absolute HTTP or HTTPS URL.", nameof(originalUrl));
        }

        if (timeToLive.HasValue && timeToLive.Value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeToLive), "Time-to-live must be a positive time span when provided.");
        }

        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = timeToLive.HasValue ? createdAt.Add(timeToLive.Value) : null;

        for (var attempt = 0; attempt < _maxGenerationAttempts; attempt++)
        {
            var code = GenerateCode(_codeLength);
            var shortUrl = new ShortUrl(code, uri.ToString(), createdAt, expiresAt);

            if (_urls.TryAdd(code, shortUrl))
            {
                return shortUrl;
            }

            _logger.LogWarning("Collision detected while generating code {Code} (attempt {Attempt}/{MaxAttempts}).", code, attempt + 1, _maxGenerationAttempts);
        }

        throw new InvalidOperationException("Failed to generate a unique short code. Please try again.");
    }

    public bool TryGetShortUrl(string code, out ShortUrl? shortUrl)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            shortUrl = null;
            return false;
        }

        return _urls.TryGetValue(code, out shortUrl);
    }

    public bool TryRegisterHit(string code, out ShortUrl? shortUrl)
    {
        shortUrl = null;
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        if (!_urls.TryGetValue(code, out var existing))
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        if (existing.IsExpired(now))
        {
            shortUrl = existing;
            return false;
        }

        existing.RegisterAccess(now);
        shortUrl = existing;
        return true;
    }

    private static string GenerateCode(int length)
    {
        Span<byte> buffer = stackalloc byte[length];
        RandomNumberGenerator.Fill(buffer);

        Span<char> chars = stackalloc char[length];
        for (var i = 0; i < length; i++)
        {
            chars[i] = CodeAlphabet[buffer[i] % CodeAlphabet.Length];
        }

        return new string(chars);
    }
}
