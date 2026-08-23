namespace TradingBot.Infrastructure.Configuration;

public sealed class ChartOptions
{
    public const string SectionName = "Chart";

    public int CandleCount { get; init; } = 100;

    public int MovingAveragePeriod { get; init; } = 20;

    public int Width { get; init; } = 1600;

    public int Height { get; init; } = 900;
}
