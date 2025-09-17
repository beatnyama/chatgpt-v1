using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradingSystem.Domain;

namespace TradingSystem.Data;

/// <summary>
/// Provides deterministic historical data for testing purposes.
/// </summary>
public sealed class InMemoryMarketDataSource : IMarketDataSource
{
    private readonly IReadOnlyDictionary<CurrencyPair, IReadOnlyList<HistoricalCandle>> _data;

    public InMemoryMarketDataSource(IEnumerable<HistoricalCandle> candles, CurrencyPair pair)
        : this(new Dictionary<CurrencyPair, IReadOnlyList<HistoricalCandle>>
        {
            [pair] = candles.OrderBy(c => c.Timestamp).ToArray()
        })
    {
    }

    public InMemoryMarketDataSource(IReadOnlyDictionary<CurrencyPair, IReadOnlyList<HistoricalCandle>> data)
    {
        _data = data;
    }

    public Task<IReadOnlyList<HistoricalCandle>> GetHistoricalDataAsync(
        CurrencyPair pair,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        if (!_data.TryGetValue(pair, out var candles))
        {
            return Task.FromResult<IReadOnlyList<HistoricalCandle>>(Array.Empty<HistoricalCandle>());
        }

        var result = candles
            .Where(c => c.Timestamp >= start && c.Timestamp <= end)
            .OrderBy(c => c.Timestamp)
            .ToArray();

        return Task.FromResult<IReadOnlyList<HistoricalCandle>>(result);
    }
}
