using System.Text.Json;
using System.Text.Json.Serialization;

namespace TradingBot.Infrastructure.Providers.Binance;

[JsonConverter(typeof(BinanceKlineJsonConverter))]
public sealed record BinanceKlineResponse(
    long OpenTime,
    string Open,
    string High,
    string Low,
    string Close,
    string Volume);

public sealed class BinanceKlineJsonConverter
    : JsonConverter<BinanceKlineResponse>
{
    public override BinanceKlineResponse Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException(
                "Expected Binance kline to be an array.");
        }

        using var document =
            JsonDocument.ParseValue(ref reader);

        var array = document.RootElement;

        if (array.ValueKind != JsonValueKind.Array ||
            array.GetArrayLength() < 6)
        {
            throw new JsonException(
                "Invalid Binance kline response.");
        }

        return new BinanceKlineResponse(
            array[0].GetInt64(),
            array[1].GetString() ?? string.Empty,
            array[2].GetString() ?? string.Empty,
            array[3].GetString() ?? string.Empty,
            array[4].GetString() ?? string.Empty,
            array[5].GetString() ?? string.Empty);
    }

    public override void Write(
        Utf8JsonWriter writer,
        BinanceKlineResponse value,
        JsonSerializerOptions options)
    {
        throw new NotSupportedException(
            "Binance klines are read-only.");
    }
}
