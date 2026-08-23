using TradingBot.Application.DTOs;
using TradingBot.Application.Results;

namespace TradingBot.Application.Abstractions;

public interface IChartGenerator
{
    Result<GeneratedChart> Generate(ChartData data);
}
