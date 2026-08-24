using Microsoft.Extensions.Logging.Abstractions;
using TradingBot.Domain.Enums;
using TradingBot.Domain.ValueObjects;
using YahooQuotesApi;
using TradingBot.Infrastructure.Providers.Yahoo;
using TradingBot.Application.Errors;

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

    [Fact]
    public async Task GetCandlesAsync_YahooThrowsHttpException_ReturnsProviderUnavailable()
    {
        var client =
            new FakeYahooHistoryClient(
                YahooQuotesApi.Result<History>.Ok(
                    YahooTestData.CreateHistory(100)),
                new HttpRequestException("Test failure."));

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

        Assert.True(result.IsFailure);
        Assert.Equal(
            ApplicationErrors.ProviderUnavailable,
            result.Error);
    }

    [Fact]
    public async Task GetCandlesAsync_InsufficientHistory_ReturnsInsufficientHistoricalData()
    {
        var history =
            YahooTestData.CreateHistory(75);

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

        Assert.True(result.IsFailure);
        Assert.Equal(
            ApplicationErrors.InsufficientHistoricalData,
            result.Error);
    }

    [Fact]
    public async Task GetCandlesAsync_FourHourTimeframe_AggregatesHourlyCandles()
    {
        var history =
            YahooTestData.CreateHistory(400);

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
                Timeframe.FourHours,
                100,
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(100, result.Value.Count);
    }

    [Fact]
    public async Task GetCandlesAsync_FourHourTimeframe_PreservesCorrectOhlcValues()
    {
        var history =
            YahooTestData.CreateHistory(
                4,
                startingPrice: 100);

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
                Timeframe.FourHours,
                1,
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);

        var candle = result.Value[0];

        Assert.Equal(100, candle.Open);
        Assert.Equal(108, candle.High);
        Assert.Equal(95, candle.Low);
        Assert.Equal(105, candle.Close);
        Assert.Equal(4006, candle.Volume);
    }

    [Fact]
    public async Task GetCandlesAsync_CancellationRequested_PropagatesCancellation()
    {
        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

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

        await Assert.ThrowsAsync<OperationCanceledException>(
            () =>
                provider.GetCandlesAsync(
                    symbol,
                    Timeframe.OneHour,
                    100,
                    cancellationTokenSource.Token));
    }
}
