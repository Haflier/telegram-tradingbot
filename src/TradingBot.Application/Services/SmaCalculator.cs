using TradingBot.Application.Abstractions;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;

namespace TradingBot.Application.Services;

public sealed class SmaCalculator : ISmaCalculator
{
    public Result<IReadOnlyList<decimal?>> Calculate(
        IReadOnlyList<decimal> values,
        int period)
    {
        if (period <= 0)
        {
            return Result<IReadOnlyList<decimal?>>.Failure(
                new(
                    "InvalidMovingAveragePeriod",
                    "Moving average period must be greater than zero."));
        }

        if (values.Count == 0)
        {
            return Result<IReadOnlyList<decimal?>>.Failure(
                ApplicationErrors.InsufficientHistoricalData);
        }

        var result = new decimal?[values.Count];

        decimal sum = 0;

        for (var i = 0; i < values.Count; i++)
        {
            sum += values[i];

            if (i >= period)
                sum -= values[i - period];

            if (i >= period - 1)
                result[i] = sum / period;
        }

        return Result<IReadOnlyList<decimal?>>.Success(result);
    }
}
