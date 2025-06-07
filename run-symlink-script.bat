@echo off
echo GameFrame Symlink Setup - Admin Launcher
echo =======================================
echo This script will create a symbolic link from GameFrame framework to your game project.
echo.

:: Check for admin rights
NET SESSION >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: This script requires administrator privileges.
    echo Please right-click on this batch file and select "Run as administrator".
    echo.
    pause
    exit /b 1
)

echo Looking for GameFrame framework directory...

:: Define the framework path (current directory + client\Assets\GameFramework)
set "FRAMEWORK_PATH=%~dp0client\Assets\GameFramework"

:: Check if the framework directory exists
if not exist "%FRAMEWORK_PATH%" (
    echo ERROR: GameFramework directory not found at: %FRAMEWORK_PATH%
    echo Please make sure you are running this script from the GameFrame root directory.
    echo.
    pause
    exit /b 1
)

echo Found framework at: %FRAMEWORK_PATH%
echo.

:: Ask for game client directory
set "CLIENT_PATH="
set /p CLIENT_PATH=Enter your game client project directory (e.g. D:\Projects\MyGame): 

if "%CLIENT_PATH%"=="" (
    echo ERROR: You must provide a game client directory.
    pause
    exit /b 1
)

echo.
echo Searching for Assets directory in %CLIENT_PATH%...

:: Look for Assets directory
set "ASSETS_PATH="

:: Check if Assets exists directly in the client path
if exist "%CLIENT_PATH%\Assets" (
    set "ASSETS_PATH=%CLIENT_PATH%\Assets"
    goto :found_assets
)

:: Check if Assets exists in client subdirectory
if exist "%CLIENT_PATH%\client\Assets" (
    set "ASSETS_PATH=%CLIENT_PATH%\client\Assets"
    goto :found_assets
)

:: Look in subdirectories
for /d %%i in ("%CLIENT_PATH%\*") do (
    if exist "%%i\Assets" (
        set "ASSETS_PATH=%%i\Assets"
        goto :found_assets
    )
)

:: If we get here, we didn't find an Assets directory
echo ERROR: Could not find an Assets directory in %CLIENT_PATH%
echo Directory structure should be:
echo - %CLIENT_PATH%\Assets or
echo - %CLIENT_PATH%\client\Assets or
echo - %CLIENT_PATH%\subdirectory\Assets
echo.
pause
exit /b 1

:found_assets
echo Found Assets directory at: %ASSETS_PATH%
echo.

:: Define target path
set "TARGET_PATH=%ASSETS_PATH%\GameFramework"

:: Check if target already exists
if exist "%TARGET_PATH%" (
    echo GameFramework directory or link already exists. Removing...
    rmdir /s /q "%TARGET_PATH%" >nul 2>&1
    del /f /q "%TARGET_PATH%" >nul 2>&1
)

echo Creating symbolic link...
echo From: %FRAMEWORK_PATH%
echo To: %TARGET_PATH%
echo.

:: Create the symbolic link
mklink /d "%TARGET_PATH%" "%FRAMEWORK_PATH%"

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to create symbolic link.
    echo Please check paths and permissions.
    pause
    exit /b 1
)

echo.
echo Success! The symbolic link has been created.
echo When you modify the GameFramework, changes will automatically be reflected in your game project.
echo.
pause 