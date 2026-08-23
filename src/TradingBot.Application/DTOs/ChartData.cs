using TradingBot.Domain.Entities;
using TradingBot.Domain.ValueObjects;

namespace TradingBot.Application.DTOs;

public sealed record ChartData(
    TradingSymbol Symbol,
    IReadOnlyList<Candle> Candles,
    IReadOnlyList<decimal?> MovingAverage);
