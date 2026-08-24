using System.Globalization;
using System.Text.Json;

namespace TradingBot.UnitTests.Infrastructure.TwelveData;

internal static class TwelveDataTestData
{
    public static string CreateResponse(
        int count,
        decimal startingPrice = 100m)
    {
        var values = new List<object>();

        for (var i = 0; i < count; i++)
        {
            var open = startingPrice + i;
            var close = open + 5m;
            var high = open + 8m;
            var low = open - 5m;
            var volume = 4000m + i;

            values.Add(
                new
                {
                    datetime =
                        DateTime.UtcNow
                            .AddDays(-count + i)
                            .ToString(
                                "yyyy-MM-dd HH:mm:ss",
                                CultureInfo.InvariantCulture),

                    open = open.ToString(
                        CultureInfo.InvariantCulture),

                    high = high.ToString(
                        CultureInfo.InvariantCulture),

                    low = low.ToString(
                        CultureInfo.InvariantCulture),

                    close = close.ToString(
                        CultureInfo.InvariantCulture),

                    volume = volume.ToString(
                        CultureInfo.InvariantCulture)
                });
        }

        return JsonSerializer.Serialize(
            new
            {
                status = "ok",
                values
            });
    }
}
