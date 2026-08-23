using TradingBot.Application.Results;
using TradingBot.Domain.ValueObjects;

namespace TradingBot.Application.Abstractions;

public interface ISymbolResolver
{
    Result<TradingSymbol> Resolve(string rawSymbol);
}
