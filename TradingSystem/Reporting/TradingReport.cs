using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystem.Domain;

namespace TradingSystem.Reporting;

/// <summary>
/// Collects execution artefacts produced by the trading engine.
/// </summary>
public sealed class TradingReport
{
    private readonly List<TradeSignal> _signals = new();
    private readonly List<TradePosition> _completedPositions = new();

    public TradingReport(decimal initialCapital, decimal targetProfit, TimeSpan horizon)
    {
        InitialCapital = initialCapital;
        TargetProfit = targetProfit;
        TargetHorizon = horizon;
        GeneratedAtUtc = DateTime.UtcNow;
    }

    public decimal InitialCapital { get; }

    public decimal TargetProfit { get; }

    public TimeSpan TargetHorizon { get; }

    public DateTime GeneratedAtUtc { get; }

    public IReadOnlyList<TradeSignal> Signals => _signals;

    public IReadOnlyList<TradePosition> CompletedPositions => _completedPositions;

    public decimal FinalEquity { get; private set; }

    public bool ProfitTargetAchieved => FinalEquity - InitialCapital >= TargetProfit;

    public decimal TotalRealizedProfit => _completedPositions.Sum(p => p.RealizedPnL ?? 0m);

    public void RecordSignal(TradeSignal signal) => _signals.Add(signal);

    public void RecordPositionOutcome(TradePosition position)
    {
        if (position.ClosedAt is not null)
        {
            _completedPositions.Add(position);
        }
    }

    public void Complete(decimal finalEquity)
    {
        FinalEquity = finalEquity;
    }
}
