@echo off
setlocal enabledelayedexpansion

echo =======================================================
echo        Endless Crowd Runner 3D - Android Build
echo =======================================================
echo.

set PROJECT_DIR=%~dp0
set UNITY_EXE=""

:: 1. Search for installed Unity versions in standard paths
for %%D in (
    "C:\Program Files\Unity\Hub\Editor"
    "D:\Program Files\Unity\Hub\Editor"
    "C:\Program Files\Unity\Editor"
    "D:\Program Files\Unity\Editor"
) do (
    if exist %%D (
        for /f "delims=" %%F in ('dir /b /s "%%~D\Unity.exe" 2^>nul') do (
            if exist "%%F" (
                set UNITY_EXE="%%F"
                goto FoundUnity
            )
        )
    )
)

:AskUnity
echo Unity Editor was not detected in standard Hub locations.
echo Please enter the full path to your Unity.exe (or install Unity 2022.3 LTS with Android Build Support):
set /p UNITY_INPUT="Unity.exe Path: "
set UNITY_EXE=%UNITY_INPUT%

if not exist %UNITY_EXE% (
    echo [ERROR] The path "%UNITY_EXE%" does not exist.
    pause
    exit /b 1
)

:FoundUnity
echo [INFO] Using Unity at: %UNITY_EXE%
echo [INFO] Building Android APK into %PROJECT_DIR%Builds\Android\EndlessCrowdRunner.apk ...
echo [INFO] This will take a few minutes. Check 'build.log' for detailed compilation progress.
echo.

if not exist "%PROJECT_DIR%Builds\Android" mkdir "%PROJECT_DIR%Builds\Android"

%UNITY_EXE% -quit -batchmode -nographics -projectPath "%PROJECT_DIR%" -executeMethod EndlessCrowdRunner.Editor.BuildPipelineHelper.BuildAndroidApk -logFile "%PROJECT_DIR%build.log"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =======================================================
    echo [SUCCESS] APK built successfully!
    echo Output: %PROJECT_DIR%Builds\Android\EndlessCrowdRunner.apk
    echo =======================================================
) else (
    echo.
    echo =======================================================
    echo [ERROR] Build failed with exit code %ERRORLEVEL%.
    echo Please inspect '%PROJECT_DIR%build.log' for compiler/Android SDK errors.
    echo =======================================================
)

pause
