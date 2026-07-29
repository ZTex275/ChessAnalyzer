using ChessAnalyzer.Core.Analysis;
using ChessAnalyzer.Core.ChessCom;
using ChessAnalyzer.Core.Engine;
using ChessAnalyzer.Maui.Services;
using ChessAnalyzer.Maui.Views;
using ChessAnalyzer.Stockfish;
using Microsoft.Extensions.Logging;

namespace ChessAnalyzer.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddScoped(_ => new HttpClient());
        builder.Services.AddScoped<ChessComClient>();
        builder.Services.AddScoped<IStockfishEngine>(_ => new ProcessStockfishEngine(StockfishPathHelper.GetPath()));
        builder.Services.AddScoped<GameAnalyzer>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
