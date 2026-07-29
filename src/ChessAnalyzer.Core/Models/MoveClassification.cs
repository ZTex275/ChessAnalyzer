namespace ChessAnalyzer.Core.Models;

public enum MoveClassification
{
    Best,
    Excellent,
    Good,
    Book,
    Inaccuracy,
    Mistake,
    Blunder,
    Miss,
    Brilliant
}

public static class MoveClassificationExtensions
{
    public static string ToDisplayName(this MoveClassification c) => c switch
    {
        MoveClassification.Best => "Лучший",
        MoveClassification.Excellent => "Отличный",
        MoveClassification.Good => "Хороший",
        MoveClassification.Book => "Дебют",
        MoveClassification.Inaccuracy => "Неточность",
        MoveClassification.Mistake => "Ошибка",
        MoveClassification.Blunder => "Зевок",
        MoveClassification.Miss => "Упущение",
        MoveClassification.Brilliant => "Блестящий",
        _ => c.ToString()
    };

    public static string ToColorHex(this MoveClassification c) => c switch
    {
        MoveClassification.Best => "#26C281",
        MoveClassification.Excellent => "#96BC4B",
        MoveClassification.Good => "#96BC4B",
        MoveClassification.Book => "#A88865",
        MoveClassification.Inaccuracy => "#F0C15D",
        MoveClassification.Mistake => "#E58F2A",
        MoveClassification.Blunder => "#CA3431",
        MoveClassification.Miss => "#CA3431",
        MoveClassification.Brilliant => "#1BAAA6",
        _ => "#888888"
    };

    public static string ToIcon(this MoveClassification c) => c switch
    {
        MoveClassification.Brilliant => "!!",
        MoveClassification.Best => "✓",
        MoveClassification.Excellent => "!",
        MoveClassification.Book => "▤",
        MoveClassification.Inaccuracy => "?!",
        MoveClassification.Mistake => "?",
        MoveClassification.Blunder => "??",
        MoveClassification.Miss => "✗",
        _ => ""
    };

    public static string ToCssClass(this MoveClassification c) => c switch
    {
        MoveClassification.Brilliant => "badge-brilliant",
        MoveClassification.Best => "badge-best",
        MoveClassification.Excellent => "badge-excellent",
        MoveClassification.Good => "badge-good",
        MoveClassification.Book => "badge-book",
        MoveClassification.Inaccuracy => "badge-inaccuracy",
        MoveClassification.Mistake => "badge-mistake",
        MoveClassification.Blunder => "badge-blunder",
        MoveClassification.Miss => "badge-miss",
        _ => "badge-good"
    };

    public static bool HasIcon(this MoveClassification c) =>
        c is not MoveClassification.Good;
}
