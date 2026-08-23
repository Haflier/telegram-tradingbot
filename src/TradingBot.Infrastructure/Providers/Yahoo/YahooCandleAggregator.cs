using TradingBot.Domain.Entities;

namespace TradingBot.Infrastructure.Providers.Yahoo;

public static class YahooCandleAggregator
{
    public static IReadOnlyList<Candle> AggregateFourHours(
        IReadOnlyList<Candle> hourlyCandles)
    {
        if (hourlyCandles.Count == 0)
            return [];

        var result = new List<Candle>();

        for (var i = 0; i + 3 < hourlyCandles.Count; i += 4)
        {
            var first = hourlyCandles[i];
            var fourth = hourlyCandles[i + 3];

            var high =
                hourlyCandles
                    .Skip(i)
                    .Take(4)
                    .Max(x => x.High);

            var low =
                hourlyCandles
                    .Skip(i)
                    .Take(4)
                    .Min(x => x.Low);

            var volume =
                hourlyCandles
                    .Skip(i)
                    .Take(4)
                    .Sum(x => x.Volume);

            result.Add(
                new Candle(
                    first.Timestamp,
                    first.Open,
                    high,
                    low,
                    fourth.Close,
                    volume));
        }

        return result;
    }
}
