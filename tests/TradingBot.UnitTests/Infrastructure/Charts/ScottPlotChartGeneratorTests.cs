using TradingBot.Application.Abstractions;
using TradingBot.Application.DTOs;
using TradingBot.Domain.Entities;
using TradingBot.Domain.Enums;
using TradingBot.Domain.ValueObjects;
using TradingBot.Infrastructure.Charts;
using TradingBot.Infrastructure.Configuration;

namespace TradingBot.UnitTests.Infrastructure.Charts;

public sealed class ScottPlotChartGeneratorTests
{
    [Fact]
    public void Generate_ValidChartData_ReturnsPng()
    {
        var configuration =
            new FakeChartConfiguration(
                width: 800,
                height: 600);

        var generator =
            new ScottPlotChartGenerator(
                configuration,
                configuration);

        var candles =
            CreateCandles(100);

        var movingAverage =
            Enumerable
                .Repeat<decimal?>(100m, 100)
                .ToList();

        var data =
            new ChartData(
                TradingSymbol.Create(
                    "AAPL",
                    AssetType.Stock),
                candles,
                movingAverage);

        var result =
            generator.Generate(data);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("AAPL.png", result.Value.FileName);
        Assert.Equal("image/png", result.Value.ContentType);
        Assert.True(result.Value.Content.Length > 0);

        result.Value.Content.Dispose();
    }

    [Fact]
    public void Generate_EmptyCandles_ReturnsInsufficientHistoricalData()
    {
        var configuration =
            new FakeChartConfiguration();

        var generator =
            new ScottPlotChartGenerator(
                configuration,
                configuration);

        var data =
            new ChartData(
                TradingSymbol.Create(
                    "AAPL",
                    AssetType.Stock),
                Array.Empty<Candle>(),
                Array.Empty<decimal?>());

        var result =
            generator.Generate(data);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "InsufficientHistoricalData",
            result.Error.Code);
    }

    [Fact]
    public void Generate_MovingAverageCountMismatch_ReturnsFailure()
    {
        var configuration =
            new FakeChartConfiguration();

        var generator =
            new ScottPlotChartGenerator(
                configuration,
                configuration);

        var candles =
            CreateCandles(10);

        var movingAverage =
            Enumerable
                .Repeat<decimal?>(100m, 5)
                .ToList();

        var data =
            new ChartData(
                TradingSymbol.Create(
                    "AAPL",
                    AssetType.Stock),
                candles,
                movingAverage);

        var result =
            generator.Generate(data);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "InsufficientHistoricalData",
            result.Error.Code);
    }

    private static IReadOnlyList<Candle> CreateCandles(
        int count)
    {
        var candles =
            new List<Candle>(count);

        for (var i = 0; i < count; i++)
        {
            candles.Add(
                new Candle(
                    DateTimeOffset.UtcNow.AddMinutes(i),
                    100m,
                    105m,
                    95m,
                    102m,
                    1000m));
        }

        return candles;
    }

    private sealed class FakeChartConfiguration(
        int width = 1600,
        int height = 900)
        : IChartConfiguration, IChartDimensions
    {
        public int CandleCount => 100;

        public int MovingAveragePeriod => 20;

        public int Width => width;

        public int Height => height;
    }
}
