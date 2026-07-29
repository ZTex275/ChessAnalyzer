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

    public static (int X, int Y) SquareCenter(string square, int squareSize, bool flip = false)
    {
        var file = square[0] - 'a';
        var rank = square[1] - '1';
        var col = flip ? 7 - file : file;
        var row = flip ? rank : 7 - rank;
        return (col * squareSize + squareSize / 2, row * squareSize + squareSize / 2);
    }

    public static IReadOnlyList<(char Piece, string Square)> ParseFen(string fen, bool flip)
    {
        var rows = fen.Split(' ')[0].Split('/');
        var squares = new List<(char, string)>(64);

        for (var rankIndex = 0; rankIndex < 8; rankIndex++)
        {
            var fenRank = flip ? 7 - rankIndex : rankIndex;
            var nameRank = flip ? rankIndex : 7 - rankIndex;
            var fileIndex = 0;

            foreach (var ch in rows[fenRank])
            {
                if (char.IsDigit(ch))
                {
                    var empty = ch - '0';
                    for (var i = 0; i < empty; i++)
                    {
                        var file = flip ? 7 - fileIndex : fileIndex;
                        squares.Add(('.', SquareName(file, nameRank)));
                        fileIndex++;
                    }
                }
                else
                {
                    var file = flip ? 7 - fileIndex : fileIndex;
                    squares.Add((ch, SquareName(file, nameRank)));
                    fileIndex++;
                }
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

    private static string SquareName(int file, int rank) =>
        $"{(char)('a' + file)}{rank + 1}";
}
