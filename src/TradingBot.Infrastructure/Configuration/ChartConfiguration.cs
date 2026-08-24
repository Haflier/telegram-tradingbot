using Microsoft.Extensions.Options;
using TradingBot.Application.Abstractions;

namespace TradingBot.Infrastructure.Configuration;

public sealed class ChartConfiguration(
    IOptions<ChartOptions> options)
    : IChartConfiguration, IChartDimensions
{
    private readonly ChartOptions _options = options.Value;

    public int CandleCount =>
        _options.CandleCount;

    public int MovingAveragePeriod =>
        _options.MovingAveragePeriod;

    public int Width =>
        _options.Width;

    public int Height =>
        _options.Height;
}
