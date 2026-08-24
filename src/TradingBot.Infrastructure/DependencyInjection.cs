using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradingBot.Application.Abstractions;
using TradingBot.Infrastructure.Configuration;
using TradingBot.Infrastructure.Http;
using TradingBot.Infrastructure.Providers.Binance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using YahooQuotesApi;
using TradingBot.Infrastructure.Providers.Yahoo;
using TradingBot.Infrastructure.Charts;
using TradingBot.Infrastructure.Telegram;
using TradingBot.Application.Services;

namespace TradingBot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<BinanceOptions>()
            .Bind(configuration.GetSection(
                BinanceOptions.SectionName))
            .Validate(
                options =>
                    Uri.TryCreate(
                        options.BaseUrl,
                        UriKind.Absolute,
                        out _),
                "Binance BaseUrl must be a valid absolute URI.")
            .Validate(
                options => options.TimeoutSeconds > 0,
                "Binance timeout must be greater than zero.");

        services
            .AddOptions<YahooOptions>()
            .Bind(configuration.GetSection(
                YahooOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.UserAgent),
                "Yahoo UserAgent must not be empty.")
            .Validate(
                options => options.HistoryCacheDurationMinutes >= 0,
                "Yahoo history cache duration cannot be negative.")
            .ValidateOnStart();

        services.AddSingleton<YahooQuotes>(serviceProvider =>
        {
            var options =
                serviceProvider
                    .GetRequiredService<
                        IOptions<YahooOptions>>()
                    .Value;

            var loggerFactory =
                serviceProvider
                    .GetRequiredService<ILoggerFactory>();

            return new YahooQuotesBuilder()
                .WithLoggerFactory(loggerFactory)
                .WithHttpUserAgent(options.UserAgent)
                .WithHistoryCacheDuration(
                    Duration.FromMinutes(
                        options.HistoryCacheDurationMinutes))
                .Build();
        });

        services.AddScoped<IPriceDataProvider,
            YahooPriceDataProvider>();

        services.AddHttpClient(
            HttpClientNames.Binance,
            (serviceProvider, client) =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<
                            Microsoft.Extensions.Options
                                .IOptions<BinanceOptions>>()
                        .Value;

                client.BaseAddress =
                    new Uri(options.BaseUrl);

                client.Timeout =
                    TimeSpan.FromSeconds(
                        options.TimeoutSeconds);

                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "TradingBot/1.0");
            });

        services.AddScoped<IPriceDataProvider,
            BinancePriceDataProvider>();

        services.AddSingleton<IYahooHistoryClient,
            YahooHistoryClient>();

        services.AddScoped<IChartGenerator, ScottPlotChartGenerator>();

        services.AddSingleton<ChartConfiguration>();

        services.AddSingleton<IChartConfiguration>(
            provider => provider.GetRequiredService<ChartConfiguration>());

        services.AddSingleton<IChartDimensions>(
            provider => provider.GetRequiredService<ChartConfiguration>());

        services
            .AddOptions<TelegramOptions>()
            .Bind(configuration.GetSection(
                TelegramOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.BotToken),
                "Telegram BotToken must not be empty.")
            .Validate(
                options => options.TimeoutSeconds > 0,
                "Telegram timeout must be greater than zero.")
            .ValidateOnStart();

        services.AddScoped<ITelegramSender, TelegramSender>();

        services.AddScoped<IPriceDataProviderResolver,
            PriceDataProviderResolver>();

        services.AddScoped<ISymbolResolver,
            SymbolResolver>();

        services.AddScoped<ISmaCalculator,
            SmaCalculator>();

        services.AddScoped<IChartService,
            ChartService>();

        services.AddScoped<ICommandParser,
            CommandParser>();

        services.AddScoped<ITelegramBotHandler,
            TelegramBotHandler>();

        return services;
    }
}
