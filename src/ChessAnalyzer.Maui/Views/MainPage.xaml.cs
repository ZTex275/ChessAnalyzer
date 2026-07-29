namespace ChessAnalyzer.Maui.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        Loader.IsRunning = true;
        Loader.IsVisible = true;
    }

    private void OnNavigated(object? sender, WebNavigatedEventArgs e)
    {
        Loader.IsRunning = false;
        Loader.IsVisible = false;

        if (e.Result != WebNavigationResult.Success)
        {
            DisplayAlert(
                "Ошибка загрузки",
                "Не удалось открыть приложение. Проверьте интернет и попробуйте снова.",
                "OK");
        }
    }
}
