namespace TradingBot.Domain.Entities;

public sealed record Candle
{
    public DateTimeOffset Timestamp { get; }
    public decimal Open { get; }
    public decimal High { get; }
    public decimal Low { get; }
    public decimal Close { get; }
    public decimal Volume { get; }

    public Candle(
        DateTimeOffset timestamp,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        decimal volume)
    {
        if (open < 0)
            throw new ArgumentOutOfRangeException(nameof(open));

        if (high < 0)
            throw new ArgumentOutOfRangeException(nameof(high));

        if (low < 0)
            throw new ArgumentOutOfRangeException(nameof(low));

        if (close < 0)
            throw new ArgumentOutOfRangeException(nameof(close));

        if (volume < 0)
            throw new ArgumentOutOfRangeException(nameof(volume));

        if (high < open || high < close || high < low)
            throw new ArgumentException(
                "High price must be greater than or equal to Open, Close, and Low.");

        if (low > open || low > close || low > high)
            throw new ArgumentException(
                "Low price must be less than or equal to Open, Close, and High.");

        Timestamp = timestamp;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
    }
}
