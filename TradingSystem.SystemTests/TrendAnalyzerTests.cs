using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystem.Analytics;
using TradingSystem.Domain;
using Xunit;

namespace TradingSystem.SystemTests;

public class TrendAnalyzerTests
{
    private static IReadOnlyList<HistoricalCandle> CreateCandles(params decimal[] closes)
    {
        var start = new DateTime(2024, 1, 1);
        return closes.Select((close, index) =>
            new HistoricalCandle(start.AddDays(index), close, close, close, close, 1m)).ToArray();
    }

    [Fact]
    public void Analyze_BullishMomentum_ReturnsBullish()
    {
        var analyzer = new TrendAnalyzer();
        var candles = CreateCandles(1.0m, 1.1m, 1.2m, 1.3m, 1.4m, 1.5m);

        var trend = analyzer.Analyze(candles, shortWindow: 3, longWindow: 5, out var momentum, out var volatility);

        Assert.Equal(MarketTrend.Bullish, trend);
        Assert.True(momentum > 0m);
        Assert.True(volatility >= 0m);
    }

    [Fact]
    public void Analyze_BearishMomentum_ReturnsBearish()
    {
        var analyzer = new TrendAnalyzer();
        var candles = CreateCandles(1.5m, 1.4m, 1.3m, 1.2m, 1.1m, 1.0m);

        var trend = analyzer.Analyze(candles, shortWindow: 3, longWindow: 5, out var momentum, out var _);

        Assert.Equal(MarketTrend.Bearish, trend);
        Assert.True(momentum < 0m);
    }
}
