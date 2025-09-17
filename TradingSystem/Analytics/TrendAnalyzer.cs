using System;
using System.Linq;
using TradingSystem.Domain;

namespace TradingSystem.Analytics;

/// <summary>
/// Performs momentum and volatility analysis over historical data.
/// </summary>
public sealed class TrendAnalyzer
{
    public MarketTrend Analyze(
        IReadOnlyList<HistoricalCandle> candles,
        int shortWindow,
        int longWindow,
        out decimal momentum,
        out decimal volatility)
    {
        if (candles.Count < longWindow || shortWindow <= 0 || longWindow <= 0 || shortWindow >= longWindow)
        {
            momentum = 0m;
            volatility = 0m;
            return MarketTrend.Sideways;
        }

        var recent = candles.TakeLast(longWindow).ToArray();
        var closes = recent.Select(c => c.Close).ToArray();
        var shortAvg = closes.TakeLast(shortWindow).Average();
        var longAvg = closes.Average();
        momentum = shortAvg - longAvg;

        var mean = closes.Average();
        var variance = closes.Select(c => Math.Pow((double)(c - mean), 2)).Average();
        volatility = (decimal)Math.Sqrt(variance);

        var threshold = mean == 0 ? 0.0001m : (decimal)(0.0005 * (double)mean);

        if (momentum > threshold)
        {
            return MarketTrend.Bullish;
        }

        if (momentum < -threshold)
        {
            return MarketTrend.Bearish;
        }

        return MarketTrend.Sideways;
    }
}
