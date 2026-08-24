using TradingBot.Application.Abstractions;
using TradingBot.Application.DTOs;

namespace TradingBot.Application.Services;

public sealed class TelegramBotHandler(
    ICommandParser commandParser,
    IChartService chartService,
    ITelegramSender telegramSender)
    : ITelegramBotHandler
{
    public async Task HandleUpdateAsync(
        TelegramUpdate update,
        CancellationToken cancellationToken)
    {
        var message = update.Message;

        if (message is null ||
            string.IsNullOrWhiteSpace(message.Text))
        {
            return;
        }

        var commandResult =
            commandParser.ParseChartCommand(message.Text);

        if (commandResult.IsFailure)
        {
            await telegramSender.SendTextAsync(
                message.ChatId,
                commandResult.Error.Message,
                cancellationToken);

            return;
        }

        var chartResult =
            await chartService.GenerateChartAsync(
                commandResult.Value!,
                cancellationToken);

        if (chartResult.IsFailure)
        {
            await telegramSender.SendTextAsync(
                message.ChatId,
                chartResult.Error.Message,
                cancellationToken);

            return;
        }

        await telegramSender.SendChartAsync(
            message.ChatId,
            chartResult.Value!,
            cancellationToken);
    }
}
