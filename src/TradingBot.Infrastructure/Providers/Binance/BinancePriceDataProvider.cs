using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingBot.Application.Abstractions;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;
using TradingBot.Domain.Entities;
using TradingBot.Domain.Enums;
using TradingBot.Domain.ValueObjects;
using TradingBot.Infrastructure.Configuration;
using TradingBot.Infrastructure.Http;

namespace TradingBot.Infrastructure.Providers.Binance;

public sealed class BinancePriceDataProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<BinanceOptions> options,
    ILogger<BinancePriceDataProvider> logger)
    : IPriceDataProvider
{
    private readonly BinanceOptions _options = options.Value;

    public bool CanHandle(TradingSymbol symbol) =>
        symbol.AssetType == AssetType.Crypto;

    public async Task<Result<IReadOnlyList<Candle>>> GetCandlesAsync(
        TradingSymbol symbol,
        Timeframe timeframe,
        int count,
        CancellationToken cancellationToken)
    {
        try
        {
            var client =
                httpClientFactory.CreateClient(
                    HttpClientNames.Binance);

            var interval =
                BinanceTimeframeMapper.Map(timeframe);

            var url =
                $"/api/v3/klines" +
                $"?symbol={Uri.EscapeDataString(symbol.Value)}" +
                $"&interval={interval}" +
                $"&limit={count}";

            logger.LogInformation(
                "Requesting Binance klines for {Symbol} at {Timeframe} with {Count} candles",
                symbol.Value,
                interval,
                count);

            using var response =
                await client.GetAsync(
                    url,
                    cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest)
            {
                logger.LogWarning(
                    "Binance rejected symbol {Symbol}",
                    symbol.Value);

                return Result<IReadOnlyList<Candle>>.Failure(
                    ApplicationErrors.InvalidSymbol);
            }

            if (response.StatusCode == (HttpStatusCode)429)
            {
                logger.LogWarning(
                    "Binance rate limited request for {Symbol}",
                    symbol.Value);

                return Result<IReadOnlyList<Candle>>.Failure(
                    ApplicationErrors.ProviderRateLimited);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "Binance returned HTTP {StatusCode} for {Symbol}",
                    (int)response.StatusCode,
                    symbol.Value);

                return Result<IReadOnlyList<Candle>>.Failure(
                    ApplicationErrors.ProviderUnavailable);
            }

            await using var stream =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken);

            var klines =
                await JsonSerializer.DeserializeAsync<
                    List<BinanceKlineResponse>>(
                    stream,
                    cancellationToken: cancellationToken);

            if (klines is null || klines.Count == 0)
            {
                return Result<IReadOnlyList<Candle>>.Failure(
                    ApplicationErrors.InsufficientHistoricalData);
            }

            var candles =
                new List<Candle>(klines.Count);

            foreach (var kline in klines)
            {
                if (!decimal.TryParse(
                        kline.Open,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var open) ||
                    !decimal.TryParse(
                        kline.High,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var high) ||
                    !decimal.TryParse(
                        kline.Low,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var low) ||
                    !decimal.TryParse(
                        kline.Close,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var close) ||
                    !decimal.TryParse(
                        kline.Volume,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var volume))
                {
                    logger.LogError(
                        "Binance returned malformed price data for {Symbol}",
                        symbol.Value);

                    return Result<IReadOnlyList<Candle>>.Failure(
                        ApplicationErrors.ProviderUnavailable);
                }

                candles.Add(
                    new Candle(
                        DateTimeOffset
                            .FromUnixTimeMilliseconds(
                                kline.OpenTime),
                        open,
                        high,
                        low,
                        close,
                        volume));
            }

            return Result<IReadOnlyList<Candle>>.Success(
                candles);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                "Binance request timed out for {Symbol}",
                symbol.Value);

            return Result<IReadOnlyList<Candle>>.Failure(
                ApplicationErrors.ProviderTimeout);
        }
        catch (JsonException exception)
        {
            logger.LogError(
                exception,
                "Failed to deserialize Binance response for {Symbol}",
                symbol.Value);

            return Result<IReadOnlyList<Candle>>.Failure(
                ApplicationErrors.ProviderUnavailable);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                exception,
                "HTTP error while contacting Binance for {Symbol}",
                symbol.Value);

            return Result<IReadOnlyList<Candle>>.Failure(
                ApplicationErrors.ProviderUnavailable);
        }
    }
}
