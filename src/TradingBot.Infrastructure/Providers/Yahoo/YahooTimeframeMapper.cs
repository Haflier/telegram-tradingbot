using TradingBot.Domain.Enums;

namespace TradingBot.Infrastructure.Providers.Yahoo;

public static class YahooTimeframeMapper
{
    public static string Map(Timeframe timeframe) =>
        timeframe switch
        {
            Timeframe.OneMinute => "1m",
            Timeframe.FiveMinutes => "5m",
            Timeframe.FifteenMinutes => "15m",
            Timeframe.OneHour => "1h",
            Timeframe.FourHours => "1h",
            Timeframe.OneDay => "1d",
            Timeframe.OneWeek => "1wk",
            Timeframe.OneMonth => "1mo",

            _ => throw new ArgumentOutOfRangeException(
                nameof(timeframe),
                timeframe,
                "Unsupported Yahoo Finance timeframe.")
        };
}
