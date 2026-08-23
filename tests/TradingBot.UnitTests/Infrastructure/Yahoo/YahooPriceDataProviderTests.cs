using Microsoft.Extensions.Logging.Abstractions;
using TradingBot.Domain.Enums;
using TradingBot.Domain.ValueObjects;
using YahooQuotesApi;
using TradingBot.Infrastructure.Providers.Yahoo;

namespace TradingBot.UnitTests.Infrastructure.Yahoo;

public sealed class YahooPriceDataProviderTests
{
    [Fact]
    public async Task GetCandlesAsync_ValidHistory_ReturnsCandles()
    {
        var history =
            YahooTestData.CreateHistory(100);

        var client =
            new FakeYahooHistoryClient(
                YahooQuotesApi.Result<History>.Ok(history));

        var provider =
            new YahooPriceDataProvider(
                client,
                NullLogger<YahooPriceDataProvider>.Instance);

        var symbol =
            TradingSymbol.Create(
              "AAPL",
              AssetType.Stock);

        var result =
            await provider.GetCandlesAsync(
                symbol,
                Timeframe.OneHour,
                100,
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(100, result.Value.Count);
    }
}
