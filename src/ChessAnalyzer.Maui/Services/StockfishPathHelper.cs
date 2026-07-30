namespace ChessAnalyzer.Maui.Services;

public static class StockfishPathHelper
{
    private static string? _cachedPath;

    public static string GetPath()
    {
        if (_cachedPath is not null)
            return _cachedPath;

        if (OperatingSystem.IsAndroid())
            _cachedPath = EnsureAndroidStockfish();
        else if (OperatingSystem.IsWindows())
            _cachedPath = Path.Combine(AppContext.BaseDirectory, "engines", "stockfish.exe");
        else
            _cachedPath = Path.Combine(AppContext.BaseDirectory, "engines", "stockfish");

        return _cachedPath;
    }

    private static string EnsureAndroidStockfish()
    {
        var dest = Path.Combine(FileSystem.CacheDirectory, "stockfish");
        if (File.Exists(dest))
        {
            EnsureExecutable(dest);
            return dest;
        }

        return Task.Run(async () =>
        {
            await using var input = await FileSystem.OpenAppPackageFileAsync("engines/stockfish");
            await using var output = File.Create(dest);
            await input.CopyToAsync(output);
            EnsureExecutable(dest);
            return dest;
        }).GetAwaiter().GetResult();
    }

    private static void EnsureExecutable(string path)
    {
#if ANDROID
        var file = new Java.IO.File(path);
        file.SetReadable(true, false);
        file.SetExecutable(true, false);
        file.SetWritable(true, false);
#endif
    }
}
