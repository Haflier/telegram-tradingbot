namespace TradingBot.Application.DTOs;

public sealed record GeneratedChart(
    Stream Content,
    string FileName,
    string ContentType,
    string? Caption = null);
