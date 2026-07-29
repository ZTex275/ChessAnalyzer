using ChessAnalyzer.Core.Analysis;
using ChessDotNetCore;

namespace ChessAnalyzer.Core.Chess;

public sealed class BoardState
{
    private readonly List<string> _mainFens =
    [
        StartFen
    ];

    private readonly List<(string San, string Uci, bool IsWhite)> _mainMoves = [];
    private readonly List<string> _varFens = [];
    private readonly List<(string San, string Uci, bool IsWhite)> _varMoves = [];
    private int _branchPly = -1;
    private int _ply;

    public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public string CurrentFen => GetFenAt(_ply);
    public int CurrentPly => _ply;
    public int MainMoveCount => _mainMoves.Count;
    public int MoveCount => IsInVariation && _ply > _branchPly
        ? _branchPly + _varMoves.Count
        : _mainMoves.Count;
    public bool IsInVariation => _branchPly >= 0;
    public int BranchPly => _branchPly;
    public bool IsOnMainLine => !IsInVariation || _ply <= _branchPly;

    public bool CanStepBack => _ply > 0;

    public bool CanStepForward =>
        IsInVariation && _ply > _branchPly
            ? _ply < _branchPly + _varMoves.Count
            : _ply < _mainMoves.Count;

    public IReadOnlyList<(string San, string Uci, bool IsWhite)> Moves => GetCurrentMoves();

    public void Reset()
    {
        _mainFens.Clear();
        _mainFens.Add(StartFen);
        _mainMoves.Clear();
        ClearVariation();
        _ply = 0;
    }

    public void LoadFromPgn(string pgn)
    {
        Reset();
        foreach (var (san, uci, _, fenAfter, isWhite) in PgnGameLoader.LoadMoves(pgn))
        {
            _mainMoves.Add((san, uci, isWhite));
            _mainFens.Add(fenAfter);
        }
    }

    public string GetFenAt(int ply)
    {
        if (ply < 0)
            throw new ArgumentOutOfRangeException(nameof(ply));

        if (!IsInVariation || ply <= _branchPly)
            return _mainFens[ply];

        var varIndex = ply - _branchPly;
        return _varFens[varIndex];
    }

    public (string San, string Uci, bool IsWhite)? GetMoveAt(int moveIndex)
    {
        if (moveIndex < 0)
            return null;

        if (!IsInVariation || moveIndex < _branchPly)
        {
            if (moveIndex >= _mainMoves.Count)
                return null;

            return _mainMoves[moveIndex];
        }

        var varIndex = moveIndex - _branchPly;
        if (varIndex >= _varMoves.Count)
            return null;

        return _varMoves[varIndex];
    }

    public bool TryMakeMove(string fromSquare, string toSquare, char? promotion = null)
    {
        if (!IsInVariation && _ply < _mainMoves.Count)
        {
            var game = new ChessGame(CurrentFen);
            var move = FindMove(game, fromSquare, toSquare, promotion)
                ?? FindMove(game, fromSquare, toSquare, 'q');
            if (move is null)
                return false;

            var uci = ToUci(move);
            var mainMove = _mainMoves[_ply];
            if (string.Equals(uci, mainMove.Uci, StringComparison.OrdinalIgnoreCase))
            {
                _ply++;
                return true;
            }

            return TryStartVariation(fromSquare, toSquare, promotion);
        }

        if (IsInVariation && _ply > _branchPly)
            TruncateVariationAfterCurrentPly();

        return TryStartVariation(fromSquare, toSquare, promotion);
    }

    public void StepBack()
    {
        if (_ply <= 0)
            return;

        _ply--;

        if (IsInVariation && _ply < _branchPly)
            ClearVariation();
    }

    public void StepForward()
    {
        if (!CanStepForward)
            return;

        if (IsInVariation && _ply == _branchPly)
            ClearVariation();

        _ply++;
    }

    public void GoToPly(int ply)
    {
        ClearVariation();
        _ply = Math.Clamp(ply, 0, _mainMoves.Count);
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

    private bool TryStartVariation(string fromSquare, string toSquare, char? promotion)
    {
        var game = new ChessGame(CurrentFen);
        var move = FindMove(game, fromSquare, toSquare, promotion)
            ?? FindMove(game, fromSquare, toSquare, 'q');
        if (move is null)
            return false;

        if (!IsInVariation || _ply <= _branchPly)
        {
            _branchPly = _ply;
            _varFens.Clear();
            _varMoves.Clear();
            _varFens.Add(_mainFens[_branchPly]);
        }
        else
        {
            TruncateVariationAfterCurrentPly();
        }

        var isWhite = game.CurrentPlayer == Player.White;
        game.MakeMove(move, true);
        var san = game.AllMoves.Last().SAN;
        var uci = ToUci(move);

        _varMoves.Add((san, uci, isWhite));
        _varFens.Add(game.GetFen());
        _ply = _branchPly + _varMoves.Count;
        return true;
    }

    private IReadOnlyList<(string San, string Uci, bool IsWhite)> GetCurrentMoves()
    {
        if (!IsInVariation || _ply <= _branchPly)
            return _mainMoves;

        var combined = new List<(string San, string Uci, bool IsWhite)>(_branchPly + _varMoves.Count);
        for (var i = 0; i < _branchPly; i++)
            combined.Add(_mainMoves[i]);

        combined.AddRange(_varMoves);
        return combined;
    }

    private void ClearVariation()
    {
        _branchPly = -1;
        _varFens.Clear();
        _varMoves.Clear();
    }

    private void TruncateVariationAfterCurrentPly()
    {
        while (_branchPly + _varMoves.Count > _ply)
        {
            _varMoves.RemoveAt(_varMoves.Count - 1);
            _varFens.RemoveAt(_varFens.Count - 1);
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
