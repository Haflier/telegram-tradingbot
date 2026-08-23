using TradingBot.Application.Abstractions;
using TradingBot.Application.Results;
using TradingBot.Application.Services;
using TradingBot.Domain.Entities;
using TradingBot.Domain.Enums;
using TradingBot.Domain.ValueObjects;

namespace TradingBot.UnitTests.Services;

public sealed class PriceDataProviderResolverTests
{
    [Fact]
    public void Resolve_ReturnsProviderThatCanHandleSymbol()
    {
        var cryptoProvider =
            new FakeProvider(AssetType.Crypto);

        var stockProvider =
            new FakeProvider(AssetType.Stock);

        var resolver =
            new PriceDataProviderResolver(
            [
                cryptoProvider,
                stockProvider
            ]);

        var symbol =
            TradingSymbol.Create(
                "BTCUSDT",
                AssetType.Crypto);

        var result =
            resolver.Resolve(symbol);

        Assert.True(result.IsSuccess);

        Assert.Same(
            cryptoProvider,
            result.Value);
    }

    [Fact]
    public void Resolve_WhenNoProviderCanHandle_ReturnsFailure()
    {
        var provider =
            new FakeProvider(AssetType.Crypto);

        var resolver =
            new PriceDataProviderResolver([provider]);

        var symbol =
            TradingSymbol.Create(
                "AAPL",
                AssetType.Stock);

        var result =
            resolver.Resolve(symbol);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "UnsupportedSymbol",
            result.Error.Code);
    }

    private sealed class FakeProvider(
        AssetType supportedAssetType)
        : IPriceDataProvider
    {
        public bool CanHandle(
            TradingSymbol symbol) =>
            symbol.AssetType == supportedAssetType;

        public Task<Result<IReadOnlyList<Candle>>> GetCandlesAsync(
            TradingSymbol symbol,
            Timeframe timeframe,
            int count,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Result<IReadOnlyList<Candle>>.Success(
                    Array.Empty<Candle>()));
        }
    }
}
