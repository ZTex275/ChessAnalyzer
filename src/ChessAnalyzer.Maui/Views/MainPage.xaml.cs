using ChessAnalyzer.Core.Models;
using ChessAnalyzer.Maui.ViewModels;

namespace ChessAnalyzer.Maui.Views;

public partial class MainPage : ContentPage
{
    private readonly AnalysisViewModel _analysisViewModel;

    public MainPage(MainViewModel vm, AnalysisViewModel analysisViewModel)
    {
        InitializeComponent();
        BindingContext = vm;
        _analysisViewModel = analysisViewModel;
    }

    private async void OnGameSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ChessComGameSummary game)
            return;

        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        _analysisViewModel.SetGame(game);
        await Navigation.PushAsync(new AnalysisPage(_analysisViewModel));
    }
}
