using System.Text.Json;
using Microsoft.Extensions.Logging;
using TradingBot.Application.Abstractions;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;
using TradingBot.Domain.Entities;
using TradingBot.Domain.Enums;
using TradingBot.Domain.ValueObjects;

namespace TradingBot.Infrastructure.Providers.TwelveData;

public sealed class TwelveDataPriceDataProvider(
    ITwelveDataClient client,
    ILogger<TwelveDataPriceDataProvider> logger)
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
            var interval = MapInterval(timeframe);

            logger.LogInformation(
                "Requesting Twelve Data history for {Symbol} at {Timeframe} with {Count} candles",
                symbol.Value,
                timeframe,
                count);

            var json =
                await client.GetTimeSeriesAsync(
                    symbol.Value,
                    interval,
                    count,
                    cancellationToken);

            using var document =
                JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty(
                    "values",
                    out var values))
            {
                logger.LogWarning(
                    "Twelve Data returned no values for {Symbol}",
                    symbol.Value);

                return Result<IReadOnlyList<Candle>>.Failure(
                    ApplicationErrors.InsufficientHistoricalData);
            }

            var candles = new List<Candle>();

            foreach (var value in values.EnumerateArray())
            {
                var date =
                    DateTimeOffset.Parse(
                        value.GetProperty("datetime").GetString()!);

                var open =
                    decimal.Parse(
                        value.GetProperty("open").GetString()!);

                var high =
                    decimal.Parse(
                        value.GetProperty("high").GetString()!);

                var low =
                    decimal.Parse(
                        value.GetProperty("low").GetString()!);

                var close =
                    decimal.Parse(
                        value.GetProperty("close").GetString()!);

                var volume =
                    value.TryGetProperty(
                        "volume",
                        out var volumeElement)
                        ? decimal.Parse(
                            volumeElement.GetString() ?? "0")
                        : 0m;

                candles.Add(
                    new Candle(
                        date,
                        open,
                        high,
                        low,
                        close,
                        volume));
            }

            candles =
                candles
                    .OrderBy(candle => candle.Timestamp)
                    .ToList();

            if (candles.Count < count)
            {
                logger.LogWarning(
                    "Twelve Data returned insufficient data for {Symbol}. Requested {Requested}, received {Received}",
                    symbol.Value,
                    count,
                    candles.Count);

                return Result<IReadOnlyList<Candle>>.Failure(
                    ApplicationErrors.InsufficientHistoricalData);
            }

            return Result<IReadOnlyList<Candle>>.Success(
                candles.TakeLast(count).ToList());
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                "Twelve Data request timed out for {Symbol}",
                symbol.Value);

            return Result<IReadOnlyList<Candle>>.Failure(
                ApplicationErrors.ProviderTimeout);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                exception,
                "HTTP error while contacting Twelve Data for {Symbol}",
                symbol.Value);

            return Result<IReadOnlyList<Candle>>.Failure(
                ApplicationErrors.ProviderUnavailable);
        }
        catch (JsonException exception)
        {
            logger.LogError(
                exception,
                "Invalid JSON returned by Twelve Data for {Symbol}",
                symbol.Value);

            return Result<IReadOnlyList<Candle>>.Failure(
                ApplicationErrors.ProviderUnavailable);
        }
        catch (FormatException exception)
        {
            logger.LogError(
                exception,
                "Invalid market data returned by Twelve Data for {Symbol}",
                symbol.Value);

            return Result<IReadOnlyList<Candle>>.Failure(
                ApplicationErrors.ProviderUnavailable);
        }
    }

    private static string MapInterval(
        Timeframe timeframe) =>
        timeframe switch
        {
            Timeframe.OneMinute => "1min",
            Timeframe.FiveMinutes => "5min",
            Timeframe.FifteenMinutes => "15min",
            Timeframe.OneHour => "1h",
            Timeframe.FourHours => "4h",
            Timeframe.OneDay => "1day",
            Timeframe.OneWeek => "1week",

            _ => throw new ArgumentOutOfRangeException(
                nameof(timeframe),
                timeframe,
                null)
        };
}
