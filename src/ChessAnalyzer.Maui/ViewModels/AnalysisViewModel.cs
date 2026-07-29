using ChessAnalyzer.Core.Analysis;
using ChessAnalyzer.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChessAnalyzer.Maui.ViewModels;

public partial class AnalysisViewModel(GameAnalyzer analyzer) : ObservableObject
{
    [ObservableProperty]
    private ChessComGameSummary? _game;

    [ObservableProperty]
    private GameAnalysisResult? _result;

    [ObservableProperty]
    private MoveAnalysis? _selectedMove;

    [ObservableProperty]
    private bool _isAnalyzing;

    [ObservableProperty]
    private string _progressText = "";

    [ObservableProperty]
    private int _selectedMoveIndex;

    public void SetGame(ChessComGameSummary game)
    {
        Game = game;
        Result = null;
        SelectedMove = null;
        SelectedMoveIndex = 0;
    }

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        if (Game is null)
            return;

        try
        {
            IsAnalyzing = true;
            ProgressText = "Запуск Stockfish...";

            var options = new AnalysisOptions
            {
                Depth = 14,
                Threads = Math.Max(2, Environment.ProcessorCount - 1),
                HashMb = 256,
                FastMode = true
            };

            var progress = new Progress<(int current, int total, string san)>(p =>
            {
                ProgressText = $"Анализ {p.current}/{p.total}: {p.san}";
            });

            Result = await analyzer.AnalyzeAsync(Game, options, progress);
            SelectedMoveIndex = 0;
            SelectedMove = Result.Moves.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ProgressText = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    partial void OnSelectedMoveIndexChanged(int value)
    {
        if (Result is null || value < 0 || value >= Result.Moves.Count)
            return;

        SelectedMove = Result.Moves[value];
    }

    [RelayCommand]
    private void NextMove()
    {
        if (Result is null)
            return;

        if (SelectedMoveIndex < Result.Moves.Count - 1)
            SelectedMoveIndex++;
    }

    [RelayCommand]
    private void PrevMove()
    {
        if (SelectedMoveIndex > 0)
            SelectedMoveIndex--;
    }
}
