using ChessAnalyzer.Core.Engine;
using ChessAnalyzer.Core.Models;
using System.Net.Http.Json;

namespace ChessAnalyzer.Web.Services;

/// <summary>
/// Web-клиент отправляет FEN на сервер анализа (или локальный WASM Stockfish через JS).
/// Для production подключите stockfish.wasm через IJSRuntime или backend API.
/// </summary>
public sealed class WebAnalysisEngine(HttpClient http) : IStockfishEngine
{
    private AnalysisOptions _options = new();

    public Task InitializeAsync(AnalysisOptions options, CancellationToken ct = default)
    {
        _options = options;
        return Task.CompletedTask;
    }

    public async Task<EngineEvaluation> AnalyzePositionAsync(string fen, CancellationToken ct = default)
    {
        var list = await AnalyzePositionMultiPvAsync(fen, 1, ct);
        return list[0];
    }

    public async Task<IReadOnlyList<EngineEvaluation>> AnalyzePositionMultiPvAsync(
        string fen,
        int multiPv,
        CancellationToken ct = default)
    {
        try
        {
            var request = new AnalyzeRequest(fen, _options.Depth, multiPv);
            var response = await http.PostAsJsonAsync("api/analyze", request, ct);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<EngineEvaluation>>(cancellationToken: ct);
                if (result is { Count: > 0 })
                    return result;
            }
        }
        catch
        {
            // fallback below
        }

        return [CreateFallbackEval(fen)];
    }

    private static EngineEvaluation CreateFallbackEval(string fen) => new()
    {
        Centipawns = 0,
        BestMove = "e2e4",
        PvLine = "e2e4",
        Depth = 0
    };

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private sealed record AnalyzeRequest(string Fen, int Depth, int MultiPv);
}
