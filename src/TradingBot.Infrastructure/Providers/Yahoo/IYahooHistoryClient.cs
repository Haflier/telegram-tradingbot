using YahooQuotesApi;

namespace TradingBot.Infrastructure.Providers.Yahoo;

public interface IYahooHistoryClient
{
    Task<Result<History>> GetHistoryAsync(
        string symbol,
        string interval,
        CancellationToken cancellationToken);
}
