using ChessAnalyzer.Core.Models;
using ChessAnalyzer.Maui.ViewModels;

namespace ChessAnalyzer.Maui.Views;

public partial class AnalysisPage : ContentPage
{
    public AnalysisPage(AnalysisViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        Resources.Add("InvertedBoolConverter", new InvertedBoolConverter());
        Resources.Add("IsNotNullConverter", new IsNotNullConverter());
        Resources.Add("ClassificationConverter", new ClassificationNameConverter());
        Resources.Add("ClassificationColorConverter", new ClassificationColorConverter());
    }
}

internal sealed class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
        value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
        value is bool b && !b;
}

internal sealed class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
        value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
        throw new NotSupportedException();
}

internal sealed class ClassificationNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
        value is MoveClassification c ? c.ToDisplayName() : "";

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
        throw new NotSupportedException();
}

internal sealed class ClassificationColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
        value is MoveClassification c ? Color.FromArgb(c.ToColorHex()) : Colors.Gray;

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
        throw new NotSupportedException();
}
