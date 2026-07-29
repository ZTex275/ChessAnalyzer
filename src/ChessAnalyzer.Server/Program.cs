using ChessAnalyzer.Core.Engine;
using ChessAnalyzer.Core.Models;
using ChessAnalyzer.Stockfish;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddSingleton<IStockfishEngine, ProcessStockfishEngine>();

var app = builder.Build();
app.UseCors();

app.MapPost("/api/analyze", async (AnalyzeRequest req, IStockfishEngine engine, CancellationToken ct) =>
{
    await engine.InitializeAsync(new AnalysisOptions
    {
        Depth = req.Depth,
        FastMode = true,
        Threads = Environment.ProcessorCount,
        HashMb = 256
    }, ct);

    var result = await engine.AnalyzePositionMultiPvAsync(req.Fen, req.MultiPv, ct);
    return Results.Ok(result);
});

app.Run();

internal sealed record AnalyzeRequest(string Fen, int Depth, int MultiPv);
