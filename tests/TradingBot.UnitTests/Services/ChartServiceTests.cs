using TradingBot.Application.Abstractions;
using TradingBot.Application.DTOs;
using TradingBot.Application.Results;
using TradingBot.Application.Services;
using TradingBot.Domain.Entities;
using TradingBot.Domain.Enums;
using TradingBot.Domain.Errors;
using TradingBot.Domain.ValueObjects;

namespace TradingBot.UnitTests.Services;

public sealed class ChartServiceTests
{
    [Fact]
    public async Task GenerateChartAsync_WithValidRequest_GeneratesChart()
    {
        var candles = CreateCandles(100);

        var provider =
            new FakeProvider(candles);

        var symbolResolver =
            new FakeSymbolResolver();

        var providerResolver =
            new FakeProviderResolver(provider);

        var smaCalculator =
            new SmaCalculator();

        var chartGenerator =
            new FakeChartGenerator();

        var chartConfiguration =
            new FakeChartConfiguration();

        var service =
            new ChartService(
                symbolResolver,
                providerResolver,
                smaCalculator,
                chartGenerator,
                chartConfiguration);

        var request =
            new ChartRequest(
                "BTC",
                Timeframe.FourHours);

        var result =
            await service.GenerateChartAsync(
                request,
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.True(
            chartGenerator.WasCalled);

        Assert.Equal(
            100,
            chartGenerator.ReceivedData!.Candles.Count);

        Assert.Equal(
            100,
            chartGenerator.ReceivedData.MovingAverage.Count);
    }

    [Fact]
    public async Task GenerateChartAsync_WhenProviderFails_PropagatesError()
    {
        var provider =
            new FailingProvider();

        var service =
            new ChartService(
                new FakeSymbolResolver(),
                new FakeProviderResolver(provider),
                new SmaCalculator(),
                new FakeChartGenerator(),
                new FakeChartConfiguration());

        var result =
            await service.GenerateChartAsync(
                new ChartRequest(
                    "BTC",
                    Timeframe.FourHours),
                CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "ProviderUnavailable",
            result.Error.Code);
    }

    [Fact]
    public async Task GenerateChartAsync_WhenProviderReturnsWrongCount_Fails()
    {
        var provider =
            new FakeProvider(
                CreateCandles(99));

        var service =
            new ChartService(
                new FakeSymbolResolver(),
                new FakeProviderResolver(provider),
                new SmaCalculator(),
                new FakeChartGenerator(),
                new FakeChartConfiguration());

        var result =
            await service.GenerateChartAsync(
                new ChartRequest(
                    "BTC",
                    Timeframe.FourHours),
                CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "InsufficientHistoricalData",
            result.Error.Code);
    }

    private static IReadOnlyList<Candle> CreateCandles(
        int count)
    {
        var candles = new List<Candle>(count);

        for (var i = 0; i < count; i++)
        {
            var open = 100m + i;
            var close = open + 1m;

            candles.Add(
                new Candle(
                    DateTimeOffset.UtcNow.AddHours(i),
                    open,
                    close + 1m,
                    open - 1m,
                    close,
                    1000m));
        }

        return candles;
    }

    private sealed class FakeSymbolResolver
        : ISymbolResolver
    {
        public Result<TradingSymbol> Resolve(
            string rawSymbol)
        {
            return Result<TradingSymbol>.Success(
                TradingSymbol.Create(
                    "BTCUSDT",
                    AssetType.Crypto));
        }
    }

    private sealed class FakeProviderResolver(
        IPriceDataProvider provider)
        : IPriceDataProviderResolver
    {
        public Result<IPriceDataProvider> Resolve(
            TradingSymbol symbol)
        {
            return Result<IPriceDataProvider>.Success(
                provider);
        }
    }

    private class FakeProvider(
        IReadOnlyList<Candle> candles)
        : IPriceDataProvider
    {
        public bool CanHandle(
            TradingSymbol symbol) =>
            true;

        public Task<Result<IReadOnlyList<Candle>>> GetCandlesAsync(
            TradingSymbol symbol,
            Timeframe timeframe,
            int count,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Result<IReadOnlyList<Candle>>.Success(
                    candles));
        }
    }

    private sealed class FailingProvider
        : IPriceDataProvider
    {
        public bool CanHandle(
            TradingSymbol symbol) =>
            true;

        public Task<Result<IReadOnlyList<Candle>>> GetCandlesAsync(
            TradingSymbol symbol,
            Timeframe timeframe,
            int count,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Result<IReadOnlyList<Candle>>.Failure(
                    new Error(
                        "ProviderUnavailable",
                        "Provider unavailable.")));
        }
    }

    private sealed class FakeChartGenerator
        : IChartGenerator
    {
        public bool WasCalled { get; private set; }

        public ChartData? ReceivedData { get; private set; }

        public Result<GeneratedChart> Generate(
            ChartData data)
        {
            WasCalled = true;
            ReceivedData = data;

            return Result<GeneratedChart>.Success(
                new GeneratedChart(
                    new MemoryStream(),
                    "BTCUSDT.png",
                    "image/png"));
        }
    }

    private sealed class FakeChartConfiguration
          : IChartConfiguration
    {
        public int CandleCount => 100;

        public int MovingAveragePeriod => 20;
    }
}
