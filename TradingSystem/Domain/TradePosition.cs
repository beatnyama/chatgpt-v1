using System;

namespace TradingSystem.Domain;

/// <summary>
/// Represents an open trade in the simulated portfolio.
/// </summary>
public sealed class TradePosition
{
    public TradePosition(CurrencyPair pair, TradeActionType direction, decimal entryPrice, decimal quantity, DateTime openedAt)
    {
        if (direction is not TradeActionType.Buy and not TradeActionType.Sell)
        {
            throw new ArgumentException("Positions must be long or short.", nameof(direction));
        }

        Pair = pair;
        Direction = direction;
        EntryPrice = entryPrice;
        Quantity = quantity;
        OpenedAt = openedAt;
    }

    public CurrencyPair Pair { get; }

    public TradeActionType Direction { get; }

    public decimal EntryPrice { get; }

    public decimal Quantity { get; }

    public DateTime OpenedAt { get; }

    public DateTime? ClosedAt { get; private set; }

    public decimal? ExitPrice { get; private set; }

    public decimal UnrealizedPnL(decimal currentPrice)
    {
        var multiplier = Direction == TradeActionType.Buy ? 1m : -1m;
        return (currentPrice - EntryPrice) * Quantity * multiplier;
    }

    public decimal? RealizedPnL => ExitPrice is null
        ? null
        : (ExitPrice - EntryPrice) * Quantity * (Direction == TradeActionType.Buy ? 1m : -1m);

    public void Close(decimal exitPrice, DateTime closedAt)
    {
        if (ClosedAt is not null)
        {
            throw new InvalidOperationException("Position already closed.");
        }

        ExitPrice = exitPrice;
        ClosedAt = closedAt;
    }
}
