namespace TradingBot.Infrastructure.Configuration;

public sealed class TwelveDataOptions
{
    public const string SectionName = "TwelveData";

    public string ApiKey { get; init; } = string.Empty;

    public string BaseUrl { get; init; } =
        "https://api.twelvedata.com";

    public int TimeoutSeconds { get; init; } = 10;
}
