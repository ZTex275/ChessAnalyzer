using ChessAnalyzer.Core.Models;

namespace ChessAnalyzer.Core.Analysis;

public static class MoveClassifier
{
    public static MoveClassification Classify(
        int centipawnLoss,
        bool playedBestMove,
        bool isBookMove = false)
    {
        if (isBookMove)
            return MoveClassification.Book;

        if (playedBestMove)
            return MoveClassification.Best;

        return centipawnLoss switch
        {
            <= 10 => MoveClassification.Excellent,
            <= 25 => MoveClassification.Good,
            <= 50 => MoveClassification.Inaccuracy,
            <= 100 => MoveClassification.Mistake,
            <= 300 => MoveClassification.Blunder,
            _ => MoveClassification.Blunder
        };
    }

    public static int ComputeCentipawnLoss(int evalBeforeCp, int evalAfterCp, bool isWhiteMove)
    {
        var swing = isWhiteMove
            ? evalBeforeCp - evalAfterCp
            : evalAfterCp - evalBeforeCp;

        return Math.Max(0, swing);
    }

    public static int ComputeAccuracy(IEnumerable<MoveAnalysis> moves, bool forWhite)
    {
        var relevant = moves.Where(m => m.IsWhiteMove == forWhite).ToList();
        if (relevant.Count == 0)
            return 100;

        var totalLoss = relevant.Sum(m => Math.Min(m.CentipawnLoss, 600));
        var avgLoss = totalLoss / (double)relevant.Count;
        var accuracy = 103.1668 * Math.Exp(-0.04354 * avgLoss) - 3.1669;
        return (int)Math.Clamp(Math.Round(accuracy), 0, 100);
    }
}
