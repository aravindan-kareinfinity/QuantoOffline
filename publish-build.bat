@echo off
setlocal EnableExtensions
cd /d "%~dp0"

set "PROJECT=%~dp0backend\Quanto.Client.csproj"
for %%I in ("%~dp0..\build") do set "OUTDIR=%%~fI"

if not exist "%PROJECT%" (
  echo ERROR: Project not found:
  echo   %PROJECT%
  goto :fail
)

where dotnet >nul 2>&1
if errorlevel 1 (
  echo ERROR: .NET SDK not found on this PC.
  echo Install the .NET 10 SDK on the machine that creates the build.
  goto :fail
)

echo.
echo Publishing self-contained single-file Quanto.Client (win-x64)
echo Target PCs do not need .NET installed.
echo   Project : %PROJECT%
echo   Output  : %OUTDIR%
echo.

if exist "%OUTDIR%" (
  echo Cleaning previous build files...
  for %%D in (cs de es fr it ja ko pl pt-BR ru tr zh-Hans zh-Hant runtimes BI Tally) do (
    if exist "%OUTDIR%\%%D" rmdir /s /q "%OUTDIR%\%%D"
  )
  del /q "%OUTDIR%\*.dll" 2>nul
  del /q "%OUTDIR%\*.exe" 2>nul
  del /q "%OUTDIR%\*.pdb" 2>nul
  del /q "%OUTDIR%\*.json" 2>nul
  del /q "%OUTDIR%\*.config" 2>nul
  del /q "%OUTDIR%\*.xml" 2>nul
)

dotnet publish "%PROJECT%" ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:EnableCompressionInSingleFile=true ^
  -p:IncludeAllContentForSelfExtract=true ^
  -p:PublishDir="%OUTDIR%\\"

if errorlevel 1 (
  echo.
  echo ERROR: Publish failed.
  goto :fail
)

echo.
echo Done. Copy this folder to the other PC and run Quanto.Client.exe
echo   %OUTDIR%
echo.
pause
exit /b 0

:fail
echo.
pause
exit /b 1
