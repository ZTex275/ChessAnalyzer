# Install Chess Analyzer Debug APK on a connected Android device.
$ErrorActionPreference = "Stop"

$apk = Join-Path $PSScriptRoot "..\src\ChessAnalyzer.Maui\bin\Debug\net9.0-android\com.chessanalyzer.app-Signed.apk"
$apk = [System.IO.Path]::GetFullPath($apk)

if (-not (Test-Path $apk)) {
    throw "APK not found: $apk. Build Android first."
}

$devices = adb devices | Select-String -Pattern "`tdevice$"
if (-not $devices) {
    throw "No Android device connected (adb devices)."
}

Write-Host "Installing $apk"
adb install --no-incremental $apk
if ($LASTEXITCODE -ne 0) {
    throw "adb install failed with exit code $LASTEXITCODE"
}

adb shell am force-stop com.chessanalyzer.app 2>$null
adb shell am start -n com.chessanalyzer.app/crc64fc9f27bbd600045e.MainActivity
Write-Host "Launched Chess Analyzer on device."
