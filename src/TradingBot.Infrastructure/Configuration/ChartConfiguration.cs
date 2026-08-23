using Microsoft.Extensions.Options;
using TradingBot.Application.Abstractions;

namespace TradingBot.Infrastructure.Configuration;

public sealed class ChartConfiguration(
    IOptions<ChartOptions> options)
    : IChartConfiguration
{
    private readonly ChartOptions _options = options.Value;

    public int CandleCount =>
        _options.CandleCount;

    public int MovingAveragePeriod =>
        _options.MovingAveragePeriod;
}
