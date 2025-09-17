using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradingSystem.Domain;

namespace TradingSystem.Data;

/// <summary>
/// Provides historical candles for a given currency pair.
/// </summary>
public interface IMarketDataSource
{
    Task<IReadOnlyList<HistoricalCandle>> GetHistoricalDataAsync(
        CurrencyPair pair,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);
}
