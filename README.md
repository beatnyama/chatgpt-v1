# chatgpt-v1
ChatGPT Connection Test

## Automated Forex Trading System

This repository now contains a modular C# trading engine capable of:

- Downloading historical forex candles (USD/AUD, EUR/USD, GBP/USD and USD/JPY) from public archives dating back to 1980 via `HttpMarketDataSource`.
- Analysing price action using configurable momentum windows to generate buy, sell and close signals.
- Simulating a portfolio starting at $100 with position sizing, stop-loss and take-profit management tuned to reach a $2,500 profit target within 48 hours of trading time.
- Producing detailed execution reports that summarise realised profit and whether the profit objective has been achieved.

Refer to `docs/SystemTestsCodex.md` for an overview of the system test harness and CI integration strategy.
