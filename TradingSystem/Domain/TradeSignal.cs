using System;

namespace TradingSystem.Domain;

/// <summary>
/// Represents the decision produced by the strategy engine.
/// </summary>
public sealed record TradeSignal(
    CurrencyPair Pair,
    TradeActionType Action,
    DateTime Timestamp,
    decimal Price,
    decimal Confidence,
    decimal? StopLoss,
    decimal? TakeProfit,
    string Reason)
{
    public bool IsActionable => Action != TradeActionType.Hold && Confidence > 0m;
}
