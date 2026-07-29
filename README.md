Chess Analyzer — кроссплатформенный анализатор партий Chess.com

Структура решения

ChessAnalyzer.Core       — модели, Chess.com API, PGN, классификация ходов
ChessAnalyzer.Stockfish  — UCI-обёртка над бинарником Stockfish
ChessAnalyzer.Maui       — Android + Windows (.NET MAUI)
ChessAnalyzer.Web        — Blazor WebAssembly (фронтенд)
ChessAnalyzer.Server     — API для анализа на сервере (для Web)

Возможности

- Загрузка последних партий с Chess.com по username
- Анализ каждого хода через Stockfish
- Классификация ходов как на Chess.com: лучший, отличный, неточность, ошибка, зевок
- Показ лучшего хода и потери в centipawns
- Точность белых/чёрных (%)
- Кеш оценок FEN для ускорения повторных позиций

Быстродействие

- FastMode: depth 12–14 (настраивается)
- Multi-thread Stockfish (Threads = CPU-1)
- Hash 256 MB
- Кеш EvalCache между ходами одной партии

Требования

- .NET 9 SDK
- workload maui (для Android/Windows)
- Stockfish binary:
  - Windows: src/ChessAnalyzer.Maui/engines/stockfish.exe
  - Android: engines/stockfish (ARM64)
  - Linux/Server: stockfish в PATH

Сборка

  cd /root/ChessAnalyzer
  dotnet restore
  dotnet build

MAUI Windows:
  dotnet build src/ChessAnalyzer.Maui -f net9.0-windows10.0.19041.0

MAUI Android:
  dotnet build src/ChessAnalyzer.Maui -f net9.0-android

Web (нужен запущенный Server):
  dotnet run --project src/ChessAnalyzer.Server
  dotnet run --project src/ChessAnalyzer.Web

Настройка Web-клиента

В WebAnalysisEngine запросы идут на api/analyze.
Запустите ChessAnalyzer.Server на том же хосте или пропишите BaseAddress в Program.cs Web-проекта.

Альтернатива для Web без сервера: подключить stockfish.wasm через JS Interop.

Stockfish

Скачайте официальный Stockfish с https://stockfishchess.org/download/
Положите бинарник в папку engines/ MAUI-проекта.

Лицензия Stockfish: GPL v3 — учитывайте при публикации приложения.
