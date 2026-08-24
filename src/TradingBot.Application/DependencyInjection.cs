using Microsoft.Extensions.DependencyInjection;
using TradingBot.Application.Abstractions;
using TradingBot.Application.Services;

namespace TradingBot.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ICommandParser, CommandParser>();

        services.AddScoped<ISymbolResolver, SymbolResolver>();

        services.AddScoped<ISmaCalculator, SmaCalculator>();

        services.AddScoped<IPriceDataProviderResolver,
            PriceDataProviderResolver>();

        services.AddScoped<IChartService, ChartService>();

        services.AddScoped<ITelegramBotHandler,
            TelegramBotHandler>();

        return services;
    }
}
