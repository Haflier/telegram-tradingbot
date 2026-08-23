using Microsoft.Extensions.Logging;
using TradingBot.Application.Abstractions;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;
using TradingBot.Domain.Entities;
using TradingBot.Domain.Enums;
using TradingBot.Domain.ValueObjects;

namespace TradingBot.Infrastructure.Providers.Yahoo;

public sealed class YahooPriceDataProvider(
    IYahooHistoryClient yahooHistoryClient,
    ILogger<YahooPriceDataProvider> logger)
    : IPriceDataProvider
{
    public bool CanHandle(TradingSymbol symbol) =>
        symbol.AssetType == AssetType.Stock ||
        symbol.AssetType == AssetType.Index;

    public async Task<Result<IReadOnlyList<Candle>>> GetCandlesAsync(
        TradingSymbol symbol,
        Timeframe timeframe,
        int count,
        CancellationToken cancellationToken)
    {
        try
        {
            var interval =
                YahooTimeframeMapper.Map(timeframe);

            // 4h is not a native Yahoo interval.
            // Fetch enough hourly candles to construct the
            // requested number of four-hour candles.
            var requestCount =
                timeframe == Timeframe.FourHours
                    ? count * 4
                    : count;

            logger.LogInformation(
                "Requesting Yahoo Finance history for {Symbol} at {Timeframe} with {Count} source candles",
                symbol.Value,
                interval,
                requestCount);

            var result =
                await yahooHistoryClient.GetHistoryAsync(
                    symbol.Value,
                    interval,
                    cancellationToken);

            if (result.HasError)
            {
                logger.LogWarning(
                    "Yahoo Finance returned an error for {Symbol}: {Error}",
                    symbol.Value,
                    result.Error);

                return Result<IReadOnlyList<Candle>>.Failure(
                    ApplicationErrors.InvalidSymbol);
            }

            if (!result.HasValue)
            {
                logger.LogWarning(
                    "Yahoo Finance returned no history for {Symbol}",
                    symbol.Value);

                return Result<IReadOnlyList<Candle>>.Failure(
                    ApplicationErrors.InsufficientHistoricalData);
            }

            var candles =
                result.Value.Ticks
                    .OrderBy(tick => tick.Date)
                    .Select(tick =>
                        new Candle(
                            new DateTimeOffset(
                                tick.Date
                                    .ToDateTimeUtc()),
                            Convert.ToDecimal(tick.Open),
                            Convert.ToDecimal(tick.High),
                            Convert.ToDecimal(tick.Low),
                            Convert.ToDecimal(tick.Close),
                            Convert.ToDecimal(tick.Volume)))
                    .ToList();

            if (timeframe == Timeframe.FourHours)
            {
                candles =
                    YahooCandleAggregator
                        .AggregateFourHours(candles)
                        .ToList();
            }

            if (candles.Count < count)
            {
                logger.LogWarning(
                    "Yahoo Finance returned insufficient data for {Symbol}. Requested {Requested}, received {Received}",
                    symbol.Value,
                    count,
                    candles.Count);

                return Result<IReadOnlyList<Candle>>.Failure(
                    ApplicationErrors.InsufficientHistoricalData);
            }

            // Yahoo may return more candles than requested.
            // Return exactly the requested number, keeping the newest.
            var selected =
                candles
                    .TakeLast(count)
                    .ToList();

            return Result<IReadOnlyList<Candle>>.Success(
                selected);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                "Yahoo Finance request timed out for {Symbol}",
                symbol.Value);

            return Result<IReadOnlyList<Candle>>.Failure(
                ApplicationErrors.ProviderTimeout);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                exception,
                "HTTP error while contacting Yahoo Finance for {Symbol}",
                symbol.Value);

            return Result<IReadOnlyList<Candle>>.Failure(
                ApplicationErrors.ProviderUnavailable);
        }
    }
}
