using TradingBot.Application;
using TradingBot.Infrastructure;
using TradingBot.Presentation.Telegram;

var builder =
    WebApplication.CreateBuilder(args);

builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddHostedService<
    TelegramPollingService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet(
    "/",
    () => Results.Ok(new
    {
        status = "ok",
        service = "TradingBot"
    }));

app.Run();
