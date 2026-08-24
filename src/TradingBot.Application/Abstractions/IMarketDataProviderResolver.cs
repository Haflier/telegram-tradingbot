using TradingBot.Domain.ValueObjects;

namespace TradingBot.Application.Abstractions;

public interface IMarketDataProviderResolver
{
    IPriceDataProvider Resolve(TradingSymbol symbol);
}
