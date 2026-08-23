using TradingBot.Application.Services;
using TradingBot.Domain.Enums;

namespace TradingBot.UnitTests.Services;

public sealed class CommandParserTests
{
    private readonly CommandParser _parser = new();

    [Theory]
    [InlineData("/chart BTC 4h", "BTC", Timeframe.FourHours)]
    [InlineData("/chart BTCUSDT 1h", "BTCUSDT", Timeframe.OneHour)]
    [InlineData("/chart ETH 1d", "ETH", Timeframe.OneDay)]
    [InlineData("/chart AAPL 1d", "AAPL", Timeframe.OneDay)]
    [InlineData("/chart SPY 1w", "SPY", Timeframe.OneWeek)]
    public void ParseChartCommand_ValidCommand_ReturnsRequest(
        string command,
        string expectedSymbol,
        Timeframe expectedTimeframe)
    {
        var result = _parser.ParseChartCommand(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(
            expectedSymbol,
            result.Value.RawSymbol);

        Assert.Equal(
            expectedTimeframe,
            result.Value.Timeframe);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/chart")]
    [InlineData("/chart BTC")]
    [InlineData("/chart BTC 4h extra")]
    [InlineData("/foo BTC 4h")]
    public void ParseChartCommand_InvalidCommand_ReturnsFailure(
        string command)
    {
        var result = _parser.ParseChartCommand(command);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "InvalidCommand",
            result.Error.Code);
    }

    [Theory]
    [InlineData("/chart BTC 2h")]
    [InlineData("/chart BTC 30m")]
    [InlineData("/chart BTC abc")]
    public void ParseChartCommand_InvalidTimeframe_ReturnsFailure(
        string command)
    {
        var result = _parser.ParseChartCommand(command);

        Assert.True(result.IsFailure);
        Assert.Equal(
            "InvalidTimeframe",
            result.Error.Code);
    }

    [Fact]
    public void ParseChartCommand_IsCaseInsensitive()
    {
        var result =
            _parser.ParseChartCommand("/CHART btc 4H");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(
            "BTC",
            result.Value.RawSymbol);

        Assert.Equal(
            Timeframe.FourHours,
            result.Value.Timeframe);
    }

    [Fact]
    public void ParseChartCommand_SupportsBotMention()
    {
        var result =
            _parser.ParseChartCommand(
                "/chart@MyTradingBot BTC 4h");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(
            "BTC",
            result.Value.RawSymbol);
    }
}
