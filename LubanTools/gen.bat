@echo off
setlocal

set WORKSPACE=.
set LUBAN_DLL=%WORKSPACE%\Luban\Luban.dll
set CONF_ROOT=%~dp0Configs
set OUTPUT_CODE_DIR=%~dp0..\Assets\Core\Scripts\Game\Config
set OUTPUT_DATA_DIR=%~dp0..\Assets\StreamingAssets\ConfigData

:: 检查Luban.dll是否存在
if not exist "%LUBAN_DLL%" (
    echo [ERROR] Luban.dll not found!
    echo Please download Luban from: https://github.com/focus-creative-games/luban/releases
    pause
    exit /b 1
)

:: 创建输出目录
if not exist "%OUTPUT_CODE_DIR%" mkdir "%OUTPUT_CODE_DIR%"
if not exist "%OUTPUT_DATA_DIR%" mkdir "%OUTPUT_DATA_DIR%"

echo ================================================
echo  Luban Configuration Generator
echo ================================================
echo.
echo Config Root: %CONF_ROOT%
echo Code Output: %OUTPUT_CODE_DIR%
echo Data Output: %OUTPUT_DATA_DIR%
echo.

:: 运行Luban生成
dotnet %LUBAN_DLL% ^
    -t all ^
    -c cs-simple-json ^
    -d json ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputCodeDir=%OUTPUT_CODE_DIR% ^
    -x outputDataDir=%OUTPUT_DATA_DIR%

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Generation failed!
    pause
    exit /b 1
)

echo.
echo ================================================
echo  Generation completed successfully!
echo ================================================
echo.
pause