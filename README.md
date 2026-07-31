# Chess Analyzer

Кроссплатформенный анализатор партий Chess.com со Stockfish.

## Платформы

| Платформа | Проект | Движок |
|-----------|--------|--------|
| Android / Windows | `ChessAnalyzer.Maui` | нативный Stockfish |
| Браузер | `ChessAnalyzer.Web` (Blazor WASM) | Stockfish.js в браузере |
| API (опционально) | `ChessAnalyzer.Server` | нативный Stockfish |

Общий UI — `ChessAnalyzer.Shared` (Blazor). Логика — `ChessAnalyzer.Core`.

## Возможности

- Загрузка последних партий с Chess.com по username
- Анализ ходов через Stockfish с классификацией (лучший, отличный, неточность, ошибка, зевок)
- Лучший ход, стрелка на доске, потеря в centipawns, точность белых/чёрных
- Интерактивная доска: клик и **перетаскивание** фигур, вариации от основной линии
- Фоновая разметка партии с **приоритетом текущего хода** на доске
- Кеш оценок FEN между ходами
- Иконка приложения — кремовый конь на тёмно-зелёном (Android / Windows / Web / splash)

## Требования

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- workload `maui` (для Android/Windows): `dotnet workload install maui`
- Android SDK (API 35+) и устройство/эмулятор с **ARM64**
- Бинарники Stockfish (см. ниже)

## Stockfish

Скачайте с https://stockfishchess.org/download/

| Цель | Куда положить |
|------|----------------|
| Windows (MAUI) | `src/ChessAnalyzer.Maui/engines/stockfish.exe` |
| Android (MAUI) | `src/ChessAnalyzer.Maui/engines/stockfish` (stockfish-android-armv8) |
| Server / Linux | `stockfish` в `PATH` |
| Web | CI кладёт `stockfish.js` в `wwwroot/js/` |

На Android бинарник **нельзя** запускать из cache (W^X). При сборке он копируется в `Platforms/Android/lib/arm64-v8a/libstockfish.so` и стартует из `nativeLibraryDir`.

Лицензия Stockfish: GPL v3 — учитывайте при публикации.

## Сборка

```bash
dotnet restore
dotnet build ChessAnalyzer.sln
```

### Cursor / VS Code — F5

В Run and Debug выбери конфигурацию и нажми **F5**:

| Конфигурация | Что делает |
|--------------|------------|
| **Build All Platforms** | Собирает solution + MAUI Windows/Android + Web + Server, затем стартует Server |
| **All: Web + Server** | Полная сборка, потом Web и Server вместе |
| **Web** / **Server** | Только выбранный проект |
| **MAUI Windows** | Сборка и запуск Windows-приложения |
| **Android → Phone** | Сборка APK и установка на USB-телефон |

`Ctrl+Shift+B` — та же задача `build-all` (сборка всех платформ без запуска).

### MAUI Windows

```bash
dotnet build src/ChessAnalyzer.Maui -f net9.0-windows10.0.19041.0
```

### MAUI Android (Debug APK на телефон)

```bash
dotnet build src/ChessAnalyzer.Maui -f net9.0-android -c Debug \
  -p:EmbedAssembliesIntoApk=true -p:AndroidUseSharedRuntime=false

adb install -r src/ChessAnalyzer.Maui/bin/Debug/net9.0-android/com.chessanalyzer.app-Signed.apk
```

Release / CI: `build-android-apk.sh` или workflow `.github/workflows/android-apk.yml`.

### Web

Движок в браузере — отдельный Server **не обязателен**:

```bash
dotnet run --project src/ChessAnalyzer.Web
```

Публикация на GitHub Pages: workflow `.github/workflows/github-pages.yml`  
(база сайта: `/ChessAnalyzer/`).

Опционально серверный анализ:

```bash
dotnet run --project src/ChessAnalyzer.Server
```

## Версии

Текущая версия приложения задаётся в `ChessAnalyzer.Maui.csproj`  
(`ApplicationDisplayVersion` / `ApplicationVersion`). Релизы помечаются тегами `vX.Y.Z`.
