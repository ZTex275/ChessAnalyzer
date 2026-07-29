namespace ChessAnalyzer.Web.Components;

public static class ChessBoardHelper
{
    private static readonly Dictionary<char, string> PieceImages = new()
    {
        ['K'] = "pieces/wK.png", ['Q'] = "pieces/wQ.png", ['R'] = "pieces/wR.png",
        ['B'] = "pieces/wB.png", ['N'] = "pieces/wN.png", ['P'] = "pieces/wP.png",
        ['k'] = "pieces/bK.png", ['q'] = "pieces/bQ.png", ['r'] = "pieces/bR.png",
        ['b'] = "pieces/bB.png", ['n'] = "pieces/bN.png", ['p'] = "pieces/bP.png"
    };

    public static string PieceImage(char piece) =>
        PieceImages.TryGetValue(piece, out var path) ? path : "";

    public static bool IsLightSquare(string square)
    {
        var file = square[0] - 'a';
        var rank = square[1] - '0';
        return (file + rank) % 2 == 0;
    }

    public static (double X, double Y) SquareCenterNorm(string square, bool flip = false)
    {
        var file = square[0] - 'a';
        var rank = square[1] - '1';
        var col = flip ? 7 - file : file;
        var row = flip ? rank : 7 - rank;
        return (col + 0.5, row + 0.5);
    }

    public static char FileLabel(int col, bool flip) =>
        (char)('a' + (flip ? 7 - col : col));

    public static int RankLabel(int row, bool flip) =>
        flip ? row + 1 : 8 - row;

    public static IReadOnlyList<(char Piece, string Square)> ParseFen(string fen, bool flip)
    {
        var squares = new List<(char, string)>(64);

        for (var row = 0; row < 8; row++)
        {
            for (var col = 0; col < 8; col++)
            {
                var file = flip ? 7 - col : col;
                var rank = flip ? row : 7 - row;
                var square = SquareName(file, rank);
                var piece = GetPieceAt(fen, square);
                squares.Add((piece ?? '.', square));
            }
        }

        return squares;
    }

    public static (string From, string To)? ParseUci(string? uci)
    {
        if (string.IsNullOrWhiteSpace(uci) || uci.Length < 4)
            return null;

        return (uci[..2].ToLowerInvariant(), uci[2..4].ToLowerInvariant());
    }

    public static string PieceSymbol(char piece) =>
        PieceImages.ContainsKey(piece) ? piece.ToString() : "";

    private static char? GetPieceAt(string fen, string square)
    {
        var file = square[0] - 'a';
        var rank = square[1] - '1';
        var rows = fen.Split(' ')[0].Split('/');
        var fenRank = 7 - rank;
        var fileIndex = 0;

        foreach (var ch in rows[fenRank])
        {
            if (char.IsDigit(ch))
            {
                var empty = ch - '0';
                if (fileIndex + empty > file)
                    return null;

                fileIndex += empty;
            }
            else
            {
                if (fileIndex == file)
                    return ch;

                fileIndex++;
            }
        }

        return null;
    }

    private static string SquareName(int file, int rank) =>
        $"{(char)('a' + file)}{rank + 1}";
}
