namespace TradingBot.Infrastructure.Configuration;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    public string BotToken { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 10;
}
