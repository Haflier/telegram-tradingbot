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

        /*
         * Determine the candle width from the actual spacing
         * between candles.
         *
         * The previous implementation used a fixed 1-minute width,
         * which made 4h candles appear extremely thin.
         */
        var candleWidth =
            GetCandleWidth(data);

        var ohlcs =
            data.Candles
                .Select(
                    candle =>
                        new OHLC(
                            (double)candle.Open,
                            (double)candle.High,
                            (double)candle.Low,
                            (double)candle.Close,
                            candle.Timestamp.UtcDateTime,
                            candleWidth))
                .ToArray();

        var plot = new Plot();

        plot.FigureBackground.Color = ScottPlot.Color.FromHex("#ff4200");
        plot.DataBackground.Color = ScottPlot.Color.FromHex("#c0c08a");

        plot.Axes.Color(
            ScottPlot.Color.FromHex("#ffffff"));

        plot.Axes.Left.TickLabelStyle.ForeColor =
            ScottPlot.Color.FromHex("#ffffff");
        plot.Axes.Bottom.TickLabelStyle.ForeColor =
            ScottPlot.Color.FromHex("#ffffff");
        plot.Axes.Left.TickLabelStyle.FontSize = 22;
        plot.Axes.Bottom.TickLabelStyle.FontSize = 22;

        plot.Grid.MajorLineColor =
            ScottPlot.Color.FromHex("#0747af");

        plot.Grid.MinorLineColor =
            ScottPlot.Color.FromHex("#0747af");



        plot.Add.Candlestick(ohlcs);

        /*
         * SMA
         */
        var movingAverage =
            data.Candles
                .Zip(
                    data.MovingAverage,
                    (candle, value) =>
                        value.HasValue
                            ? new Coordinates(
                                candle.Timestamp.UtcDateTime.ToOADate(),
                                (double)value.Value)
                            : (Coordinates?)null)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToArray();

        if (movingAverage.Length > 0)
        {
            var sma =
                plot.Add.Scatter(movingAverage);

            sma.LegendText =
                $"SMA {configuration.MovingAveragePeriod}";

            sma.MarkerSize = 0;
        }

        /*
         * The latest candle close represents the latest
         * available market price.
         */
        var currentPrice =
            data.Candles[^1].Close;

        /*
         * Draw a horizontal dotted line at the current price.
         */
        var currentPriceLine =
            plot.Add.HorizontalLine(
                (double)currentPrice);

        currentPriceLine.LinePattern =
            LinePattern.Dotted;

        currentPriceLine.LineWidth = 4;
        currentPriceLine.Color = Colors.Black;

        /*
         * Put the current price label at the right side
         * of the chart.
         */
        var lastTimestamp =
            data.Candles[^1]
                .Timestamp
                .UtcDateTime
                .ToOADate();

        var currentPriceLabel =
            plot.Add.Text(
                $"{currentPrice:N2}",
                lastTimestamp,
                (double)currentPrice);

        currentPriceLabel.Alignment =
            Alignment.LowerRight;

        currentPriceLabel.OffsetX = 0;

        currentPriceLabel.LabelStyle.FontSize = 21;
        currentPriceLabel.LabelStyle.Bold = true;
        currentPriceLabel.LabelStyle.ForeColor = Colors.Black;

        /*
         * Display calendar dates on the X axis.
         */
        plot.Axes.DateTimeTicksBottom();

        plot.Axes.Bottom.TickLabelStyle.ForeColor =
            ScottPlot.Color.FromHex("#ffffff");

        plot.Axes.Bottom.TickLabelStyle.FontSize = 18;

        //Title
        plot.Title(
            $"{data.Symbol.Value}");
        plot.Axes.Title.Label.FontSize = 30;

        //plot.XLabel("Time");
        //plot.YLabel("Price");

        plot.ShowLegend();

        var memoryStream =
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

    private static TimeSpan GetCandleWidth(
        ChartData data)
    {
        if (data.Candles.Count < 2)
        {
            return TimeSpan.FromHours(1);
        }

        /*
         * Use the median timestamp difference rather than
         * simply the first difference. This makes the width
         * more robust if the data contains an irregular gap.
         */
        var intervals =
            data.Candles
                .Zip(
                    data.Candles.Skip(1),
                    (current, next) =>
                        next.Timestamp - current.Timestamp)
                .Where(
                    interval =>
                        interval > TimeSpan.Zero)
                .OrderBy(
                    interval => interval)
                .ToArray();

        if (intervals.Length == 0)
        {
            return TimeSpan.FromHours(1);
        }

        var medianInterval =
            intervals[intervals.Length / 2];

        /*
         * Make candles approximately 75% as wide as the
         * distance between candle centers.
         *
         * This produces clearly separated but substantial
         * candlesticks.
         */
        var widthTicks =
            (long)(
                medianInterval.Ticks * 0.75);

        if (widthTicks <= 0)
        {
            widthTicks = TimeSpan.TicksPerMinute;
        }

        return TimeSpan.FromTicks(widthTicks);
    }
}
