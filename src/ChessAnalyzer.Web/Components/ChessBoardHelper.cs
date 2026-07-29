namespace ChessAnalyzer.Web.Components;

public static class ChessBoardHelper
{
    private static readonly Dictionary<char, string> PieceSymbols = new()
    {
        ['K'] = "♔", ['Q'] = "♕", ['R'] = "♖", ['B'] = "♗", ['N'] = "♘", ['P'] = "♙",
        ['k'] = "♚", ['q'] = "♛", ['r'] = "♜", ['b'] = "♝", ['n'] = "♞", ['p'] = "♟"
    };

    public static IReadOnlyList<(char Piece, string Square)> ParseFen(string fen, bool flip)
    {
        var rows = fen.Split(' ')[0].Split('/');
        var squares = new List<(char, string)>(64);

        for (var rankIndex = 0; rankIndex < 8; rankIndex++)
        {
            var fenRank = flip ? rankIndex : 7 - rankIndex;
            var displayRank = flip ? rankIndex : 7 - rankIndex;
            var fileIndex = 0;

            foreach (var ch in rows[fenRank])
            {
                if (char.IsDigit(ch))
                {
                    var empty = ch - '0';
                    for (var i = 0; i < empty; i++)
                    {
                        var file = flip ? 7 - fileIndex : fileIndex;
                        squares.Add(('.', SquareName(file, displayRank)));
                        fileIndex++;
                    }
                }
                else
                {
                    var file = flip ? 7 - fileIndex : fileIndex;
                    squares.Add((ch, SquareName(file, displayRank)));
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
        PieceSymbols.TryGetValue(piece, out var symbol) ? symbol : "";

    private static string SquareName(int file, int rank) =>
        $"{(char)('a' + file)}{rank + 1}";
}
