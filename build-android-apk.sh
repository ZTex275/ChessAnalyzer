#!/bin/bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
cd "$ROOT"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
if [ -z "${JAVA_HOME:-}" ]; then
  if [ -d /usr/lib/jvm/java-17-openjdk-arm64 ]; then
    export JAVA_HOME=/usr/lib/jvm/java-17-openjdk-arm64
  else
    export JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64
  fi
fi
export ANDROID_HOME="${ANDROID_HOME:-/root/android-sdk}"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
export PATH="$JAVA_HOME/bin:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools:$PATH"

if [ ! -d "$ANDROID_HOME/platform-tools" ]; then
  echo "Android SDK not found at $ANDROID_HOME"
  exit 1
fi

if [ ! -f "$ROOT/android/chessanalyzer.keystore" ]; then
  echo "Keystore not found: $ROOT/android/chessanalyzer.keystore"
  exit 1
fi

VERSION=$(grep -m1 ApplicationDisplayVersion src/ChessAnalyzer.Maui/ChessAnalyzer.Maui.csproj | sed -E 's/.*>([^<]+)<.*/\1/' | tr -d ' ')
APK_NAME="ChessAnalyzer-${VERSION}.apk"

if ! /root/.dotnet/dotnet workload list 2>/dev/null | grep -q maui-android; then
  /root/.dotnet/dotnet workload install maui-android --skip-manifest-update
fi

if [ ! -x src/ChessAnalyzer.Maui/engines/stockfish ]; then
  mkdir -p src/ChessAnalyzer.Maui/engines
  curl -fsSL "https://github.com/official-stockfish/Stockfish/releases/download/sf_18/stockfish-android-armv8.tar" -o /tmp/stockfish.tar
  tar -xf /tmp/stockfish.tar -C /tmp
  cp /tmp/stockfish/stockfish-android-armv8 src/ChessAnalyzer.Maui/engines/stockfish
  chmod +x src/ChessAnalyzer.Maui/engines/stockfish
fi

/root/.dotnet/dotnet restore src/ChessAnalyzer.Maui/ChessAnalyzer.Maui.csproj -p:EnableWindowsTargeting=true

/root/.dotnet/dotnet publish src/ChessAnalyzer.Maui/ChessAnalyzer.Maui.csproj \
  -f net9.0-android \
  -c Release \
  -p:EnableWindowsTargeting=true \
  -p:TargetFrameworks=net9.0-android \
  -p:AndroidPackageFormat=apk \
  -p:AndroidSdkDirectory="$ANDROID_HOME" \
  -p:JavaSdkDirectory="$JAVA_HOME" \
  -p:AndroidKeyStore=true \
  -p:AndroidSigningKeyStore="$ROOT/android/chessanalyzer.keystore" \
  -p:AndroidSigningKeyAlias=chessanalyzer \
  -p:AndroidSigningStorePass=chessanalyzer \
  -p:AndroidSigningKeyPass=chessanalyzer

APK=$(find "$ROOT" -path '*/Release/*-Signed.apk' -type f | head -1)
if [ -z "$APK" ]; then
  echo "Signed APK not found"
  exit 1
fi

cp "$APK" "$ROOT/$APK_NAME"
echo "Built: $ROOT/$APK_NAME"
