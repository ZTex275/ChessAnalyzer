using ChessAnalyzer.Core.Models;

namespace ChessAnalyzer.Core.Analysis;

public sealed class EvalCache
{
    private readonly Dictionary<string, EngineEvaluation> _cache = new(StringComparer.Ordinal);

    public bool TryGet(string fen, out EngineEvaluation eval) => _cache.TryGetValue(fen, out eval!);

    public void Set(string fen, EngineEvaluation eval) => _cache[fen] = eval;
}
