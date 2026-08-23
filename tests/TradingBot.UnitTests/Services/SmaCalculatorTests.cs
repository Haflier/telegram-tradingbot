using TradingBot.Application.Services;

namespace TradingBot.UnitTests.Services;

public sealed class SmaCalculatorTests
{
    private readonly SmaCalculator _calculator = new();

    [Fact]
    public void Calculate_ReturnsNullUntilEnoughValuesExist()
    {
        var values = new decimal[]
        {
            1, 2, 3, 4
        };

        var result =
            _calculator.Calculate(values, 3);

        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Value);

        Assert.Null(result.Value[0]);
        Assert.Null(result.Value[1]);

        Assert.Equal(
            2m,
            result.Value[2]);

        Assert.Equal(
            3m,
            result.Value[3]);
    }

    [Fact]
    public void Calculate_UsesRollingWindow()
    {
        var values = new decimal[]
        {
            10,
            20,
            30,
            40,
            50
        };

        var result =
            _calculator.Calculate(values, 3);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            20m,
            result.Value[2]);

        Assert.Equal(
            30m,
            result.Value[3]);

        Assert.Equal(
            40m,
            result.Value[4]);
    }

    [Fact]
    public void Calculate_WithEmptyInput_ReturnsFailure()
    {
        var result =
            _calculator.Calculate(
                Array.Empty<decimal>(),
                20);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "InsufficientHistoricalData",
            result.Error.Code);
    }

    [Fact]
    public void Calculate_WithInvalidPeriod_ReturnsFailure()
    {
        var result =
            _calculator.Calculate(
                [1m, 2m, 3m],
                0);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "InvalidMovingAveragePeriod",
            result.Error.Code);
    }
}
