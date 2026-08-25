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

        /*
         * Handle /start separately.
         */
        if (message.Text.Trim().Equals(
                "/start",
                StringComparison.OrdinalIgnoreCase) ||
            message.Text.Trim().StartsWith(
                "/start@",
                StringComparison.OrdinalIgnoreCase))
        {
            await telegramSender.SendTextAsync(
                message.ChatId,
                """
                Welcome to @Cryptockerbot! 📈

                Get market charts and price information directly from Telegram.

                Usage:
                /chart <symbol> <timeframe>

                Examples:
                /chart BTC 4h
                /chart ETH 1h
                /chart AAPL 1d

                Supported timeframes:
                1m 5m 15m 1h 4h 1d 1w
                """,
                cancellationToken);

            return;
        }

        var commandResult =
            commandParser.ParseChartCommand(
                message.Text);

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

        var chart =
            chartResult.Value!;

        await telegramSender.SendChartAsync(
            message.ChatId,
            chart,
            cancellationToken);
    }
}
