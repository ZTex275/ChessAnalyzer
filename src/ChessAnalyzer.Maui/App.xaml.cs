using ChessAnalyzer.Maui.Views;

namespace ChessAnalyzer.Maui;

public partial class App : Application
{
    public App(Views.MainPage mainPage)
    {
        InitializeComponent();
        MainPage = mainPage;
    }
}
