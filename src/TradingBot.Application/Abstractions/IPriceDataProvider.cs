using TradingBot.Application.Results;
using TradingBot.Domain.Entities;
using TradingBot.Domain.ValueObjects;
using TradingBot.Domain.Enums;

namespace TradingBot.Application.Abstractions;

public interface IPriceDataProvider
{
    bool CanHandle(TradingSymbol symbol);

    Task<Result<IReadOnlyList<Candle>>> GetCandlesAsync(
        TradingSymbol symbol,
        Timeframe timeframe,
        int count,
        CancellationToken cancellationToken);
}
