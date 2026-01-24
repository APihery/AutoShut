@echo off
echo ========================================
echo Building AutoShut for Windows
echo ========================================
echo.

REM Navigate to the solution directory
cd /d "%~dp0"

REM Restore NuGet packages
echo [1/4] Restoring NuGet packages...
dotnet restore AutoShut\AutoShut.csproj
if %errorlevel% neq 0 (
    echo ERROR: Restore failed!
    pause
    exit /b %errorlevel%
)
echo.

REM Clean previous builds
echo [2/4] Cleaning previous builds...
dotnet clean AutoShut\AutoShut.csproj --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: Clean failed!
    pause
    exit /b %errorlevel%
)
echo.

REM Publish the application
echo [3/4] Publishing application...
dotnet publish AutoShut\AutoShut.csproj --configuration Release --framework net10.0-windows10.0.19041.0 --output "Build\Windows"
if %errorlevel% neq 0 (
    echo ERROR: Publish failed!
    pause
    exit /b %errorlevel%
)
echo.

REM Success message
echo [4/4] Build completed successfully!
echo.
echo ========================================
echo Build output location: Build\Windows
echo ========================================
echo.
echo Press any key to open the build folder...
pause > nul
explorer "Build\Windows"
