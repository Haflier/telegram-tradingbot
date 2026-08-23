using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TradingBot.Domain.Enums;
using TradingBot.Domain.ValueObjects;
using TradingBot.Infrastructure.Configuration;
using TradingBot.Infrastructure.Http;
using TradingBot.Infrastructure.Providers.Binance;

namespace TradingBot.UnitTests.Infrastructure.Binance;

public sealed class BinancePriceDataProviderTests
{
    [Fact]
    public async Task GetCandlesAsync_ValidResponse_ReturnsCandles()
    {
        const string json =
            """
            [
              [
                1672531200000,
                "16500.00",
                "16600.00",
                "16400.00",
                "16550.00",
                "1234.567",
                1672545599999,
                "20345678.90",
                1000,
                "600.123",
                "10012345.67",
                "0"
              ]
            ]
            """;

        var handler =
            new FakeHttpMessageHandler(
                HttpStatusCode.OK,
                json);

        var httpClient =
            new HttpClient(handler)
            {
                BaseAddress =
                    new Uri("https://api.binance.com")
            };

        var factory =
            new FakeHttpClientFactory(
                HttpClientNames.Binance,
                httpClient);

        var options =
            Options.Create(
                new BinanceOptions
                {
                    BaseUrl =
                        "https://api.binance.com",
                    TimeoutSeconds = 10
                });

        var provider =
            new BinancePriceDataProvider(
                factory,
                options,
                NullLogger<BinancePriceDataProvider>.Instance);

        var symbol =
            TradingSymbol.Create(
                "BTCUSDT",
                AssetType.Crypto);

        var result =
            await provider.GetCandlesAsync(
                symbol,
                Timeframe.FourHours,
                1,
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var candle = result.Value[0];

        Assert.Equal(16500m, candle.Open);
        Assert.Equal(16600m, candle.High);
        Assert.Equal(16400m, candle.Low);
        Assert.Equal(16550m, candle.Close);
        Assert.Equal(1234.567m, candle.Volume);

        Assert.Equal(
            DateTimeOffset.FromUnixTimeMilliseconds(
                1672531200000),
            candle.Timestamp);
    }

    [Fact]
    public async Task GetCandlesAsync_BadRequest_ReturnsInvalidSymbol()
    {
        var handler =
            new FakeHttpMessageHandler(
                HttpStatusCode.BadRequest,
                """
                {
                    "code": -1121,
                    "msg": "Invalid symbol."
                }
                """);

        var httpClient =
            new HttpClient(handler)
            {
                BaseAddress =
                    new Uri("https://api.binance.com")
            };

        var provider =
            CreateProvider(httpClient);

        var result =
            await provider.GetCandlesAsync(
                TradingSymbol.Create(
                    "INVALID",
                    AssetType.Crypto),
                Timeframe.OneHour,
                100,
                CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "InvalidSymbol",
            result.Error.Code);
    }

    [Fact]
    public async Task GetCandlesAsync_RateLimited_ReturnsRateLimitError()
    {
        var handler =
            new FakeHttpMessageHandler(
                (HttpStatusCode)429,
                "{}");

        var httpClient =
            new HttpClient(handler)
            {
                BaseAddress =
                    new Uri("https://api.binance.com")
            };

        var provider =
            CreateProvider(httpClient);

        var result =
            await provider.GetCandlesAsync(
                TradingSymbol.Create(
                    "BTCUSDT",
                    AssetType.Crypto),
                Timeframe.OneHour,
                100,
                CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "ProviderRateLimited",
            result.Error.Code);
    }

    [Fact]
    public async Task GetCandlesAsync_ServerError_ReturnsUnavailable()
    {
        var handler =
            new FakeHttpMessageHandler(
                HttpStatusCode.ServiceUnavailable,
                "{}");

        var httpClient =
            new HttpClient(handler)
            {
                BaseAddress =
                    new Uri("https://api.binance.com")
            };

        var provider =
            CreateProvider(httpClient);

        var result =
            await provider.GetCandlesAsync(
                TradingSymbol.Create(
                    "BTCUSDT",
                    AssetType.Crypto),
                Timeframe.OneHour,
                100,
                CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "ProviderUnavailable",
            result.Error.Code);
    }

    private static BinancePriceDataProvider CreateProvider(
        HttpClient httpClient)
    {
        var factory =
            new FakeHttpClientFactory(
                HttpClientNames.Binance,
                httpClient);

        return new BinancePriceDataProvider(
            factory,
            Options.Create(
                new BinanceOptions
                {
                    BaseUrl =
                        "https://api.binance.com",
                    TimeoutSeconds = 10
                }),
            NullLogger<BinancePriceDataProvider>.Instance);
    }

    private sealed class FakeHttpClientFactory(
        string name,
        HttpClient client)
        : IHttpClientFactory
    {
        public HttpClient CreateClient(string clientName)
        {
            if (clientName != name)
                throw new InvalidOperationException(
                    $"Unexpected HTTP client: {clientName}");

            return client;
        }
    }

    private sealed class FakeHttpMessageHandler(
        HttpStatusCode statusCode,
        string content)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response =
                new HttpResponseMessage(statusCode)
                {
                    Content =
                        new StringContent(content)
                };

            return Task.FromResult(response);
        }
    }
}
