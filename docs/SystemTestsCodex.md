# Trading System System Tests Codex

This codex describes the strategy used to validate the end-to-end behaviour of the automated forex trading engine.

## Objectives

1. **Historical data integration** – ensure the engine can consume long-running historical streams (starting in 1980) by abstracting data providers behind `IMarketDataSource` and providing an HTTP implementation that targets public data archives.
2. **Signal quality** – verify that the analytics layer (trend analysis and signal generation) produces actionable buy/sell/close instructions for supported forex pairs.
3. **Portfolio safety** – confirm that position sizing, stop loss and take profit logic preserve capital and react to market reversals.
4. **Github integration** – provide automated system tests (`TradingSystem.SystemTests`) that can run as part of a CI workflow to guard regressions before merging pull requests.

## Test Architecture

- **In-memory market data** – `InMemoryMarketDataSource` serves deterministic historical candles so that system tests can simulate complex market behaviour without hitting live endpoints.
- **Synthetic market regimes** – tests generate bullish, bearish and oscillating price series to validate that the engine reacts by opening and closing positions appropriately.
- **Portfolio assertions** – system tests assert that the resulting report contains actionable signals, realized profit calculations and capital preservation metrics.
- **Extensibility** – additional scenarios (e.g. latency spikes, multiple concurrent positions) can be encoded by adding new test fixtures that target the same abstractions.

## Execution Guidance

1. Restore and build the solution: `dotnet build`.
2. Execute the system test suite: `dotnet test TradingSystem.SystemTests/TradingSystem.SystemTests.csproj`.
3. To run against live data, create the engine via `TradingSystemFactory.CreateDefault()` and call `RunAsync()`.

The codex is intentionally high-level so that it can be version controlled alongside the source code and evolve with the trading strategy.
