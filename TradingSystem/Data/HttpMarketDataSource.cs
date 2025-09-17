using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TradingSystem.Domain;

namespace TradingSystem.Data;

/// <summary>
/// Downloads historical forex prices from <see href="https://stooq.com/">Stooq</see>.
/// </summary>
public sealed class HttpMarketDataSource : IMarketDataSource
{
    private static readonly IReadOnlyDictionary<string, string> SymbolMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["USD/AUD"] = "usdaud",
        ["EUR/USD"] = "eurusd",
        ["GBP/USD"] = "gbpusd",
        ["USD/JPY"] = "usdjpy"
    };

    private readonly HttpClient _httpClient;
    private readonly Uri _endpoint;

    public HttpMarketDataSource(HttpClient httpClient, Uri? endpoint = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _endpoint = endpoint ?? new Uri("https://stooq.com/");
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TradingSystem", "1.0"));
    }

    public async Task<IReadOnlyList<HistoricalCandle>> GetHistoricalDataAsync(
        CurrencyPair pair,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        if (start >= end)
        {
            throw new ArgumentException("Start date must be before end date.", nameof(start));
        }

        var symbol = ResolveSymbol(pair);
        var requestUri = new Uri(_endpoint, $"q/d/l/?s={symbol}&i=d");

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(contentStream);
        var candles = new List<HistoricalCandle>();
        string? line = await reader.ReadLineAsync().ConfigureAwait(false); // Skip header

        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            if (TryParse(line, out var candle) && candle.Timestamp >= start && candle.Timestamp <= end)
            {
                candles.Add(candle);
            }
        }

        return candles
            .OrderBy(c => c.Timestamp)
            .ToArray();
    }

    private static bool TryParse(string csvLine, out HistoricalCandle candle)
    {
        var parts = csvLine.Split(',');
        if (parts.Length < 6)
        {
            candle = default!;
            return false;
        }

        if (!DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
        {
            candle = default!;
            return false;
        }

        if (!decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var open) ||
            !decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var high) ||
            !decimal.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var low) ||
            !decimal.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var close))
        {
            candle = default!;
            return false;
        }

        decimal volume = 0m;
        _ = parts.Length > 5 && decimal.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out volume);

        candle = new HistoricalCandle(date, open, high, low, close, volume);
        return true;
    }

    private static string ResolveSymbol(CurrencyPair pair)
    {
        var key = pair.Symbol;
        if (!SymbolMap.TryGetValue(key, out var symbol))
        {
            throw new NotSupportedException($"Symbol mapping for {key} is not configured.");
        }

        return symbol;
    }
}
