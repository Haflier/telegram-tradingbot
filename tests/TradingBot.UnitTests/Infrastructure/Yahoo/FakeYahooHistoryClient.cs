using YahooResult = YahooQuotesApi.Result<YahooQuotesApi.History>;
using TradingBot.Infrastructure.Providers.Yahoo;

namespace TradingBot.UnitTests.Infrastructure.Yahoo;

internal sealed class FakeYahooHistoryClient(
    YahooResult result,
    Exception? exception = null)
    : IYahooHistoryClient
{
    public async Task<YahooResult> GetHistoryAsync(
    string symbol,
    string interval,
    CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (exception is not null)
        {
            throw exception;
        }

        await Task.Yield();

        cancellationToken.ThrowIfCancellationRequested();

        return result;
    }
}
