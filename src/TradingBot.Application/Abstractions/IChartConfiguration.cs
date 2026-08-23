namespace TradingBot.Application.Abstractions;

public interface IChartConfiguration
{
    int CandleCount { get; }

    int MovingAveragePeriod { get; }
}
