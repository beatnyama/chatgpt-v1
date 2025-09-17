using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradingSystem.Analytics;
using TradingSystem.Configuration;
using TradingSystem.Data;
using TradingSystem.Domain;
using TradingSystem.Execution;
using TradingSystem.Runtime;
using Xunit;

namespace TradingSystem.SystemTests;

public class TradingEngineSystemTests
{
    [Fact]
    public async Task TradingEngine_WithTrendingMarket_ProducesSignalsAndReport()
    {
        var pair = new CurrencyPair("USD", "AUD");
        var candles = GenerateTrendingSeries(startPrice: 0.65m, step: 0.005m, count: 120);
        var config = new TradingConfiguration
        {
            CurrencyPairs = new[] { pair },
            HistoricalDataStart = candles.First().Timestamp,
            HistoricalDataEnd = candles.Last().Timestamp,
            InitialCapital = 1000m,
            RiskPerTrade = 0.2m,
            ShortWindow = 5,
            LongWindow = 20,
            StopLossFactor = 0.01m,
            TakeProfitFactor = 0.02m
        };

        var data = new Dictionary<CurrencyPair, IReadOnlyList<HistoricalCandle>>
        {
            [pair] = candles
        };

        var dataSource = new InMemoryMarketDataSource(data);
        var analyzer = new TrendAnalyzer();
        var signalGenerator = new SignalGenerator(analyzer);
        var portfolio = new PortfolioManager(config.InitialCapital);
        var riskManager = new RiskManager();
        var engine = new TradingEngine(dataSource, signalGenerator, config, portfolio, riskManager);

        var report = await engine.RunAsync();

        Assert.NotNull(report);
        Assert.NotEmpty(report.Signals);
        Assert.True(report.FinalEquity >= 0m);
        Assert.True(report.Signals.Any(s => s.Action is TradeActionType.Buy or TradeActionType.Sell));
    }

    private static IReadOnlyList<HistoricalCandle> GenerateTrendingSeries(decimal startPrice, decimal step, int count)
    {
        var results = new List<HistoricalCandle>(count);
        var price = startPrice;
        var startDate = new DateTime(2020, 1, 1);

        for (var i = 0; i < count; i++)
        {
            price += step;
            var candle = new HistoricalCandle(
                startDate.AddHours(i),
                price,
                price + 0.0005m,
                price - 0.0005m,
                price,
                1000m);
            results.Add(candle);

            if (i % 30 == 0 && i != 0)
            {
                step = -step; // Reverse trend periodically to trigger exits.
            }
        }

        return results;
    }
}
