using System.Text.RegularExpressions;
using TradingBot.Application.Abstractions;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;
using TradingBot.Domain.Enums;
using TradingBot.Domain.ValueObjects;

namespace TradingBot.Application.Services;

public sealed class SymbolResolver : ISymbolResolver
{
    private static readonly HashSet<string> KnownCryptoAssets =
    [
        "BTC",
        "ETH",
        "BNB",
        "SOL",
        "XRP",
        "ADA",
        "DOGE",
        "AVAX",
        "DOT",
        "LINK",
        "LTC"
    ];

    private static readonly HashSet<string> KnownCryptoPairs =
    [
        "BTCUSDT",
        "ETHUSDT",
        "BNBUSDT",
        "SOLUSDT",
        "XRPUSDT",
        "ADAUSDT",
        "DOGEUSDT",
        "AVAXUSDT",
        "DOTUSDT",
        "LINKUSDT",
        "LTCUSDT"
    ];

    private static readonly Regex SymbolPattern =
        new(
            "^[A-Z0-9.\\-^]{1,20}$",
            RegexOptions.Compiled);

    public Result<TradingSymbol> Resolve(string rawSymbol)
    {
        if (string.IsNullOrWhiteSpace(rawSymbol))
        {
            return Result<TradingSymbol>.Failure(
                ApplicationErrors.InvalidSymbol);
        }

        var symbol = rawSymbol
            .Trim()
            .ToUpperInvariant();

        if (!SymbolPattern.IsMatch(symbol))
        {
            return Result<TradingSymbol>.Failure(
                ApplicationErrors.InvalidSymbol);
        }

        if (KnownCryptoAssets.Contains(symbol))
        {
            return Result<TradingSymbol>.Success(
                TradingSymbol.Create(
                    $"{symbol}USDT",
                    AssetType.Crypto));
        }

        if (KnownCryptoPairs.Contains(symbol))
        {
            return Result<TradingSymbol>.Success(
                TradingSymbol.Create(
                    symbol,
                    AssetType.Crypto));
        }

        return Result<TradingSymbol>.Success(
            TradingSymbol.Create(
                symbol,
                AssetType.Stock));
    }
}
