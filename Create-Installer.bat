@echo off
echo ========================================
echo Creating AutoShut Installer
echo ========================================
echo.

REM Check if Inno Setup is installed
set INNO_SETUP="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not exist %INNO_SETUP% (
    echo ERROR: Inno Setup is not installed!
    echo.
    echo Download from: https://jrsoftware.org/isdl.php
    echo Install it, then run this script again.
    echo.
    pause
    exit /b 1
)

REM Navigate to the solution directory
cd /d "%~dp0"

REM Check if the build exists
if not exist "Build\Windows\AutoShut.exe" (
    echo ERROR: Build does not exist!
    echo.
    echo Run 'Build-Windows.bat' first to build the application.
    echo.
    pause
    exit /b 1
)

REM Create the installer output folder
if not exist "Build\Installer" mkdir "Build\Installer"

REM Compile the installer with Inno Setup
echo Compiling installer...
echo.
%INNO_SETUP% "AutoShut-Setup.iss"

if %errorlevel% neq 0 (
    echo.
    echo ERROR: Installer compilation failed!
    pause
    exit /b %errorlevel%
)

echo.
echo ========================================
echo Installer created successfully!
echo ========================================
echo.
echo File: Build\Installer\AutoShut-Setup-v1.0.exe
echo.
echo Press any key to open the installer folder...
pause > nul
explorer "Build\Installer"
