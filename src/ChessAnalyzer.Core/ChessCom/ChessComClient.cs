using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChessAnalyzer.Core.Models;

namespace ChessAnalyzer.Core.ChessCom;

public sealed class ChessComClient(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<ChessComGameSummary>> GetRecentGamesAsync(
        string username,
        int maxGames = 20,
        CancellationToken ct = default)
    {
        var archivesUrl = $"https://api.chess.com/pub/player/{username.ToLowerInvariant()}/games/archives";
        var archivesJson = await http.GetStringAsync(archivesUrl, ct);
        var archives = JsonSerializer.Deserialize<ArchivesResponse>(archivesJson, JsonOptions)
            ?? throw new InvalidOperationException("Не удалось получить архивы Chess.com");

        var games = new List<ChessComGameSummary>();

        foreach (var archiveUrl in archives.Archives.TakeLast(3).Reverse())
        {
            if (games.Count >= maxGames)
                break;

            var monthJson = await http.GetStringAsync(archiveUrl, ct);
            var month = JsonSerializer.Deserialize<MonthGamesResponse>(monthJson, JsonOptions);
            if (month?.Games is null)
                continue;

            foreach (var g in month.Games.OrderByDescending(x => x.EndTime).Take(maxGames - games.Count))
            {
                games.Add(new ChessComGameSummary
                {
                    Url = g.Url,
                    White = g.White.Username,
                    Black = g.Black.Username,
                    Result = FormatResult(g.White.Result, g.Black.Result),
                    EndTime = DateTimeOffset.FromUnixTimeSeconds(g.EndTime).UtcDateTime,
                    TimeControl = g.TimeControl,
                    Pgn = g.Pgn
                });
            }
        }

        return games;
    }

    private static string FormatResult(string whiteResult, string blackResult) =>
        whiteResult switch
        {
            "win" => "1-0",
            "checkmated" or "resigned" or "timeout" or "abandoned" => "0-1",
            _ when blackResult == "win" => "0-1",
            _ => "1/2-1/2"
        };

    private sealed class ArchivesResponse
    {
        [JsonPropertyName("archives")]
        public List<string> Archives { get; set; } = [];
    }

    private sealed class MonthGamesResponse
    {
        [JsonPropertyName("games")]
        public List<GameEntry>? Games { get; set; }
    }

    private sealed class GameEntry
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = "";

        [JsonPropertyName("pgn")]
        public string Pgn { get; set; } = "";

        [JsonPropertyName("time_control")]
        public string TimeControl { get; set; } = "";

        [JsonPropertyName("end_time")]
        public long EndTime { get; set; }

        [JsonPropertyName("white")]
        public PlayerResult White { get; set; } = new();

        [JsonPropertyName("black")]
        public PlayerResult Black { get; set; } = new();
    }

    private sealed class PlayerResult
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = "";

        [JsonPropertyName("result")]
        public string Result { get; set; } = "";
    }
}
