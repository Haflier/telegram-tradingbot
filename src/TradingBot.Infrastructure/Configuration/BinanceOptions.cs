namespace TradingBot.Infrastructure.Configuration;

public sealed class BinanceOptions
{
    public const string SectionName = "Binance";

    public string BaseUrl { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 10;
}
