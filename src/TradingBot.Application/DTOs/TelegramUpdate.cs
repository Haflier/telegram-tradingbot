namespace TradingBot.Application.DTOs;

public sealed record TelegramUpdate(
    long UpdateId,
    TelegramMessage? Message);

public sealed record TelegramMessage(
    long ChatId,
    long UserId,
    string? Username,
    string Text);
