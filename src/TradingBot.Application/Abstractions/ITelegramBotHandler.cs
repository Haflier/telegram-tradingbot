using TradingBot.Application.DTOs;

namespace TradingBot.Application.Abstractions;

public interface ITelegramBotHandler
{
    Task HandleUpdateAsync(
        TelegramUpdate update,
        CancellationToken cancellationToken);
}
