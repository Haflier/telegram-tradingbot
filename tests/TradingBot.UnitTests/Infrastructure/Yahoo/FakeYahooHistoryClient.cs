using YahooResult = YahooQuotesApi.Result<YahooQuotesApi.History>;
using YahooQuotesApi;
using TradingBot.Infrastructure.Providers.Yahoo;

namespace TradingBot.UnitTests.Infrastructure.Yahoo;

internal sealed class FakeYahooHistoryClient : IYahooHistoryClient
{
    private readonly YahooResult _result;

    public FakeYahooHistoryClient(YahooResult result)
    {
        _result = result;
    }

    public Task<YahooResult> GetHistoryAsync(
        string symbol,
        string interval,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_result);
    }
}
