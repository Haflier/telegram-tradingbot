using TradingBot.Application.DTOs;
using TradingBot.Application.Results;

namespace TradingBot.Application.Abstractions;

public interface ITelegramSender
{
    Task<Result> SendTextAsync(
        long chatId,
        string text,
        CancellationToken cancellationToken);

    Task<Result> SendChartAsync(
        long chatId,
        GeneratedChart chart,
        CancellationToken cancellationToken);
}
