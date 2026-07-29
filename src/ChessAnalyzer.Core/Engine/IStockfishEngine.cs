using ChessAnalyzer.Core.Models;

namespace ChessAnalyzer.Core.Engine;

public interface IStockfishEngine : IAsyncDisposable
{
    Task InitializeAsync(AnalysisOptions options, CancellationToken ct = default);
    Task<EngineEvaluation> AnalyzePositionAsync(string fen, CancellationToken ct = default);
    Task<IReadOnlyList<EngineEvaluation>> AnalyzePositionMultiPvAsync(string fen, int multiPv, CancellationToken ct = default);
}
