using TradingBot.Application.DTOs;
using TradingBot.Application.Results;

namespace TradingBot.Application.Abstractions;

public interface IChartService
{
    Task<Result<GeneratedChart>> GenerateChartAsync(
        ChartRequest request,
        CancellationToken cancellationToken);
}
