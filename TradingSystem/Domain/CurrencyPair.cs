namespace TradingSystem.Domain;

/// <summary>
/// Represents a tradable currency pair (e.g. USD/AUD).
/// </summary>
public readonly record struct CurrencyPair(string BaseCurrency, string QuoteCurrency)
{
    public string Symbol => $"{BaseCurrency}/{QuoteCurrency}";

    public override string ToString() => Symbol;
}
