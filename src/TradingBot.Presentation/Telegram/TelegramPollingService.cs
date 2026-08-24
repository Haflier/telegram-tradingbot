using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using TradingBot.Application.Abstractions;
using TradingBot.Application.DTOs;
using TradingBot.Infrastructure.Configuration;

namespace TradingBot.Presentation.Telegram;

public sealed class TelegramPollingService(
    IServiceScopeFactory scopeFactory,
    IOptions<TelegramOptions> options,
    ILogger<TelegramPollingService> logger)
    : BackgroundService
{
    private readonly ITelegramBotClient _client =
        new TelegramBotClient(options.Value.BotToken);

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var me =
            await _client.GetMe(
                cancellationToken: stoppingToken);

        logger.LogInformation(
            "Telegram bot started: @{Username}",
            me.Username);

        var offset = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates =
                    await _client.GetUpdates(
                        offset: offset,
                        timeout: 30,
                        cancellationToken: stoppingToken);

                foreach (var update in updates)
                {
                    offset = update.Id + 1;

                    var telegramUpdate =
                        MapUpdate(update);

                    if (telegramUpdate is null)
                    {
                        continue;
                    }

                    using var scope =
                        scopeFactory.CreateScope();

                    var handler =
                        scope.ServiceProvider
                            .GetRequiredService<
                                ITelegramBotHandler>();

                    await handler.HandleUpdateAsync(
                        telegramUpdate,
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Error while polling Telegram");

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }

        logger.LogInformation(
            "Telegram polling service stopped.");
    }

    private static TelegramUpdate? MapUpdate(
        Update update)
    {
        var message = update.Message;

        if (message is null)
        {
            return new TelegramUpdate(
                update.Id,
                null);
        }

        return new TelegramUpdate(
            update.Id,
            new TelegramMessage(
                message.Chat.Id,
                message.From?.Id ?? 0,
                message.From?.Username,
                message.Text ?? string.Empty));
    }
}
