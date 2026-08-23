using TradingBot.Application.Abstractions;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;
using TradingBot.Domain.ValueObjects;

namespace TradingBot.Application.Services;

public sealed class PriceDataProviderResolver(
    IEnumerable<IPriceDataProvider> providers)
    : IPriceDataProviderResolver
{
    private readonly IReadOnlyList<IPriceDataProvider> _providers =
        providers.ToList();

    public Result<IPriceDataProvider> Resolve(
        TradingSymbol symbol)
    {
        var provider = _providers
            .FirstOrDefault(x => x.CanHandle(symbol));

        return provider is not null
            ? Result<IPriceDataProvider>.Success(provider)
            : Result<IPriceDataProvider>.Failure(
                ApplicationErrors.UnsupportedSymbol);
    }
}
