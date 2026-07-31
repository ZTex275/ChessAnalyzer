using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using ChessAnalyzer.Core.Engine;
using ChessAnalyzer.Core.Models;

namespace ChessAnalyzer.Stockfish;

public sealed class ProcessStockfishEngine : IStockfishEngine
{
    private static readonly Regex InfoRegex = new(
        @"info depth (?<depth>\d+).*?score (?<type>cp|mate) (?<score>-?\d+).*? pv (?<pv>\S+(?: \S+)*)",
        RegexOptions.Compiled);

    private Process? _process;
    private StreamWriter? _input;
    private AnalysisOptions _options = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly string _executablePath;

    public ProcessStockfishEngine(string? executablePath = null)
    {
        _executablePath = executablePath ?? FindStockfish();
    }

    public async Task InitializeAsync(AnalysisOptions options, CancellationToken ct = default)
    {
        _options = options;

        if (_process is { HasExited: false })
        {
            await SendCommandAsync("setoption name Threads value " + options.Threads, ct);
            await SendCommandAsync("setoption name Hash value " + options.HashMb, ct);
            return;
        }

        if (!File.Exists(_executablePath))
            throw new FileNotFoundException($"Stockfish не найден: {_executablePath}", _executablePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = _executablePath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            // Do not redirect stderr without a reader — Stockfish NNUE/logs can fill the pipe and deadlock.
            RedirectStandardError = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(_executablePath) ?? AppContext.BaseDirectory
        };

        _process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        try
        {
            if (!_process.Start())
                throw new InvalidOperationException($"Не удалось запустить Stockfish: {_executablePath}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Не удалось запустить Stockfish ({_executablePath}): {ex.Message}", ex);
        }

        _input = _process.StandardInput;

        await SendCommandAsync("uci", ct);
        await WaitForAsync("uciok", ct);
        await SendCommandAsync("setoption name Threads value " + options.Threads, ct);
        await SendCommandAsync("setoption name Hash value " + options.HashMb, ct);
        await SendCommandAsync("isready", ct);
        await WaitForAsync("readyok", ct);
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
        await _lock.WaitAsync(ct);
        try
        {
            var depth = _options.FastMode ? Math.Min(_options.Depth, 12) : _options.Depth;
            await SendCommandAsync($"setoption name MultiPV value {multiPv}", ct);
            await SendCommandAsync($"position fen {fen}", ct);
            await SendCommandAsync($"go depth {depth}", ct);

            var evaluations = new Dictionary<int, EngineEvaluation>();
            var bestMove = await ReadAnalysisAsync(depth, multiPv, evaluations, ct);

            var result = new List<EngineEvaluation>();
            for (var i = 1; i <= multiPv; i++)
            {
                if (evaluations.TryGetValue(i, out var ev))
                    result.Add(ev);
            }

            if (result.Count == 0)
            {
                result.Add(new EngineEvaluation
                {
                    Centipawns = 0,
                    BestMove = bestMove ?? "0000",
                    PvLine = bestMove ?? "",
                    Depth = depth
                });
            }

            return result;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<string?> ReadAnalysisAsync(
        int targetDepth,
        int multiPv,
        Dictionary<int, EngineEvaluation> evaluations,
        CancellationToken ct)
    {
        string? bestMove = null;
        var output = _process!.StandardOutput;

        while (!ct.IsCancellationRequested)
        {
            var line = await output.ReadLineAsync(ct);
            if (line is null)
                break;

            if (line.StartsWith("bestmove "))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    bestMove = parts[1];
                break;
            }

            if (!line.Contains(" pv "))
                continue;

            var match = InfoRegex.Match(line);
            if (!match.Success)
                continue;

            var depth = int.Parse(match.Groups["depth"].Value);
            if (depth < targetDepth - 1)
                continue;

            var multipv = 1;
            var mpMatch = Regex.Match(line, @"multipv (\d+)");
            if (mpMatch.Success)
                multipv = int.Parse(mpMatch.Groups[1].Value);

            var scoreType = match.Groups["type"].Value;
            var scoreVal = int.Parse(match.Groups["score"].Value);
            var pv = match.Groups["pv"].Value;
            var pvMoves = pv.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            evaluations[multipv] = new EngineEvaluation
            {
                Depth = depth,
                Centipawns = scoreType == "cp" ? scoreVal : 0,
                MateIn = scoreType == "mate" ? scoreVal.ToString() : null,
                BestMove = pvMoves[0],
                PvLine = pv
            };
        }

        return bestMove;
    }

    private async Task SendCommandAsync(string command, CancellationToken ct)
    {
        if (_input is null)
            throw new InvalidOperationException("Stockfish не запущен");

        await _input.WriteLineAsync(command.AsMemory(), ct);
        await _input.FlushAsync(ct);
    }

    private async Task WaitForAsync(string token, CancellationToken ct)
    {
        var output = _process!.StandardOutput;
        while (!ct.IsCancellationRequested)
        {
            var line = await output.ReadLineAsync(ct);
            if (line is null)
                throw new InvalidOperationException("Stockfish завершился неожиданно");

            if (line.Contains(token, StringComparison.Ordinal))
                return;
        }
    }

    private static string FindStockfish()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(AppContext.BaseDirectory, "engines", "stockfish.exe");

        if (OperatingSystem.IsAndroid())
            return Path.Combine(AppContext.BaseDirectory, "engines", "stockfish");

        return "stockfish";
    }

    public async ValueTask DisposeAsync()
    {
        if (_process is { HasExited: false })
        {
            try
            {
                await SendCommandAsync("quit", CancellationToken.None);
            }
            catch
            {
                // ignore
            }

            _process.Kill(entireProcessTree: true);
        }

        _process?.Dispose();
        _lock.Dispose();
    }
}
