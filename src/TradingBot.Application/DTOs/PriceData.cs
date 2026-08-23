using TradingBot.Domain.Entities;
using TradingBot.Domain.ValueObjects;

namespace TradingBot.Application.DTOs;

public sealed record PriceData(
    TradingSymbol Symbol,
    IReadOnlyList<Candle> Candles);
