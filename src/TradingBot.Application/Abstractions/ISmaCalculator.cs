using TradingBot.Application.Results;

namespace TradingBot.Application.Abstractions;

public interface ISmaCalculator
{
    Result<IReadOnlyList<decimal?>> Calculate(
        IReadOnlyList<decimal> values,
        int period);
}
