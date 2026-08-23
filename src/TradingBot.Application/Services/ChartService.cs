using TradingBot.Application.Abstractions;
using TradingBot.Application.DTOs;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;

namespace TradingBot.Application.Services;

public sealed class ChartService(
    ISymbolResolver symbolResolver,
    IPriceDataProviderResolver providerResolver,
    ISmaCalculator smaCalculator,
    IChartGenerator chartGenerator)
    : IChartService
{
    private const int CandleCount = 100;
    private const int MovingAveragePeriod = 20;

    public async Task<Result<GeneratedChart>> GenerateChartAsync(
        ChartRequest request,
        CancellationToken cancellationToken)
    {
        var symbolResult =
            symbolResolver.Resolve(request.RawSymbol);

        if (symbolResult.IsFailure)
            return Result<GeneratedChart>.Failure(
                symbolResult.Error);

        var symbol = symbolResult.Value!;

        var providerResult =
            providerResolver.Resolve(symbol);

        if (providerResult.IsFailure)
            return Result<GeneratedChart>.Failure(
                providerResult.Error);

        var provider = providerResult.Value!;

        var candlesResult =
            await provider.GetCandlesAsync(
                symbol,
                request.Timeframe,
                CandleCount,
                cancellationToken);

        if (candlesResult.IsFailure)
            return Result<GeneratedChart>.Failure(
                candlesResult.Error);

        var candles = candlesResult.Value!;

        if (candles.Count != CandleCount)
        {
            return Result<GeneratedChart>.Failure(
                ApplicationErrors.InsufficientHistoricalData);
        }

        var closingPrices = candles
            .Select(candle => candle.Close)
            .ToArray();

        var movingAverageResult =
            smaCalculator.Calculate(
                closingPrices,
                MovingAveragePeriod);

        if (movingAverageResult.IsFailure)
            return Result<GeneratedChart>.Failure(
                movingAverageResult.Error);

        var chartData = new ChartData(
            symbol,
            candles,
            movingAverageResult.Value!);

        return chartGenerator.Generate(chartData);
    }
}
