using TradingBot.Application.Abstractions;
using TradingBot.Application.DTOs;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;
using TradingBot.Domain.Entities;
using TradingBot.Domain.Enums;

namespace TradingBot.Application.Services;

public sealed class ChartService(
    ISymbolResolver symbolResolver,
    IPriceDataProviderResolver providerResolver,
    ISmaCalculator smaCalculator,
    IChartGenerator chartGenerator,
    IChartConfiguration configuration)
    : IChartService
{
    public async Task<Result<GeneratedChart>> GenerateChartAsync(
        ChartRequest request,
        CancellationToken cancellationToken)
    {
        var symbolResult =
            symbolResolver.Resolve(request.RawSymbol);

        if (symbolResult.IsFailure)
        {
            return Result<GeneratedChart>.Failure(
                symbolResult.Error);
        }

        var symbol = symbolResult.Value!;

        var providerResult =
            providerResolver.Resolve(symbol);

        if (providerResult.IsFailure)
        {
            return Result<GeneratedChart>.Failure(
                providerResult.Error);
        }

        var provider = providerResult.Value!;

        var candlesResult =
            await provider.GetCandlesAsync(
                symbol,
                request.Timeframe,
                configuration.CandleCount,
                cancellationToken);

        if (candlesResult.IsFailure)
        {
            return Result<GeneratedChart>.Failure(
                candlesResult.Error);
        }

        var candles = candlesResult.Value!;

        if (candles.Count != configuration.CandleCount)
        {
            return Result<GeneratedChart>.Failure(
                ApplicationErrors.InsufficientHistoricalData);
        }

        var closingPrices =
            candles
                .Select(candle => candle.Close)
                .ToArray();

        var movingAverageResult =
            smaCalculator.Calculate(
                closingPrices,
                configuration.MovingAveragePeriod);

        if (movingAverageResult.IsFailure)
        {
            return Result<GeneratedChart>.Failure(
                movingAverageResult.Error);
        }

        var chartData =
            new ChartData(
                symbol,
                candles,
                movingAverageResult.Value!);

        var chartResult =
            chartGenerator.Generate(chartData);

        if (chartResult.IsFailure)
        {
            return Result<GeneratedChart>.Failure(
                chartResult.Error);
        }

        var chart = chartResult.Value!;

        var caption =
            BuildCaption(
                symbol.Value,
                request.Timeframe,
                candles);

        return Result<GeneratedChart>.Success(
            chart with
            {
                Caption = caption
            });
    }

    private static string BuildCaption(
        string symbol,
        Timeframe timeframe,
        IReadOnlyList<Candle> candles)
    {
        var latest =
            candles[^1];

        /*
         * Determine the beginning of the 24-hour window.
         *
         * We use the timestamp of the latest candle and look
         * backwards exactly 24 hours.
         */
        var windowStart =
            latest.Timestamp - TimeSpan.FromHours(24);

        var last24Hours =
            candles
                .Where(
                    candle =>
                        candle.Timestamp >= windowStart &&
                        candle.Timestamp <= latest.Timestamp)
                .ToList();

        /*
         * There may not be enough intraday candles to calculate
         * a true 24-hour range.
         *
         * In that case, fall back to the latest available candle
         * rather than producing misleading values.
         */
        var highest24h =
            last24Hours.Count > 0
                ? last24Hours.Max(candle => candle.High)
                : latest.High;

        var lowest24h =
            last24Hours.Count > 0
                ? last24Hours.Min(candle => candle.Low)
                : latest.Low;

        /*
         * Find the candle closest to the beginning of the
         * 24-hour window.
         *
         * This gives us the correct reference price for the
         * 24-hour percentage change.
         */
        var referenceCandle =
            candles
                .Where(
                    candle =>
                        candle.Timestamp <= latest.Timestamp &&
                        candle.Timestamp >=
                            windowStart - GetTimeframeTolerance(timeframe))
                .OrderBy(
                    candle =>
                        Math.Abs(
                            (candle.Timestamp - windowStart)
                                .Ticks))
                .FirstOrDefault();

        var referenceClose =
            referenceCandle?.Close ?? latest.Close;

        var change =
            referenceClose == 0
                ? 0m
                : ((latest.Close - referenceClose) /
                   referenceClose) * 100m;

        /*
         * This is the time when the bot generated the response,
         * not the timestamp of the market candle.
         */
        var generatedAt =
            DateTimeOffset.Now;

        return
            $"<b>${symbol}</b>\n" +
            $"<b>{latest.Close:N2} $</b>\n\n" +
            $"<b>24h Information:</b>\n" +
            $"• Highest: {highest24h:N2} $\n" +
            $"• Lowest: {lowest24h:N2} $\n" +
            $"• Change: {change:+0.00;-0.00;0.00}%\n\n" +
            $"Date: {generatedAt:HH:mm:ss yyyy/MM/dd}";
    }

    private static TimeSpan GetTimeframeTolerance(
        Timeframe timeframe) =>
        timeframe switch
        {
            Timeframe.OneMinute =>
                TimeSpan.FromMinutes(1),

            Timeframe.FiveMinutes =>
                TimeSpan.FromMinutes(5),

            Timeframe.FifteenMinutes =>
                TimeSpan.FromMinutes(15),

            Timeframe.OneHour =>
                TimeSpan.FromHours(1),

            Timeframe.FourHours =>
                TimeSpan.FromHours(4),

            Timeframe.OneDay =>
                TimeSpan.FromDays(1),

            Timeframe.OneWeek =>
                TimeSpan.FromDays(7),

            Timeframe.OneMonth =>
                TimeSpan.FromDays(31),

            _ =>
                TimeSpan.FromHours(1)
        };
}
