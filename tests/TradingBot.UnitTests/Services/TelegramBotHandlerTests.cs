using TradingBot.Application.Abstractions;
using TradingBot.Application.DTOs;
using TradingBot.Application.Errors;
using TradingBot.Application.Results;
using TradingBot.Application.Services;
using TradingBot.Domain.Errors;
using TradingBot.Domain.Enums;
using TradingBot.Domain.Entities;

namespace TradingBot.UnitTests.Services;

public sealed class TelegramBotHandlerTests
{
    [Fact]
    public async Task HandleUpdateAsync_WhenMessageIsNull_DoesNothing()
    {
        var sender = new FakeTelegramSender();

        var handler =
            new TelegramBotHandler(
                new FakeCommandParser(),
                new FakeChartService(),
                sender);

        await handler.HandleUpdateAsync(
            new TelegramUpdate(1, null),
            CancellationToken.None);

        Assert.False(sender.SendTextCalled);
        Assert.False(sender.SendChartCalled);
    }

    [Fact]
    public async Task HandleUpdateAsync_WhenMessageTextIsEmpty_DoesNothing()
    {
        var sender = new FakeTelegramSender();

        var handler =
            new TelegramBotHandler(
                new FakeCommandParser(),
                new FakeChartService(),
                sender);

        var message =
            new TelegramMessage(
                123,
                456,
                "test",
                "");

        await handler.HandleUpdateAsync(
            new TelegramUpdate(1, message),
            CancellationToken.None);

        Assert.False(sender.SendTextCalled);
        Assert.False(sender.SendChartCalled);
    }

    [Fact]
    public async Task HandleUpdateAsync_WhenCommandIsInvalid_SendsError()
    {
        var sender = new FakeTelegramSender();

        var parser =
            new FakeCommandParser(
                Result<ChartRequest>.Failure(
                    ApplicationErrors.InvalidCommand));

        var handler =
            new TelegramBotHandler(
                parser,
                new FakeChartService(),
                sender);

        var message =
            CreateMessage("/invalid");

        await handler.HandleUpdateAsync(
            new TelegramUpdate(1, message),
            CancellationToken.None);

        Assert.True(sender.SendTextCalled);
        Assert.Equal(
            123,
            sender.LastChatId);

        Assert.Equal(
            "The command format is invalid.",
            sender.LastText);

        Assert.False(sender.SendChartCalled);
    }

    [Fact]
    public async Task HandleUpdateAsync_WhenChartGenerationFails_SendsError()
    {
        var sender = new FakeTelegramSender();

        var chartService =
            new FakeChartService(
                Result<GeneratedChart>.Failure(
                    ApplicationErrors.ProviderUnavailable));

        var handler =
            new TelegramBotHandler(
                new FakeCommandParser(),
                chartService,
                sender);

        var message =
            CreateMessage("/chart BTC 4h");

        await handler.HandleUpdateAsync(
            new TelegramUpdate(1, message),
            CancellationToken.None);

        Assert.True(sender.SendTextCalled);

        Assert.Equal(
            "The market data provider is temporarily unavailable.",
            sender.LastText);

        Assert.False(sender.SendChartCalled);
    }

    [Fact]
    public async Task HandleUpdateAsync_WhenChartGenerationSucceeds_SendsChart()
    {
        var sender = new FakeTelegramSender();

        var chart =
            new GeneratedChart(
                new MemoryStream(),
                "BTCUSDT.png",
                "image/png");

        var chartService =
            new FakeChartService(
                Result<GeneratedChart>.Success(chart));

        var handler =
            new TelegramBotHandler(
                new FakeCommandParser(),
                chartService,
                sender);

        var message =
            CreateMessage("/chart BTC 4h");

        await handler.HandleUpdateAsync(
            new TelegramUpdate(1, message),
            CancellationToken.None);

        Assert.True(sender.SendChartCalled);
        Assert.Same(chart, sender.LastChart);

        Assert.False(sender.SendTextCalled);
    }

    private static TelegramMessage CreateMessage(
        string text)
    {
        return new TelegramMessage(
            123,
            456,
            "test",
            text);
    }

    private sealed class FakeCommandParser(
        Result<ChartRequest>? result = null)
        : ICommandParser
    {
        private readonly Result<ChartRequest> _result =
            result ??
            Result<ChartRequest>.Success(
                new ChartRequest(
                    "BTC",
                    Timeframe.FourHours));

        public Result<ChartRequest> ParseChartCommand(
            string text)
        {
            return _result;
        }
    }

    private sealed class FakeChartService(
        Result<GeneratedChart>? result = null)
        : IChartService
    {
        private readonly Result<GeneratedChart> _result =
            result ??
            Result<GeneratedChart>.Success(
                new GeneratedChart(
                    new MemoryStream(),
                    "BTCUSDT.png",
                    "image/png"));

        public Task<Result<GeneratedChart>> GenerateChartAsync(
            ChartRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class FakeTelegramSender
        : ITelegramSender
    {
        public bool SendTextCalled { get; private set; }

        public bool SendChartCalled { get; private set; }

        public long LastChatId { get; private set; }

        public string? LastText { get; private set; }

        public GeneratedChart? LastChart { get; private set; }

        public Task<Result> SendTextAsync(
            long chatId,
            string text,
            CancellationToken cancellationToken)
        {
            SendTextCalled = true;
            LastChatId = chatId;
            LastText = text;

            return Task.FromResult(
                Result.Success());
        }

        public Task<Result> SendChartAsync(
            long chatId,
            GeneratedChart chart,
            CancellationToken cancellationToken)
        {
            SendChartCalled = true;
            LastChatId = chatId;
            LastChart = chart;

            return Task.FromResult(
                Result.Success());
        }
    }
}
