namespace TradingBot.Infrastructure.Configuration;

public sealed class YahooOptions
{
    public const string SectionName = "Yahoo";

    public string UserAgent { get; init; } =
        "TradingBot/1.0";

    public int HistoryCacheDurationMinutes { get; init; } = 5;
}
