using TradingBot.Application.Services;
using TradingBot.Domain.Enums;

namespace TradingBot.UnitTests.Services;

public sealed class SymbolResolverTests
{
    private readonly SymbolResolver _resolver = new();

    [Theory]
    [InlineData("BTC", "BTCUSDT")]
    [InlineData("btc", "BTCUSDT")]
    [InlineData("ETH", "ETHUSDT")]
    [InlineData("SOL", "SOLUSDT")]
    public void Resolve_BareCryptoSymbol_NormalizesToUsdtPair(
        string input,
        string expected)
    {
        var result = _resolver.Resolve(input);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(
            expected,
            result.Value.Value);

        Assert.Equal(
            AssetType.Crypto,
            result.Value.AssetType);
    }

    [Theory]
    [InlineData("BTCUSDT")]
    [InlineData("ETHUSDT")]
    [InlineData("SOLUSDT")]
    public void Resolve_CryptoPair_PreservesPair(
        string input)
    {
        var result = _resolver.Resolve(input);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(
            input,
            result.Value.Value);

        Assert.Equal(
            AssetType.Crypto,
            result.Value.AssetType);
    }

    [Theory]
    [InlineData("AAPL")]
    [InlineData("TSLA")]
    [InlineData("SPY")]
    public void Resolve_NonCryptoSymbol_ClassifiesAsStock(
        string input)
    {
        var result = _resolver.Resolve(input);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(
            input,
            result.Value.Value);

        Assert.Equal(
            AssetType.Stock,
            result.Value.AssetType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("BTC!")]
    [InlineData("THIS_SYMBOL_IS_WAY_TOO_LONG")]
    public void Resolve_InvalidSymbol_ReturnsFailure(
        string input)
    {
        var result = _resolver.Resolve(input);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "InvalidSymbol",
            result.Error.Code);
    }
}
