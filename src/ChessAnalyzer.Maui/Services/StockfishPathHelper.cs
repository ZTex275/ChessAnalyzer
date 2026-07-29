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

        using var input = FileSystem.OpenAppPackageFileAsync("engines/stockfish").GetAwaiter().GetResult();
        using var output = File.Create(dest);
        input.CopyTo(output);
        EnsureExecutable(dest);
        return dest;
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
