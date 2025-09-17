using System;

namespace TradingSystem.Execution;

/// <summary>
/// Calculates position sizes based on capital and risk appetite.
/// </summary>
public sealed class RiskManager
{
    public decimal CalculatePositionSize(decimal availableCapital, decimal price, decimal riskPerTrade)
    {
        if (availableCapital <= 0m)
        {
            return 0m;
        }

        var capitalAtRisk = availableCapital * riskPerTrade;
        return Math.Max(0m, capitalAtRisk / Math.Max(0.0001m, price));
    }
}
