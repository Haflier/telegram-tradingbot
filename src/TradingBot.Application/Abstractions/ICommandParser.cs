using TradingBot.Application.DTOs;
using TradingBot.Application.Results;

namespace TradingBot.Application.Abstractions;

public interface ICommandParser
{
    Result<ChartRequest> ParseChartCommand(string text);
}
