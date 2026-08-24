using Microsoft.Extensions.Logging.Abstractions;
using TradingBot.Application.Errors;
using TradingBot.Domain.Enums;
using TradingBot.Domain.ValueObjects;
using TradingBot.Infrastructure.Providers.TwelveData;

namespace TradingBot.UnitTests.Infrastructure.TwelveData;

public sealed class TwelveDataPriceDataProviderTests
{
    [Fact]
    public async Task GetCandlesAsync_ValidResponse_ReturnsCandles()
    {
        var json =
            TwelveDataTestData.CreateResponse(100);

        var client =
            new FakeTwelveDataClient(json);

        var provider =
            new TwelveDataPriceDataProvider(
                client,
                NullLogger<TwelveDataPriceDataProvider>.Instance);

        var symbol =
            TradingSymbol.Create(
                "AAPL",
                AssetType.Stock);

        var result =
            await provider.GetCandlesAsync(
                symbol,
                Timeframe.OneDay,
                100,
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(100, result.Value.Count);
    }

    [Fact]
    public async Task GetCandlesAsync_InsufficientResponse_ReturnsInsufficientHistoricalData()
    {
        var json =
            TwelveDataTestData.CreateResponse(75);

        var client =
            new FakeTwelveDataClient(json);

        var provider =
            new TwelveDataPriceDataProvider(
                client,
                NullLogger<TwelveDataPriceDataProvider>.Instance);

        var symbol =
            TradingSymbol.Create(
                "AAPL",
                AssetType.Stock);

        var result =
            await provider.GetCandlesAsync(
                symbol,
                Timeframe.OneDay,
                100,
                CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            ApplicationErrors.InsufficientHistoricalData,
            result.Error);
    }

    [Fact]
    public async Task GetCandlesAsync_ClientThrowsHttpException_ReturnsProviderUnavailable()
    {
        var client =
            new FakeTwelveDataClient(
                new HttpRequestException("Test failure."));

        var provider =
            new TwelveDataPriceDataProvider(
                client,
                NullLogger<TwelveDataPriceDataProvider>.Instance);

        var symbol =
            TradingSymbol.Create(
                "AAPL",
                AssetType.Stock);

        var result =
            await provider.GetCandlesAsync(
                symbol,
                Timeframe.OneDay,
                100,
                CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            ApplicationErrors.ProviderUnavailable,
            result.Error);
    }

    [Fact]
    public async Task GetCandlesAsync_InvalidJson_ReturnsProviderUnavailable()
    {
        var client =
            new FakeTwelveDataClient(
                "{ invalid json }");

        var provider =
            new TwelveDataPriceDataProvider(
                client,
                NullLogger<TwelveDataPriceDataProvider>.Instance);

        var symbol =
            TradingSymbol.Create(
                "AAPL",
                AssetType.Stock);

        var result =
            await provider.GetCandlesAsync(
                symbol,
                Timeframe.OneDay,
                100,
                CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            ApplicationErrors.ProviderUnavailable,
            result.Error);
    }

    [Fact]
    public async Task GetCandlesAsync_ResponseWithoutValues_ReturnsInsufficientHistoricalData()
    {
        var client =
            new FakeTwelveDataClient(
                """
                {
                    "status": "error",
                    "code": 400,
                    "message": "Invalid symbol"
                }
                """);

        var provider =
            new TwelveDataPriceDataProvider(
                client,
                NullLogger<TwelveDataPriceDataProvider>.Instance);

        var symbol =
            TradingSymbol.Create(
                "AAPL",
                AssetType.Stock);

        var result =
            await provider.GetCandlesAsync(
                symbol,
                Timeframe.OneDay,
                100,
                CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            ApplicationErrors.InsufficientHistoricalData,
            result.Error);
    }

    [Fact]
    public async Task GetCandlesAsync_FourHourTimeframe_RequestsFourHourInterval()
    {
        var json =
            TwelveDataTestData.CreateResponse(100);

        var client =
            new FakeTwelveDataClient(json);

        var provider =
            new TwelveDataPriceDataProvider(
                client,
                NullLogger<TwelveDataPriceDataProvider>.Instance);

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

        Assert.Equal(
            "4h",
            client.ReceivedInterval);

        Assert.Equal(
            100,
            client.ReceivedOutputSize);
    }

    [Fact]
    public async Task GetCandlesAsync_PreservesCorrectOhlcValues()
    {
        var json =
            TwelveDataTestData.CreateResponse(
                1,
                startingPrice: 100);

        var client =
            new FakeTwelveDataClient(json);

        var provider =
            new TwelveDataPriceDataProvider(
                client,
                NullLogger<TwelveDataPriceDataProvider>.Instance);

        var symbol =
            TradingSymbol.Create(
                "AAPL",
                AssetType.Stock);

        var result =
            await provider.GetCandlesAsync(
                symbol,
                Timeframe.OneDay,
                1,
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);

        var candle =
            result.Value[0];

        Assert.Equal(100m, candle.Open);
        Assert.Equal(108m, candle.High);
        Assert.Equal(95m, candle.Low);
        Assert.Equal(105m, candle.Close);
        Assert.Equal(4000m, candle.Volume);
    }

    [Fact]
    public async Task GetCandlesAsync_CancellationRequested_PropagatesCancellation()
    {
        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        var client =
            new FakeTwelveDataClient(
                TwelveDataTestData.CreateResponse(100));

        var provider =
            new TwelveDataPriceDataProvider(
                client,
                NullLogger<TwelveDataPriceDataProvider>.Instance);

        var symbol =
            TradingSymbol.Create(
                "AAPL",
                AssetType.Stock);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () =>
                provider.GetCandlesAsync(
                    symbol,
                    Timeframe.OneDay,
                    100,
                    cancellationTokenSource.Token));
    }

    private sealed class FakeTwelveDataClient
        : ITwelveDataClient
    {
        private readonly string? _response;
        private readonly Exception? _exception;

        public string? ReceivedInterval { get; private set; }

        public int ReceivedOutputSize { get; private set; }

        public FakeTwelveDataClient(
            string response)
        {
            _response = response;
        }

        public FakeTwelveDataClient(
            Exception exception)
        {
            _exception = exception;
        }

        public async Task<string> GetTimeSeriesAsync(
            string symbol,
            string interval,
            int outputSize,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ReceivedInterval = interval;
            ReceivedOutputSize = outputSize;

            if (_exception is not null)
                throw _exception;

            return await Task.FromResult(
                _response!);
        }
    }
}
