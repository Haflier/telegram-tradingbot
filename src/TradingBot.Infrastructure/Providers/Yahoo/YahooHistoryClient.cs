using YahooQuotesApi;

namespace TradingBot.Infrastructure.Providers.Yahoo;

public sealed class YahooHistoryClient(
    YahooQuotes yahooQuotes)
    : IYahooHistoryClient
{
    public Task<Result<History>> GetHistoryAsync(
        string symbol,
        string interval,
        CancellationToken cancellationToken)
    {
        return yahooQuotes.GetHistoryAsync(
            symbol,
            interval,
            cancellationToken);
    }
}
