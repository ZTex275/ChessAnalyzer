using ChessDotNetCore;
using ChessAnalyzer.Core.Analysis;
using ChessAnalyzer.Core.Engine;
using ChessAnalyzer.Core.Models;

namespace ChessAnalyzer.Core.Analysis;

public sealed class GameAnalyzer(IStockfishEngine engine)
{
    public async Task<GameAnalysisResult> AnalyzeAsync(
        ChessComGameSummary game,
        AnalysisOptions options,
        IProgress<(int current, int total, string san)>? progress = null,
        CancellationToken ct = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await engine.InitializeAsync(options, ct);

        var parsedMoves = PgnGameLoader.LoadMoves(game.Pgn);
        var results = new List<MoveAnalysis>(parsedMoves.Count);
        var cache = new EvalCache();

        for (var i = 0; i < parsedMoves.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var (san, uci, fenBefore, fenAfter, isWhite) = parsedMoves[i];

            progress?.Report((i + 1, parsedMoves.Count, san));

            var evalBefore = await GetEvalAsync(fenBefore, cache, ct);
            var evalAfter = await GetEvalAsync(fenAfter, cache, ct);

            var cpBefore = NormalizeCp(evalBefore, isWhite);
            var cpAfter = NormalizeCp(evalAfter, isWhite);
            var cpLoss = MoveClassifier.ComputeCentipawnLoss(cpBefore, cpAfter, isWhite);
            var playedBest = string.Equals(uci, evalBefore.BestMove, StringComparison.OrdinalIgnoreCase);

            var bestSan = TryUciToSan(fenBefore, evalBefore.BestMove) ?? evalBefore.BestMove;

            results.Add(new MoveAnalysis
            {
                PlyIndex = i,
                MoveNumber = (i / 2) + 1,
                San = san,
                Uci = uci,
                FenBefore = fenBefore,
                FenAfter = fenAfter,
                EvalBefore = evalBefore,
                EvalAfter = evalAfter,
                CentipawnLoss = cpLoss,
                Classification = MoveClassifier.Classify(cpLoss, playedBest, i < 8),
                BestMoveSan = bestSan,
                IsWhiteMove = isWhite
            });
        }

        sw.Stop();

        return new GameAnalysisResult
        {
            Game = game,
            Moves = results,
            AnalysisDuration = sw.Elapsed,
            WhiteAccuracy = MoveClassifier.ComputeAccuracy(results, true),
            BlackAccuracy = MoveClassifier.ComputeAccuracy(results, false)
        };
    }

    public async Task<MoveAnalysis> AnalyzeSingleMoveAsync(
        string san,
        string uci,
        string fenBefore,
        string fenAfter,
        bool isWhite,
        int plyIndex,
        AnalysisOptions options,
        EvalCache? cache = null,
        CancellationToken ct = default)
    {
        await engine.InitializeAsync(options, ct);
        cache ??= new EvalCache();

        var evalBefore = await GetEvalAsync(fenBefore, cache, ct);
        var evalAfter = await GetEvalAsync(fenAfter, cache, ct);

        var cpBefore = NormalizeCp(evalBefore, isWhite);
        var cpAfter = NormalizeCp(evalAfter, isWhite);
        var cpLoss = MoveClassifier.ComputeCentipawnLoss(cpBefore, cpAfter, isWhite);
        var playedBest = string.Equals(uci, evalBefore.BestMove, StringComparison.OrdinalIgnoreCase);
        var bestSan = TryUciToSan(fenBefore, evalBefore.BestMove) ?? evalBefore.BestMove;

        return new MoveAnalysis
        {
            PlyIndex = plyIndex,
            MoveNumber = (plyIndex / 2) + 1,
            San = san,
            Uci = uci,
            FenBefore = fenBefore,
            FenAfter = fenAfter,
            EvalBefore = evalBefore,
            EvalAfter = evalAfter,
            CentipawnLoss = cpLoss,
            Classification = MoveClassifier.Classify(cpLoss, playedBest, plyIndex < 8),
            BestMoveSan = bestSan,
            IsWhiteMove = isWhite
        };
    }

    public async Task<(EngineEvaluation Eval, string BestSan)> AnalyzePositionAsync(
        string fen,
        AnalysisOptions options,
        CancellationToken ct = default)
    {
        await engine.InitializeAsync(options, ct);
        var eval = await GetEvalAsync(fen, new EvalCache(), ct);
        var bestSan = TryUciToSan(fen, eval.BestMove) ?? eval.BestMove;
        return (eval, bestSan);
    }

    private static int NormalizeCp(EngineEvaluation eval, bool isWhiteMove)
    {
        if (eval.MateIn is not null)
            return eval.MateIn.StartsWith('-') ? -10000 : 10000;

        return isWhiteMove ? eval.Centipawns : -eval.Centipawns;
    }

    private static string? TryUciToSan(string fen, string uci)
    {
        try
        {
            if (uci.Length < 4)
                return null;

            var game = new ChessGame(fen);
            var from = uci[..2];
            var to = uci[2..4];
            char? promo = uci.Length > 4 ? uci[4] : null;

            foreach (var move in game.GetValidMoves(game.CurrentPlayer))
            {
                if (move.OriginalPosition.ToString() != from || move.NewPosition.ToString() != to)
                    continue;

                if (promo.HasValue && move.Promotion?.ToString()[0] != char.ToUpperInvariant(promo.Value))
                    continue;

                var clone = new ChessGame(fen);
                clone.MakeMove(move, true);
                return clone.AllMoves.Last().SAN;
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private async Task<EngineEvaluation> GetEvalAsync(string fen, EvalCache cache, CancellationToken ct)
    {
        if (cache.TryGet(fen, out var cached))
            return cached;

        var eval = await engine.AnalyzePositionAsync(fen, ct);
        cache.Set(fen, eval);
        return eval;
    }
}
