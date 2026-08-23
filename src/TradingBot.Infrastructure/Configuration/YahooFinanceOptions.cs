namespace TradingBot.Infrastructure.Configuration;

public sealed class YahooFinanceOptions
{
    public const string SectionName = "YahooFinance";

    public string BaseUrl { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 10;
}
