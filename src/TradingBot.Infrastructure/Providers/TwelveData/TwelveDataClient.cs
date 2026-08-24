using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingBot.Infrastructure.Configuration;

namespace TradingBot.Infrastructure.Providers.TwelveData;

public sealed class TwelveDataClient(
    HttpClient httpClient,
    IOptions<TwelveDataOptions> options,
    ILogger<TwelveDataClient> logger)
    : ITwelveDataClient
{
    public async Task<string> GetTimeSeriesAsync(
        string symbol,
        string interval,
        int outputSize,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Requesting Twelve Data history for {Symbol} at {Interval} with {OutputSize} candles",
            symbol,
            interval,
            outputSize);

        var response =
            await httpClient.GetAsync(
                $"time_series?symbol={Uri.EscapeDataString(symbol)}" +
                $"&interval={Uri.EscapeDataString(interval)}" +
                $"&outputsize={outputSize}" +
                $"&apikey={Uri.EscapeDataString(options.Value.ApiKey)}",
                cancellationToken);

        var content =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Twelve Data returned HTTP {StatusCode} for {Symbol}: {Response}",
                (int)response.StatusCode,
                symbol,
                content);

            throw new HttpRequestException(
                $"Twelve Data returned HTTP {(int)response.StatusCode}.");
        }

        return content;
    }
}
