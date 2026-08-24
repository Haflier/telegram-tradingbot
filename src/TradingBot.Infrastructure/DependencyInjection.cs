using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradingBot.Application.Abstractions;
using TradingBot.Infrastructure.Configuration;
using TradingBot.Infrastructure.Http;
using TradingBot.Infrastructure.Providers.Binance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingBot.Infrastructure.Charts;
using TradingBot.Infrastructure.Telegram;
using TradingBot.Application.Services;
using TradingBot.Infrastructure.Providers.TwelveData;

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
          .AddOptions<TwelveDataOptions>()
          .Bind(configuration.GetSection(
              TwelveDataOptions.SectionName))
          .Validate(
              options => !string.IsNullOrWhiteSpace(options.ApiKey),
              "Twelve Data ApiKey must not be empty.")
          .Validate(
              options =>
                  Uri.TryCreate(
                      options.BaseUrl,
                      UriKind.Absolute,
                      out _),
             "Twelve Data BaseUrl must be a valid absolute URI.")
          .Validate(
              options => options.TimeoutSeconds > 0,
             "Twelve Data timeout must be greater than zero.")
          .ValidateOnStart();

        services.AddHttpClient(
            "TwelveData",
            (serviceProvider, client) =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<
                            IOptions<TwelveDataOptions>>()
                        .Value;

                client.BaseAddress =
                    new Uri(
                        options.BaseUrl.TrimEnd('/') + "/");

                client.Timeout =
                    TimeSpan.FromSeconds(
                        options.TimeoutSeconds);

                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "TradingBot/1.0");
            });

        services.AddScoped<ITwelveDataClient>(
            serviceProvider =>
            {
                var factory =
                    serviceProvider
                        .GetRequiredService<IHttpClientFactory>();

                return new TwelveDataClient(
                    factory.CreateClient("TwelveData"),
                    serviceProvider.GetRequiredService<
                        IOptions<TwelveDataOptions>>(),
                    serviceProvider.GetRequiredService<
                       ILogger<TwelveDataClient>>());
            });

        services.AddScoped<IPriceDataProvider,
            TwelveDataPriceDataProvider>();

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
