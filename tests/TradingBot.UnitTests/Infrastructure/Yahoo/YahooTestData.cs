using System.Collections.Immutable;
using NodaTime;
using YahooQuotesApi;

namespace TradingBot.UnitTests.Infrastructure.Yahoo;

internal static class YahooTestData
{
    public static History CreateHistory(
        int count,
        double startingPrice = 100)
    {
        var history = new History();

        var start =
            Instant.FromDateTimeUtc(
                DateTime.UtcNow.AddHours(-count));

        var ticks =
            Enumerable
                .Range(0, count)
                .Select(index =>
                {
                    var price = startingPrice + index;

                    return new Tick(
                        start.Plus(
                            Duration.FromHours(index)),
                        price,
                        price + 5,
                        price - 5,
                        price + 2,
                        price + 2,
                        1000 + index);
                });

        var immutableTicks =
            ImmutableArray.CreateRange(ticks);

        var property =
            typeof(History).GetProperty(
                nameof(History.Ticks))!;

        property.SetValue(
            history,
            immutableTicks);

        return history;
    }
}
