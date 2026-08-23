using TradingBot.Application.Results;
using TradingBot.Domain.ValueObjects;

namespace TradingBot.Application.Abstractions;

public interface IPriceDataProviderResolver
{
    Result<IPriceDataProvider> Resolve(TradingSymbol symbol);
}
