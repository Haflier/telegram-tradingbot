using ScottPlot;
using TradingBot.Application.Abstractions;
using TradingBot.Application.DTOs;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;
using TradingBot.Infrastructure.Configuration;

namespace TradingBot.Infrastructure.Charts;

public sealed class ScottPlotChartGenerator(
    IChartConfiguration configuration,
    IChartDimensions chartDimensions)
    : IChartGenerator
{
    public Result<GeneratedChart> Generate(
        ChartData data)
    {
        if (data.Candles.Count == 0)
        {
            return Result<GeneratedChart>.Failure(
                ApplicationErrors.InsufficientHistoricalData);
        }

        if (data.MovingAverage.Count != data.Candles.Count)
        {
            return Result<GeneratedChart>.Failure(
                ApplicationErrors.InsufficientHistoricalData);
        }

        var ohlcs =
            data.Candles
                .Select(candle =>
                    new OHLC(
                        (double)candle.Open,
                        (double)candle.High,
                        (double)candle.Low,
                        (double)candle.Close,
                        candle.Timestamp.UtcDateTime,
                        TimeSpan.FromMinutes(1)))
                .ToArray();

        var plot = new Plot();

        plot.Add.Candlestick(ohlcs);

        var movingAverage =
            data.MovingAverage
                .Select(
                    (value, index) =>
                        value.HasValue
                            ? new Coordinates(
                                index,
                                (double)value.Value)
                            : (Coordinates?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToArray();

        if (movingAverage.Length > 0)
        {
            plot.Add.Scatter(movingAverage);
        }

        plot.Title(
            $"{data.Symbol.Value} - Market Chart");

        plot.XLabel("Time");
        plot.YLabel("Price");

        using var memoryStream =
            new MemoryStream();

        var temporaryPath =
            Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid():N}.png");

        try
        {
            plot.SavePng(
                temporaryPath,
                chartDimensions.Width,
                chartDimensions.Height);

            var bytes =
                File.ReadAllBytes(temporaryPath);

            memoryStream.Write(bytes);

            memoryStream.Position = 0;

            return Result<GeneratedChart>.Success(
                new GeneratedChart(
                    memoryStream,
                    $"{data.Symbol.Value}.png",
                    "image/png"));
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
