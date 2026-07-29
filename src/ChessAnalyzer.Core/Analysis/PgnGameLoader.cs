using System.Text.RegularExpressions;
using ChessDotNetCore;

namespace ChessAnalyzer.Core.Analysis;

public static class PgnGameLoader
{
    public static IReadOnlyList<(string San, string Uci, string FenBefore, string FenAfter, bool IsWhite)> LoadMoves(string pgn)
    {
        var sanMoves = ExtractSanMoves(pgn);
        var game = new ChessGame();
        var moves = new List<(string, string, string, string, bool)>();

        foreach (var san in sanMoves)
        {
            var fenBefore = game.GetFen();
            var isWhite = game.WhoseMove == Player.White;
            var move = FindMoveBySan(game, san)
                ?? throw new InvalidOperationException($"Недопустимый ход в PGN: {san}");

            game.MakeMove(move, true);
            var fenAfter = game.GetFen();
            var uci = ToUci(move);
            moves.Add((san, uci, fenBefore, fenAfter, isWhite));
        }

        return moves;
    }

    private static Move? FindMoveBySan(ChessGame game, string san)
    {
        foreach (var move in game.GetValidMoves(false))
        {
            var clone = new ChessGame(game.GetFen());
            clone.MakeMove(move, true);
            if (string.Equals(clone.WhoseMove == Player.White
                    ? clone.GetMoveHistory().Last().SanForWhite
                    : clone.GetMoveHistory().Last().SanForBlack,
                san, StringComparison.Ordinal))
                return move;
        }

        foreach (var move in game.GetValidMoves(false))
        {
            var test = new ChessGame(game.GetFen());
            test.MakeMove(move, true);
            var last = test.GetMoveHistory().LastOrDefault();
            if (last is null)
                continue;

            var candidate = test.WhoseMove == Player.Black ? last.SanForWhite : last.SanForBlack;
            if (string.Equals(NormalizeSan(candidate), NormalizeSan(san), StringComparison.OrdinalIgnoreCase))
                return move;
        }

        return null;
    }

    private static string NormalizeSan(string san) =>
        san.Replace("+", "").Replace("#", "").Replace("x", "").Trim();

    private static string ToUci(Move move)
    {
        var uci = $"{move.Origin}{move.Destination}";
        if (move.Promotion != null)
            uci += char.ToLowerInvariant(move.Promotion.Value.ToString()[0]);
        return uci;
    }

    public static List<string> ExtractSanMoves(string pgn)
    {
        var body = Regex.Replace(pgn, @"\[[^\]]+\]", " ");
        body = Regex.Replace(body, @"\{[^}]*\}", " ");
        body = Regex.Replace(body, @"\([^)]*\)", " ");
        body = Regex.Replace(body, @"\d+\.\.\.", " ");
        body = Regex.Replace(body, @"\d+\.", " ");

        return body
            .Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t is not ("1-0" or "0-1" or "1/2-1/2" or "*"))
            .ToList();
    }
}
