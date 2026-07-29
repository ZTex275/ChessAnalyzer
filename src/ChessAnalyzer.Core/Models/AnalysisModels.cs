namespace ChessAnalyzer.Core.Models;

public sealed class ChessComGameSummary
{
    public required string Url { get; init; }
    public required string White { get; init; }
    public required string Black { get; init; }
    public required string Result { get; init; }
    public required DateTime EndTime { get; init; }
    public required string TimeControl { get; init; }
    public required string Pgn { get; init; }
}

public sealed class EngineEvaluation
{
    public int Centipawns { get; init; }
    public string? MateIn { get; init; }
    public required string BestMove { get; init; }
    public required string PvLine { get; init; }
    public int Depth { get; init; }

    public double ToDisplayEval(bool whitePerspective)
    {
        if (MateIn is not null)
            return MateIn.StartsWith('-') ? -100 : 100;

        var cp = Centipawns / 100.0;
        return whitePerspective ? cp : -cp;
    }
}

public sealed class MoveAnalysis
{
    public int PlyIndex { get; init; }
    public int MoveNumber { get; init; }
    public required string San { get; init; }
    public required string Uci { get; init; }
    public required string FenBefore { get; init; }
    public required string FenAfter { get; init; }
    public required EngineEvaluation EvalBefore { get; init; }
    public required EngineEvaluation EvalAfter { get; init; }
    public required MoveClassification Classification { get; init; }
    public int CentipawnLoss { get; init; }
    public required string BestMoveSan { get; init; }
    public bool IsWhiteMove { get; init; }
}

public sealed class GameAnalysisResult
{
    public required ChessComGameSummary Game { get; init; }
    public required IReadOnlyList<MoveAnalysis> Moves { get; init; }
    public required TimeSpan AnalysisDuration { get; init; }
    public int WhiteAccuracy { get; init; }
    public int BlackAccuracy { get; init; }
}

public sealed class AnalysisOptions
{
    public int Depth { get; set; } = 14;
    public int Threads { get; set; } = Environment.ProcessorCount;
    public int HashMb { get; set; } = 128;
    public int MultiPv { get; set; } = 1;
    public bool FastMode { get; set; } = true;
}
