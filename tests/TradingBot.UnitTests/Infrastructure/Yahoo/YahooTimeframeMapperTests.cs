using TradingBot.Domain.Enums;
using TradingBot.Infrastructure.Providers.Yahoo;

namespace TradingBot.UnitTests.Infrastructure.Yahoo;

public sealed class YahooTimeframeMapperTests
{
    [Theory]
    [InlineData(Timeframe.OneMinute, "1m")]
    [InlineData(Timeframe.FiveMinutes, "5m")]
    [InlineData(Timeframe.FifteenMinutes, "15m")]
    [InlineData(Timeframe.OneHour, "1h")]
    [InlineData(Timeframe.FourHours, "1h")]
    [InlineData(Timeframe.OneDay, "1d")]
    [InlineData(Timeframe.OneWeek, "1wk")]
    [InlineData(Timeframe.OneMonth, "1mo")]
    public void Map_ReturnsCorrectYahooInterval(
        Timeframe timeframe,
        string expected)
    {
        var result =
            YahooTimeframeMapper.Map(timeframe);

        Assert.Equal(expected, result);
    }
}
