using System.Collections.Generic;
using System.Linq;
using TradingSystem.Domain;

namespace TradingSystem.Execution;

/// <summary>
/// Maintains the simulated trading account.
/// </summary>
public sealed class PortfolioManager
{
    private readonly List<TradePosition> _positions = new();

    public PortfolioManager(decimal initialCapital)
    {
        InitialCapital = initialCapital;
        Cash = initialCapital;
    }

    public decimal InitialCapital { get; }

    public decimal Cash { get; private set; }

    public IReadOnlyCollection<TradePosition> Positions => _positions.AsReadOnly();

    public TradePosition? GetOpenPosition(CurrencyPair pair) =>
        _positions.LastOrDefault(p => p.Pair.Equals(pair) && p.ClosedAt is null);

    public TradePosition? OpenPosition(TradeSignal signal, decimal quantity)
    {
        if (signal.Action is not TradeActionType.Buy and not TradeActionType.Sell)
        {
            return null;
        }

        if (quantity <= 0m)
        {
            return null;
        }

        if (GetOpenPosition(signal.Pair) is not null)
        {
            return null;
        }

        var cost = signal.Price * quantity;
        if (signal.Action == TradeActionType.Buy && cost > Cash)
        {
            // Scale down to fit available capital.
            quantity = Cash / signal.Price;
            cost = signal.Price * quantity;
        }

        if (quantity <= 0m)
        {
            return null;
        }

        if (signal.Action == TradeActionType.Buy)
        {
            Cash -= cost;
        }
        else
        {
            Cash += cost;
        }

        var position = new TradePosition(signal.Pair, signal.Action, signal.Price, quantity, signal.Timestamp);
        _positions.Add(position);
        return position;
    }

    public TradePosition? ClosePosition(TradeSignal signal)
    {
        if (signal.Action != TradeActionType.Close)
        {
            return null;
        }

        var position = GetOpenPosition(signal.Pair);
        if (position is null)
        {
            return null;
        }

        position.Close(signal.Price, signal.Timestamp);
        var proceeds = signal.Price * position.Quantity;

        if (position.Direction == TradeActionType.Buy)
        {
            Cash += proceeds;
        }
        else
        {
            Cash -= proceeds;
        }

        return position;
    }

    public decimal CalculateEquity(IReadOnlyDictionary<CurrencyPair, decimal> marketPrices)
    {
        var equity = Cash;
        foreach (var position in _positions.Where(p => p.ClosedAt is null))
        {
            if (!marketPrices.TryGetValue(position.Pair, out var price))
            {
                continue;
            }

            var marketValue = price * position.Quantity;
            equity += position.Direction == TradeActionType.Buy ? marketValue : -marketValue;
        }

        return equity;
    }

    public decimal TotalRealizedProfit() => _positions
        .Where(p => p.ClosedAt is not null)
        .Sum(p => p.RealizedPnL ?? 0m);
}
