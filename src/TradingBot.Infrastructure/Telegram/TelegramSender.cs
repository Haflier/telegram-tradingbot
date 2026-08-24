using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TradingBot.Application.DTOs;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;
using TradingBot.Infrastructure.Configuration;

namespace TradingBot.Infrastructure.Telegram;

public sealed class TelegramSender(
    IOptions<TelegramOptions> options,
    ILogger<TelegramSender> logger)
    : TradingBot.Application.Abstractions.ITelegramSender
{
    private readonly ITelegramBotClient _client =
        new TelegramBotClient(options.Value.BotToken);

    public async Task<Result> SendTextAsync(
        long chatId,
        string text,
        CancellationToken cancellationToken)
    {
        try
        {
            await _client.SendMessage(
                chatId,
                text,
                cancellationToken: cancellationToken);

            return Result.Success();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to send Telegram text message to chat {ChatId}",
                chatId);

            return Result.Failure(
                ApplicationErrors.TelegramDeliveryFailed);
        }
    }

    public async Task<Result> SendChartAsync(
        long chatId,
        GeneratedChart chart,
        CancellationToken cancellationToken)
    {
        try
        {
            if (chart.Content.CanSeek)
            {
                chart.Content.Position = 0;
            }

            using var stream = chart.Content;

            await _client.SendPhoto(
                chatId,
                InputFile.FromStream(
                    stream,
                    chart.FileName),
                caption: chart.Caption,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);

            return Result.Success();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to send Telegram chart to chat {ChatId}",
                chatId);

            return Result.Failure(
                ApplicationErrors.TelegramDeliveryFailed);
        }
    }
}
