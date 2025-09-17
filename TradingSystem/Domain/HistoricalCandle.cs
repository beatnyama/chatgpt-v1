using System;

namespace TradingSystem.Domain;

/// <summary>
/// Represents a single historical market data bar (OHLC).
/// </summary>
public sealed record HistoricalCandle(
    DateTime Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume)
{
    public decimal TypicalPrice => (High + Low + Close) / 3m;
}
