using ChessAnalyzer.Core.Models;

namespace ChessAnalyzer.Web.Components;

public static class ClassificationIconSvg
{
    private const string SvgOpen = """<svg viewBox="0 0 24 24" aria-hidden="true">""";
    private const string SvgClose = "</svg>";

    public static string Render(MoveClassification classification) => classification switch
    {
        MoveClassification.Brilliant => Text("!!", 10.5),
        MoveClassification.Best => Star,
        MoveClassification.Excellent => Text("!", 12),
        MoveClassification.Book => Book,
        MoveClassification.Inaccuracy => Text("?!", 9),
        MoveClassification.Mistake => Text("?", 12),
        MoveClassification.Blunder => Text("??", 9.5),
        MoveClassification.Miss => Miss,
        _ => ""
    };

    private static string Text(string value, double fontSize) =>
        $"""{SvgOpen}<text x="12" y="12.5" text-anchor="middle" dominant-baseline="middle" font-size="{fontSize.ToString(System.Globalization.CultureInfo.InvariantCulture)}" font-weight="800" fill="currentColor" font-family="Segoe UI, Roboto, Arial, sans-serif">{value}</text>{SvgClose}""";

    private const string Star =
        """<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 4.8 14.1 9.5l5.1.74-3.7 3.61.87 5.07L12 16.9l-4.57 2.4.87-5.07-3.7-3.61 5.1-.74L12 4.8z"/></svg>""";

    private const string Book =
        """<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M5 6.5h6.2c1 0 1.8.5 2.3 1.4.5-.9 1.3-1.4 2.3-1.4H19V17H14.5c-1 0-1.8.5-2.3 1.4-.5-.9-1.3-1.4-2.3-1.4H5V6.5zm2 1.8v7.4H11c.6 0 1.1.2 1.5.6V8.9c-.4-.4-.9-.6-1.5-.6H7zm6.8 0c-.6 0-1.1.2-1.5.6v7.4c.4-.4.9-.6 1.5-.6H18V8.3h-4.2z"/></svg>""";

    private const string Miss =
        """<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M8.1 8.1 9.5 6.7 12 9.2l2.5-2.5 1.4 1.4L13.4 10.6l2.5 2.5-1.4 1.4L12 12l-2.5 2.5-1.4-1.4 2.5-2.5z"/></svg>""";
}
