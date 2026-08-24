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
                <b>Welcome to Cryptockerbot! 📈</b>

                Get market charts and price information directly from Telegram.

                <b>Usage:</b>
                <code>/chart BTC 1d</code>

                <b>Examples:</b>
                <code>/chart BTC 4h</code>
                <code>/chart ETH 1h</code>
                <code>/chart AAPL 1d</code>

                <b>Supported timeframes:</b>
                <code>1m</code> <code>5m</code> <code>15m</code>
                <code>1h</code> <code>4h</code> <code>1d</code>
                <code>1w</code> <code>1mo</code>
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
