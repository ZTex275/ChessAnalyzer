using ChessAnalyzer.Core.Engine;
using ChessAnalyzer.Core.Models;
using Microsoft.JSInterop;

namespace ChessAnalyzer.Web.Services;

public sealed class StockfishWasmEngine(IJSRuntime js) : IStockfishEngine
{
    private IJSObjectReference? _module;
    private AnalysisOptions _options = new();

    public async Task InitializeAsync(AnalysisOptions options, CancellationToken ct = default)
    {
        _options = options;
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/stockfish-engine.js");
        await _module.InvokeVoidAsync("initialize", new { hashMb = options.HashMb });
        await _module.InvokeVoidAsync("configure", new { hashMb = options.HashMb });
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
        if (_module is null)
            throw new InvalidOperationException("Stockfish WASM is not initialized");

        var depth = _options.FastMode ? Math.Min(_options.Depth, 14) : _options.Depth;
        var raw = await _module.InvokeAsync<List<JsEngineEvaluation>>("analyze", fen, depth, multiPv);

        return raw.Select(x => new EngineEvaluation
        {
            Centipawns = x.Centipawns,
            MateIn = x.MateIn,
            BestMove = x.BestMove,
            PvLine = x.PvLine,
            Depth = x.Depth
        }).ToList();
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("dispose");
            }
            catch
            {
                // page unload
            }

            await _module.DisposeAsync();
            _module = null;
        }
    }

    private sealed class JsEngineEvaluation
    {
        public int Centipawns { get; set; }
        public string? MateIn { get; set; }
        public string BestMove { get; set; } = "e2e4";
        public string PvLine { get; set; } = "e2e4";
        public int Depth { get; set; }
    }
}
