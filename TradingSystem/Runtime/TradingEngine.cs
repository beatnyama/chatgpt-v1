using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradingSystem.Analytics;
using TradingSystem.Configuration;
using TradingSystem.Data;
using TradingSystem.Domain;
using TradingSystem.Execution;
using TradingSystem.Reporting;

namespace TradingSystem.Runtime;

/// <summary>
/// Coordinates the data, analytics and execution layers.
/// </summary>
public sealed class TradingEngine
{
    private readonly IMarketDataSource _marketDataSource;
    private readonly SignalGenerator _signalGenerator;
    private readonly TradingConfiguration _configuration;
    private readonly PortfolioManager _portfolioManager;
    private readonly RiskManager _riskManager;

    public TradingEngine(
        IMarketDataSource marketDataSource,
        SignalGenerator signalGenerator,
        TradingConfiguration configuration,
        PortfolioManager portfolioManager,
        RiskManager riskManager)
    {
        _marketDataSource = marketDataSource;
        _signalGenerator = signalGenerator;
        _configuration = configuration;
        _portfolioManager = portfolioManager;
        _riskManager = riskManager;
    }

    public async Task<TradingReport> RunAsync(CancellationToken cancellationToken = default)
    {
        var report = new TradingReport(
            _configuration.InitialCapital,
            _configuration.TargetProfit,
            _configuration.TargetTimeWindow);

        var lastPrices = new Dictionary<CurrencyPair, decimal>();

        foreach (var pair in _configuration.CurrencyPairs)
        {
            var candles = await _marketDataSource.GetHistoricalDataAsync(
                pair,
                _configuration.HistoricalDataStart,
                _configuration.HistoricalDataEnd,
                cancellationToken).ConfigureAwait(false);

            if (candles.Count < _configuration.LongWindow)
            {
                continue;
            }

            var buffer = new List<HistoricalCandle>();
            foreach (var candle in candles)
            {
                buffer.Add(candle);
                lastPrices[pair] = candle.Close;
                if (buffer.Count < _configuration.LongWindow)
                {
                    continue;
                }

                var openPosition = _portfolioManager.GetOpenPosition(pair);
                var signal = _signalGenerator.Generate(pair, buffer, _configuration, openPosition);
                report.RecordSignal(signal);

                if (!signal.IsActionable)
                {
                    continue;
                }

                switch (signal.Action)
                {
                    case TradeActionType.Buy:
                    case TradeActionType.Sell:
                    {
                        var quantity = _riskManager.CalculatePositionSize(
                            _portfolioManager.Cash,
                            signal.Price,
                            _configuration.RiskPerTrade);
                        var position = _portfolioManager.OpenPosition(signal, quantity);
                        if (position is not null)
                        {
                            report.RecordPositionOutcome(position);
                        }
                        break;
                    }
                    case TradeActionType.Close:
                    {
                        var closedPosition = _portfolioManager.ClosePosition(signal);
                        if (closedPosition is not null)
                        {
                            report.RecordPositionOutcome(closedPosition);
                        }
                        break;
                    }
                }
            }
        }

        var equity = _portfolioManager.CalculateEquity(lastPrices);
        report.Complete(equity);
        return report;
    }
}
