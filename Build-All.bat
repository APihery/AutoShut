@echo off
echo ========================================
echo Build and Create Installer - AutoShut
echo ========================================
echo.

REM Navigate to the solution directory
cd /d "%~dp0"

REM Step 1: Restore NuGet packages
echo [STEP 1/5] Restoring NuGet packages...
dotnet restore AutoShut\AutoShut.csproj
if %errorlevel% neq 0 (
    echo ERROR: Restore failed!
    pause
    exit /b %errorlevel%
)
echo.

REM Step 2: Clean previous builds (Windows framework only to avoid runtime resolution)
echo [STEP 2/5] Cleaning previous builds...
dotnet clean AutoShut\AutoShut.csproj --configuration Release -f net10.0-windows10.0.19041.0
if %errorlevel% neq 0 (
    echo ERROR: Clean failed!
    pause
    exit /b %errorlevel%
)
echo.

REM Step 3: Publish the application
echo [STEP 3/5] Publishing application...
dotnet publish AutoShut\AutoShut.csproj --configuration Release --framework net10.0-windows10.0.19041.0 --output "Build\Windows"
if %errorlevel% neq 0 (
    echo ERROR: Publish failed!
    pause
    exit /b %errorlevel%
)
echo.

REM Step 4: Verify Inno Setup
echo [STEP 4/5] Verifying Inno Setup...
set INNO_SETUP="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not exist %INNO_SETUP% (
    echo ERROR: Inno Setup is not installed!
    echo.
    echo Download from: https://jrsoftware.org/isdl.php
    pause
    exit /b 1
)
echo OK: Inno Setup found
echo.

REM Step 5: Create installer
echo [STEP 5/5] Creating installer...
if not exist "Build\Installer" mkdir "Build\Installer"
%INNO_SETUP% "AutoShut-Setup.iss"
if %errorlevel% neq 0 (
    echo.
    echo ERROR: Installer creation failed!
    pause
    exit /b %errorlevel%
)
echo.

echo ========================================
echo ALL DONE!
echo ========================================
echo.
echo Build output: Build\Windows\AutoShut.exe
echo Installer: Build\Installer\AutoShut-Setup-v1.1.exe
echo.
echo Press any key to open the installer folder...
pause > nul
explorer "Build\Installer"
