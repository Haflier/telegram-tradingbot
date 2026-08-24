using TradingBot.Domain.Enums;

namespace TradingBot.Infrastructure.Providers.Binance;

public static class BinanceTimeframeMapper
{
    public static string Map(Timeframe timeframe) =>
        timeframe switch
        {
            Timeframe.OneMinute => "1m",
            Timeframe.FiveMinutes => "5m",
            Timeframe.FifteenMinutes => "15m",
            Timeframe.OneHour => "1h",
            Timeframe.FourHours => "4h",
            Timeframe.OneDay => "1d",
            Timeframe.OneWeek => "1w",

            _ => throw new ArgumentOutOfRangeException(
                nameof(timeframe),
                timeframe,
                "Unsupported Binance timeframe.")
        };
}
