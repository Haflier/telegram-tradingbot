using TradingBot.Domain.Enums;

namespace TradingBot.Domain.ValueObjects;

public sealed record TradingSymbol
{
    public string Value { get; }
    public AssetType AssetType { get; }

    private TradingSymbol(string value, AssetType assetType)
    {
        Value = value;
        AssetType = assetType;
    }

    public static TradingSymbol Create(
        string value,
        AssetType assetType)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Symbol cannot be empty.",
                nameof(value));

        return new TradingSymbol(
            value.Trim().ToUpperInvariant(),
            assetType);
    }

    public override string ToString() => Value;
}
