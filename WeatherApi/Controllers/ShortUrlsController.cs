using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using WeatherApi.Models;
using WeatherApi.Routing;
using WeatherApi.Services;

namespace WeatherApi.Controllers;

[ApiController]
[Route("api/urls")]
[Produces(MediaTypeNames.Application.Json)]
public class ShortUrlsController : ControllerBase
{
    private readonly IUrlShorteningService _urlShorteningService;
    private readonly IQrCodeGenerator _qrCodeGenerator;
    private readonly ILogger<ShortUrlsController> _logger;

    public ShortUrlsController(
        IUrlShorteningService urlShorteningService,
        IQrCodeGenerator qrCodeGenerator,
        ILogger<ShortUrlsController> logger)
    {
        _urlShorteningService = urlShorteningService;
        _qrCodeGenerator = qrCodeGenerator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShortUrlDetailsResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ShortUrlDetailsResponse> CreateShortUrl([FromBody] ShortenUrlRequest request)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            ModelState.AddModelError(nameof(request.Url), "The URL to shorten is required.");
            return ValidationProblem(ModelState);
        }

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var originalUri) ||
            (originalUri.Scheme != Uri.UriSchemeHttp && originalUri.Scheme != Uri.UriSchemeHttps))
        {
            ModelState.AddModelError(nameof(request.Url), "A valid absolute HTTP or HTTPS URL is required.");
            return ValidationProblem(ModelState);
        }

        TimeSpan? timeToLive = null;
        if (request.ExpirationInMinutes.HasValue)
        {
            if (request.ExpirationInMinutes.Value <= 0)
            {
                ModelState.AddModelError(nameof(request.ExpirationInMinutes), "Expiration must be greater than zero minutes when provided.");
                return ValidationProblem(ModelState);
            }

            timeToLive = TimeSpan.FromMinutes(request.ExpirationInMinutes.Value);
        }

        ShortUrl shortUrl;
        try
        {
            shortUrl = _urlShorteningService.CreateShortUrl(originalUri.ToString(), timeToLive);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Unable to shorten URL due to invalid input.");
            ModelState.AddModelError(nameof(request.Url), ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create a unique short code.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }

        var response = MapToResponse(shortUrl, request.OutputType);
        return CreatedAtRoute(RouteNames.GetShortUrlDetails, new { code = response.Code }, response);
    }

    [HttpGet("{code}", Name = RouteNames.GetShortUrlDetails)]
    [ProducesResponseType(typeof(ShortUrlDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ShortUrlDetailsResponse> GetDetails(string code)
    {
        if (!_urlShorteningService.TryGetShortUrl(code, out var shortUrl) || shortUrl is null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(shortUrl, ShortenUrlOutputType.ShortUrl));
    }

    private ShortUrlDetailsResponse MapToResponse(ShortUrl shortUrl, ShortenUrlOutputType outputType)
    {
        var shortLink = Url.RouteUrl(RouteNames.ResolveShortUrl, new { code = shortUrl.Code },
            HttpContext.Request.Scheme, HttpContext.Request.Host.ToString())
            ?? $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{shortUrl.Code}";

        string? qrCodeBase64 = null;
        string? qrCodeDataUrl = null;

        if (outputType == ShortenUrlOutputType.QrCode)
        {
            qrCodeBase64 = _qrCodeGenerator.GeneratePngBase64(shortLink);
            qrCodeDataUrl = $"data:image/png;base64,{qrCodeBase64}";
        }

        return new ShortUrlDetailsResponse
        {
            Code = shortUrl.Code,
            OriginalUrl = shortUrl.OriginalUrl,
            ShortUrl = shortLink,
            CreatedAt = shortUrl.CreatedAt,
            ExpiresAt = shortUrl.ExpiresAt,
            IsExpired = shortUrl.IsExpired(DateTimeOffset.UtcNow),
            AccessCount = shortUrl.AccessCount,
            LastAccessedAt = shortUrl.LastAccessedAt,
            OutputType = outputType,
            QrCodeImageBase64 = qrCodeBase64,
            QrCodeImageDataUrl = qrCodeDataUrl
        };
    }
}
