using TradingBot.Application.Abstractions;
using TradingBot.Application.DTOs;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;
using TradingBot.Domain.Enums;

namespace TradingBot.Application.Services;

public sealed class CommandParser : ICommandParser
{
    private static readonly Dictionary<string, Timeframe> Timeframes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["1m"] = Timeframe.OneMinute,
            ["5m"] = Timeframe.FiveMinutes,
            ["15m"] = Timeframe.FifteenMinutes,
            ["1h"] = Timeframe.OneHour,
            ["4h"] = Timeframe.FourHours,
            ["1d"] = Timeframe.OneDay,
            ["1w"] = Timeframe.OneWeek,
        };

    public Result<ChartRequest> ParseChartCommand(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result<ChartRequest>.Failure(
                ApplicationErrors.InvalidCommand);

        var parts = text
            .Trim()
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
            return Result<ChartRequest>.Failure(
                ApplicationErrors.InvalidCommand);

        var command = parts[0];

        if (!command.Equals(
                "/chart",
                StringComparison.OrdinalIgnoreCase) &&
            !command.StartsWith(
                "/chart@",
                StringComparison.OrdinalIgnoreCase))
        {
            return Result<ChartRequest>.Failure(
                ApplicationErrors.InvalidCommand);
        }

        var rawSymbol = parts[1].Trim();

        if (string.IsNullOrWhiteSpace(rawSymbol))
            return Result<ChartRequest>.Failure(
                ApplicationErrors.InvalidSymbol);

        if (!Timeframes.TryGetValue(
                parts[2],
                out var timeframe))
        {
            return Result<ChartRequest>.Failure(
                ApplicationErrors.InvalidTimeframe);
        }

        return Result<ChartRequest>.Success(
            new ChartRequest(
                rawSymbol.ToUpperInvariant(),
                timeframe));
    }
}
