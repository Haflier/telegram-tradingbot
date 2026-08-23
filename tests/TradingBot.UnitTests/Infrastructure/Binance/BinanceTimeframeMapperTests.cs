using TradingBot.Domain.Enums;
using TradingBot.Infrastructure.Providers.Binance;

namespace TradingBot.UnitTests.Infrastructure.Binance;

public sealed class BinanceTimeframeMapperTests
{
    [Theory]
    [InlineData(Timeframe.OneMinute, "1m")]
    [InlineData(Timeframe.FiveMinutes, "5m")]
    [InlineData(Timeframe.FifteenMinutes, "15m")]
    [InlineData(Timeframe.OneHour, "1h")]
    [InlineData(Timeframe.FourHours, "4h")]
    [InlineData(Timeframe.OneDay, "1d")]
    [InlineData(Timeframe.OneWeek, "1w")]
    [InlineData(Timeframe.OneMonth, "1mo")]
    public void Map_ReturnsCorrectBinanceInterval(
        Timeframe timeframe,
        string expected)
    {
        var result =
            BinanceTimeframeMapper.Map(timeframe);

        Assert.Equal(
            expected,
            result);
    }
}
