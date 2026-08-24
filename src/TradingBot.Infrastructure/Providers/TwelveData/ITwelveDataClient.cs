namespace TradingBot.Infrastructure.Providers.TwelveData;

public interface ITwelveDataClient
{
    Task<string> GetTimeSeriesAsync(
        string symbol,
        string interval,
        int outputSize,
        CancellationToken cancellationToken);
}
