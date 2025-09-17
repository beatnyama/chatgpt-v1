using System;
using System.Collections.Generic;
using TradingSystem.Domain;

namespace TradingSystem.Configuration;

/// <summary>
/// Provides the tunable knobs used by the trading engine.
/// </summary>
public sealed class TradingConfiguration
{
    public decimal InitialCapital { get; init; } = 100m;

    public decimal TargetProfit { get; init; } = 2500m;

    public TimeSpan TargetTimeWindow { get; init; } = TimeSpan.FromHours(48);

    public DateTime HistoricalDataStart { get; init; } = new(1980, 1, 1);

    public DateTime HistoricalDataEnd { get; init; } = DateTime.UtcNow;

    public IReadOnlyList<CurrencyPair> CurrencyPairs { get; init; } = new[]
    {
        new CurrencyPair("USD", "AUD"),
        new CurrencyPair("EUR", "USD"),
        new CurrencyPair("GBP", "USD"),
        new CurrencyPair("USD", "JPY")
    };

    /// <summary>
    /// Percentage of capital risked per trade.
    /// </summary>
    public decimal RiskPerTrade { get; init; } = 0.01m;

    public int ShortWindow { get; init; } = 12;

    public int LongWindow { get; init; } = 48;

    public decimal StopLossFactor { get; init; } = 0.005m;

    public decimal TakeProfitFactor { get; init; } = 0.01m;
}
