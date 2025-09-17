using System.Net.Http;
using TradingSystem.Analytics;
using TradingSystem.Configuration;
using TradingSystem.Data;
using TradingSystem.Execution;

namespace TradingSystem.Runtime;

/// <summary>
/// Helper that wires together the trading system with sensible defaults.
/// </summary>
public static class TradingSystemFactory
{
    public static TradingEngine CreateDefault(TradingConfiguration? configuration = null, HttpClient? httpClient = null)
    {
        configuration ??= new TradingConfiguration();
        httpClient ??= new HttpClient();

        var dataSource = new HttpMarketDataSource(httpClient);
        var analyzer = new TrendAnalyzer();
        var signalGenerator = new SignalGenerator(analyzer);
        var portfolio = new PortfolioManager(configuration.InitialCapital);
        var riskManager = new RiskManager();

        return new TradingEngine(dataSource, signalGenerator, configuration, portfolio, riskManager);
    }
}
