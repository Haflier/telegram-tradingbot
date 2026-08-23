using TradingBot.Domain.Enums;

namespace TradingBot.Application.DTOs;

public sealed record ChartRequest(
    string RawSymbol,
    Timeframe Timeframe);
