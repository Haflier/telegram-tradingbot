using TradingBot.Domain.Errors;

namespace TradingBot.Application.Errors;

public static class ApplicationErrors
{
    public static Error InvalidCommand =>
        new(
            "InvalidCommand",
            "The command format is invalid.\nUsage: /chart BTC 1d");

    public static Error InvalidTimeframe =>
        new(
            "InvalidTimeframe",
            "The requested timeframe is not supported.\nSupported timeframes: 1m 5m 15m 1h 4h 1d 1w");

    public static Error InvalidSymbol =>
        new(
            "InvalidSymbol",
            "The requested symbol is invalid.");

    public static Error UnsupportedSymbol =>
        new(
            "UnsupportedSymbol",
            "The requested symbol is not supported.");

    public static Error InsufficientHistoricalData =>
        new(
            "InsufficientHistoricalData",
            "There is not enough historical market data.");

    public static Error ProviderUnavailable =>
        new(
            "ProviderUnavailable",
            "The market data provider is temporarily unavailable.");

    public static Error ProviderTimeout =>
        new(
            "ProviderTimeout",
            "The market data provider did not respond in time.");

    public static Error ProviderRateLimited =>
        new(
            "ProviderRateLimited",
            "The market data provider is rate limiting requests.");

    public static Error ChartGenerationFailed =>
        new(
            "ChartGenerationFailed",
            "The chart could not be generated.");

    public static Error TelegramDeliveryFailed =>
        new(
            "TelegramDeliveryFailed",
            "The chart could not be sent to Telegram.");
}
