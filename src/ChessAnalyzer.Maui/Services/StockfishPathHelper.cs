namespace ChessAnalyzer.Maui.Services;

public static class StockfishPathHelper
{
    private static string? _cachedPath;

    public static string GetPath()
    {
        if (_cachedPath is not null)
            return _cachedPath;

        if (OperatingSystem.IsAndroid())
            _cachedPath = GetAndroidStockfishPath();
        else if (OperatingSystem.IsWindows())
            _cachedPath = Path.Combine(AppContext.BaseDirectory, "engines", "stockfish.exe");
        else
            _cachedPath = Path.Combine(AppContext.BaseDirectory, "engines", "stockfish");

        return _cachedPath;
    }

    private static string GetAndroidStockfishPath()
    {
#if ANDROID
        // Must run from nativeLibraryDir — Android Q+ blocks execute from cache/files (W^X).
        var nativeDir = Android.App.Application.Context.ApplicationInfo?.NativeLibraryDir
            ?? throw new InvalidOperationException("NativeLibraryDir недоступен");
        var path = Path.Combine(nativeDir, "libstockfish.so");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Stockfish не найден в native libs: {path}", path);
        return path;
#else
        throw new PlatformNotSupportedException();
#endif
    }
}
