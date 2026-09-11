@echo off
setlocal EnableExtensions
cd /d "%~dp0"

set "PROJECT=%~dp0backend\Quanto.Client.csproj"
for %%I in ("%~dp0..\build") do set "OUTDIR=%%~fI"
set "STAGE=%~dp0backend\bin\Release\net10.0-windows\win-x64\publish"

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

echo Publishing to staging folder...
if exist "%STAGE%" rmdir /s /q "%STAGE%"

dotnet publish "%PROJECT%" -c Release -p:PublishProfile=WinX64SingleFile
if errorlevel 1 (
  echo.
  echo ERROR: Publish failed.
  goto :fail
)

if not exist "%STAGE%\Quanto.Client.exe" (
  echo ERROR: Staging EXE not found:
  echo   %STAGE%\Quanto.Client.exe
  goto :fail
)

echo Cleaning %OUTDIR% ...
if not exist "%OUTDIR%" mkdir "%OUTDIR%"
for /d %%D in ("%OUTDIR%\*") do rmdir /s /q "%%D"
for %%F in ("%OUTDIR%\*") do (
  if /i not "%%~xF"==".log" del /q "%%F"
)

echo Copying only the files needed to run...
copy /y "%STAGE%\Quanto.Client.exe" "%OUTDIR%\" >nul
if exist "%STAGE%\App.config" copy /y "%STAGE%\App.config" "%OUTDIR%\" >nul
if exist "%STAGE%\Quanto.Client.dll.config" copy /y "%STAGE%\Quanto.Client.dll.config" "%OUTDIR%\" >nul
if exist "%STAGE%\Quanto.Client.exe.config" copy /y "%STAGE%\Quanto.Client.exe.config" "%OUTDIR%\" >nul
if exist "%OUTDIR%\App.config" (
  if not exist "%OUTDIR%\Quanto.Client.dll.config" copy /y "%OUTDIR%\App.config" "%OUTDIR%\Quanto.Client.dll.config" >nul
  if not exist "%OUTDIR%\Quanto.Client.exe.config" copy /y "%OUTDIR%\App.config" "%OUTDIR%\Quanto.Client.exe.config" >nul
)
if exist "%STAGE%\Quanto.Client.pdb" copy /y "%STAGE%\Quanto.Client.pdb" "%OUTDIR%\" >nul
if exist "%STAGE%\BI" xcopy /e /i /y "%STAGE%\BI" "%OUTDIR%\BI\" >nul
if exist "%STAGE%\Tally" xcopy /e /i /y "%STAGE%\Tally" "%OUTDIR%\Tally\" >nul

echo.
echo Done. Copy ONLY this folder (few files). Do NOT copy backend\bin.
echo   %OUTDIR%
echo.
echo Must be next to Quanto.Client.exe:
echo   App.config
echo   Quanto.Client.dll.config
echo   Quanto.Client.exe.config
echo Runtime DLLs are inside the EXE. Do not copy System.*.dll files.
echo.
dir /b "%OUTDIR%"
echo.
pause
exit /b 0

:fail
echo.
pause
exit /b 1
