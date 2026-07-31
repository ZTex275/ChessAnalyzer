using Android.App;
using Android.Content.PM;
using Android.OS;

namespace ChessAnalyzer.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    Icon = "@mipmap/appicon", RoundIcon = "@mipmap/appicon_round",
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
public class MainActivity : MauiAppCompatActivity
{
}
