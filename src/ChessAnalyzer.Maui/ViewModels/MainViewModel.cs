using ChessAnalyzer.Core.ChessCom;
using ChessAnalyzer.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChessAnalyzer.Maui.ViewModels;

public partial class MainViewModel(ChessComClient chessCom) : ObservableObject
{
    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _status = "Введите логин Chess.com";

    [ObservableProperty]
    private List<ChessComGameSummary> _games = [];

    [RelayCommand]
    private async Task LoadGamesAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            Status = "Укажите логин";
            return;
        }

        try
        {
            IsLoading = true;
            Status = "Загрузка партий...";
            Games = (await chessCom.GetRecentGamesAsync(Username.Trim(), 15)).ToList();
            Status = Games.Count == 0 ? "Партии не найдены" : $"Найдено партий: {Games.Count}";
        }
        catch (Exception ex)
        {
            Status = $"Ошибка: {ex.Message}";
            Games = [];
        }
        finally
        {
            IsLoading = false;
        }
    }
}
