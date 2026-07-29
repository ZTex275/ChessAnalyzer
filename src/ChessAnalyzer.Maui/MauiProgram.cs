using ChessAnalyzer.Core.Analysis;
using ChessAnalyzer.Core.ChessCom;
using ChessAnalyzer.Core.Engine;
using ChessAnalyzer.Core.Models;
using ChessAnalyzer.Maui.ViewModels;
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
            .UseMauiApp<App>();

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<ChessComClient>();
        builder.Services.AddSingleton<IStockfishEngine, ProcessStockfishEngine>();
        builder.Services.AddSingleton<GameAnalyzer>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<AnalysisViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<AnalysisPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
