using Microsoft.AspNetCore.Mvc;
using WeatherApi.Routing;
using WeatherApi.Services;

namespace WeatherApi.Controllers;

[ApiController]
[Route("")]
[ApiExplorerSettings(IgnoreApi = true)]
public class RedirectController : ControllerBase
{
    private readonly IUrlShorteningService _urlShorteningService;

    public RedirectController(IUrlShorteningService urlShorteningService)
    {
        _urlShorteningService = urlShorteningService;
    }

    [HttpGet("{code}", Name = RouteNames.ResolveShortUrl)]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Resolve(string code)
    {
        if (!_urlShorteningService.TryRegisterHit(code, out var shortUrl))
        {
            if (shortUrl is not null && shortUrl.IsExpired(DateTimeOffset.UtcNow))
            {
                return StatusCode(StatusCodes.Status410Gone);
            }

            return NotFound();
        }

        return Redirect(shortUrl.OriginalUrl);
    }
}
