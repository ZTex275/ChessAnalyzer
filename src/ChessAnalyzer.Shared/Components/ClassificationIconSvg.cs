using ChessAnalyzer.Core.Models;

namespace ChessAnalyzer.Shared.Components;

public static class ClassificationIconSvg
{
    private const string SvgOpen = """<svg viewBox="0 0 24 24" aria-hidden="true">""";
    private const string SvgClose = "</svg>";

    public static string Render(MoveClassification classification) => classification switch
    {
        MoveClassification.Brilliant => Text("!!", 14),
        MoveClassification.Best => Star,
        MoveClassification.Excellent => Text("!", 16),
        MoveClassification.Book => Book,
        MoveClassification.Inaccuracy => Text("?!", 12),
        MoveClassification.Mistake => Text("?", 16),
        MoveClassification.Blunder => Text("??", 12),
        MoveClassification.Miss => Miss,
        _ => ""
    };

    private static string Text(string value, double fontSize) =>
        $"""{SvgOpen}<text x="12" y="12.5" text-anchor="middle" dominant-baseline="middle" font-size="{fontSize.ToString(System.Globalization.CultureInfo.InvariantCulture)}" font-weight="800" fill="currentColor" font-family="Segoe UI, Roboto, Arial, sans-serif">{value}</text>{SvgClose}""";

    private const string Star =
        """<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 3.2 14.6 9.2l6.4.93-4.63 4.52 1.09 6.35L12 18.1l-5.72 3 1.09-6.35L2.74 10.13l6.4-.93L12 3.2z"/></svg>""";

    private const string Book =
        """<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M4 5.5h7c1.1 0 2 .6 2.5 1.5.5-.9 1.4-1.5 2.5-1.5H20V18.5H14.5c-1.1 0-2 .6-2.5 1.5-.5-.9-1.4-1.5-2.5-1.5H4V5.5zm2.5 2v8.5H11c.7 0 1.3.3 1.7.7V8.2c-.4-.4-1-.7-1.7-.7H6.5zm7.3 0c-.7 0-1.3.3-1.7.7v8.5c.4-.4 1-.7 1.7-.7H19V7.5h-5.2z"/></svg>""";

    private const string Miss =
        """<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M7.2 7.2 9 5.4 12 8.4l3-3 1.8 1.8L13.8 10.2l3 3-1.8 1.8L12 13.8l-3 3-1.8-1.8 3-3z"/></svg>""";
}
