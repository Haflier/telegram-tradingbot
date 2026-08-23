using TradingBot.Domain.Entities;
using TradingBot.Infrastructure.Providers.Yahoo;

namespace TradingBot.UnitTests.Infrastructure.Yahoo;

public sealed class YahooCandleAggregatorTests
{
    [Fact]
    public void AggregateFourHours_CombinesFourCandles()
    {
        var candles = new[]
        {
            Create(100, 105, 95, 102, 100),
            Create(102, 110, 100, 108, 200),
            Create(108, 112, 103, 105, 300),
            Create(105, 115, 101, 110, 400)
        };

        var result =
            YahooCandleAggregator
                .AggregateFourHours(candles);

        Assert.Single(result);

        var candle = result[0];

        Assert.Equal(100m, candle.Open);
        Assert.Equal(115m, candle.High);
        Assert.Equal(95m, candle.Low);
        Assert.Equal(110m, candle.Close);
        Assert.Equal(1000m, candle.Volume);
    }

    [Fact]
    public void AggregateFourHours_IgnoresIncompleteGroup()
    {
        var candles = new[]
        {
            Create(100, 105, 95, 102, 100),
            Create(102, 110, 100, 108, 200),
            Create(108, 112, 103, 105, 300),
            Create(105, 115, 101, 110, 400),
            Create(110, 120, 108, 115, 500)
        };

        var result =
            YahooCandleAggregator
                .AggregateFourHours(candles);

        Assert.Single(result);
    }

    [Fact]
    public void AggregateFourHours_EmptyInput_ReturnsEmpty()
    {
        var result =
            YahooCandleAggregator
                .AggregateFourHours([]);

        Assert.Empty(result);
    }

    private static Candle Create(
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        decimal volume)
    {
        return new Candle(
            DateTimeOffset.UtcNow,
            open,
            high,
            low,
            close,
            volume);
    }
}
