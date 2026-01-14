#!/bin/bash

# Luban配置生成脚本 (Linux/Mac)
# 使用方法: chmod +x gen.sh && ./gen.sh

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LUBAN_DLL="$SCRIPT_DIR/Luban.dll"
CONF_ROOT="$SCRIPT_DIR/Configs"
OUTPUT_CODE_DIR="$SCRIPT_DIR/../Assets/Core/Scripts/Game/Config"
OUTPUT_DATA_DIR="$SCRIPT_DIR/../Assets/StreamingAssets/ConfigData"

# 检查Luban.dll是否存在
if [ ! -f "$LUBAN_DLL" ]; then
    echo "[ERROR] Luban.dll not found!"
    echo "Please download Luban from: https://github.com/focus-creative-games/luban/releases"
    exit 1
fi

# 创建输出目录
mkdir -p "$OUTPUT_CODE_DIR"
mkdir -p "$OUTPUT_DATA_DIR"

echo "================================================"
echo " Luban Configuration Generator"
echo "================================================"
echo ""
echo "Config Root: $CONF_ROOT"
echo "Code Output: $OUTPUT_CODE_DIR"
echo "Data Output: $OUTPUT_DATA_DIR"
echo ""

# 运行Luban生成
dotnet "$LUBAN_DLL" \
    -t all \
    -c cs-simple-json \
    -d json \
    --conf "$CONF_ROOT/luban.conf" \
    -x outputCodeDir="$OUTPUT_CODE_DIR" \
    -x outputDataDir="$OUTPUT_DATA_DIR"

if [ $? -ne 0 ]; then
    echo ""
    echo "[ERROR] Generation failed!"
    exit 1
fi

echo ""
echo "================================================"
echo " Generation completed successfully!"
echo "================================================"
echo ""