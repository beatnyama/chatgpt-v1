using System;
using System.Linq;
using TradingSystem.Configuration;
using TradingSystem.Domain;

namespace TradingSystem.Analytics;

/// <summary>
/// Translates trend analysis into actionable trade signals.
/// </summary>
public sealed class SignalGenerator
{
    private readonly TrendAnalyzer _trendAnalyzer;

    public SignalGenerator(TrendAnalyzer trendAnalyzer)
    {
        _trendAnalyzer = trendAnalyzer;
    }

    public TradeSignal Generate(
        CurrencyPair pair,
        IReadOnlyList<HistoricalCandle> candles,
        TradingConfiguration configuration,
        TradePosition? openPosition = null)
    {
        if (candles.Count == 0)
        {
            throw new ArgumentException("Historical candles are required to generate a signal.", nameof(candles));
        }

        var trend = _trendAnalyzer.Analyze(
            candles,
            configuration.ShortWindow,
            configuration.LongWindow,
            out var momentum,
            out var volatility);

        var latest = candles[^1];
        var confidence = ComputeConfidence(momentum, volatility);
        TradeActionType action;
        decimal? stopLoss = null;
        decimal? takeProfit = null;
        string reason;

        if (openPosition is not null)
        {
            var shouldClose = ShouldClosePosition(openPosition, latest, configuration);
            if (shouldClose)
            {
                action = TradeActionType.Close;
                reason = "Exit based on trailing stop or target.";
            }
            else
            {
                action = TradeActionType.Hold;
                reason = "Existing position maintained.";
            }
        }
        else
        {
            switch (trend)
            {
                case MarketTrend.Bullish:
                    action = TradeActionType.Buy;
                    stopLoss = latest.Close * (1m - configuration.StopLossFactor);
                    takeProfit = latest.Close * (1m + configuration.TakeProfitFactor);
                    reason = $"Bullish momentum detected (Δ={momentum:F5}).";
                    break;
                case MarketTrend.Bearish:
                    action = TradeActionType.Sell;
                    stopLoss = latest.Close * (1m + configuration.StopLossFactor);
                    takeProfit = latest.Close * (1m - configuration.TakeProfitFactor);
                    reason = $"Bearish momentum detected (Δ={momentum:F5}).";
                    break;
                default:
                    action = TradeActionType.Hold;
                    reason = "No clear trend signal.";
                    break;
            }
        }

        return new TradeSignal(
            pair,
            action,
            latest.Timestamp,
            latest.Close,
            confidence,
            stopLoss,
            takeProfit,
            reason);
    }

    private static bool ShouldClosePosition(TradePosition position, HistoricalCandle latest, TradingConfiguration configuration)
    {
        var price = latest.Close;
        var threshold = configuration.StopLossFactor / 2m;
        var pnl = position.UnrealizedPnL(price) / Math.Max(1m, position.EntryPrice * position.Quantity);

        if (position.Direction == TradeActionType.Buy)
        {
            var takeProfit = position.EntryPrice * (1m + configuration.TakeProfitFactor);
            var stopLoss = position.EntryPrice * (1m - configuration.StopLossFactor);
            return price >= takeProfit || price <= stopLoss || pnl <= -threshold;
        }

        var shortTakeProfit = position.EntryPrice * (1m - configuration.TakeProfitFactor);
        var shortStopLoss = position.EntryPrice * (1m + configuration.StopLossFactor);
        return price <= shortTakeProfit || price >= shortStopLoss || pnl <= -threshold;
    }

    private static decimal ComputeConfidence(decimal momentum, decimal volatility)
    {
        if (volatility == 0m)
        {
            return Math.Min(1m, Math.Abs(momentum));
        }

        var ratio = Math.Abs(momentum) / (volatility == 0 ? 1m : volatility);
        return Math.Clamp(ratio, 0m, 1m);
    }
}
