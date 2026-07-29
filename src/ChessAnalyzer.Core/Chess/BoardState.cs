using ChessAnalyzer.Core.Analysis;
using ChessDotNetCore;

namespace ChessAnalyzer.Core.Chess;

public sealed class BoardState
{
    private readonly List<string> _fens =
    [
        "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
    ];

    private readonly List<(string San, string Uci, bool IsWhite)> _moves = [];
    private int _ply;

    public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public string CurrentFen => _fens[_ply];
    public int CurrentPly => _ply;
    public int MoveCount => _moves.Count;
    public bool CanStepBack => _ply > 0;
    public bool CanStepForward => _ply < _moves.Count;
    public IReadOnlyList<(string San, string Uci, bool IsWhite)> Moves => _moves;

    public void Reset()
    {
        _fens.Clear();
        _fens.Add(StartFen);
        _moves.Clear();
        _ply = 0;
    }

    public void LoadFromPgn(string pgn)
    {
        Reset();
        foreach (var (san, uci, _, fenAfter, isWhite) in PgnGameLoader.LoadMoves(pgn))
        {
            _moves.Add((san, uci, isWhite));
            _fens.Add(fenAfter);
        }
    }

    public string GetFenAt(int ply) => _fens[ply];

    public (string San, string Uci, bool IsWhite)? GetMoveAt(int moveIndex)
    {
        if (moveIndex < 0 || moveIndex >= _moves.Count)
            return null;

        return _moves[moveIndex];
    }

    public bool TryMakeMove(string fromSquare, string toSquare, char? promotion = null)
    {
        TruncateAfterCurrentPly();

        var game = new ChessGame(CurrentFen);
        var move = FindMove(game, fromSquare, toSquare, promotion)
            ?? FindMove(game, fromSquare, toSquare, 'q');
        if (move is null)
            return false;

        var isWhite = game.CurrentPlayer == Player.White;
        game.MakeMove(move, true);
        var san = game.AllMoves.Last().SAN;
        var uci = ToUci(move);

        _moves.Add((san, uci, isWhite));
        _fens.Add(game.GetFen());
        _ply = _moves.Count;
        return true;
    }

    public void StepBack()
    {
        if (_ply > 0)
            _ply--;
    }

    public void StepForward()
    {
        if (_ply < _moves.Count)
            _ply++;
    }

    public void GoToPly(int ply)
    {
        _ply = Math.Clamp(ply, 0, _moves.Count);
    }

    public IReadOnlyList<string> GetLegalTargetSquares(string fromSquare)
    {
        try
        {
            var game = new ChessGame(CurrentFen);
            return game.GetValidMoves(game.CurrentPlayer)
                .Where(m => SquareEquals(m.OriginalPosition, fromSquare))
                .Select(m => m.NewPosition.ToString())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    public bool IsOwnPiece(string square)
    {
        try
        {
            var piece = GetPieceAtSquare(CurrentFen, square);
            if (piece is null)
                return false;

            var sideToMove = CurrentFen.Split(' ')[1];
            var isWhiteTurn = sideToMove == "w";
            var isWhitePiece = char.IsUpper(piece.Value);
            return isWhiteTurn == isWhitePiece;
        }
        catch
        {
            return false;
        }
    }

    private static char? GetPieceAtSquare(string fen, string square)
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

    private void TruncateAfterCurrentPly()
    {
        while (_moves.Count > _ply)
        {
            _moves.RemoveAt(_moves.Count - 1);
            _fens.RemoveAt(_fens.Count - 1);
        }
    }

    private static Move? FindMove(ChessGame game, string fromSquare, string toSquare, char? promotion)
    {
        foreach (var move in game.GetValidMoves(game.CurrentPlayer))
        {
            if (!string.Equals(move.OriginalPosition.ToString(), fromSquare, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(move.NewPosition.ToString(), toSquare, StringComparison.OrdinalIgnoreCase))
                continue;

            if (promotion.HasValue)
            {
                var promoChar = move.Promotion?.ToString()[0];
                if (promoChar != char.ToUpperInvariant(promotion.Value))
                    continue;
            }

            return move;
        }

        return null;
    }

    private static bool SquareEquals(Position pos, string square) =>
        string.Equals(pos.ToString(), square, StringComparison.OrdinalIgnoreCase);

    private static string ToUci(Move move)
    {
        var uci = $"{move.OriginalPosition}{move.NewPosition}";
        if (move.Promotion != null)
            uci += char.ToLowerInvariant(move.Promotion.Value);
        return uci;
    }
}
