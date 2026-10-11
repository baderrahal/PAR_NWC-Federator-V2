@echo off
rem Installs Parsons NWC Federator for this Windows user from the folder this file sits in, a
rem team zip extracted. It starts Windows PowerShell 5.1 for this one run with the script policy
rem bypassed, needs no admin rights, and runs team-install.ps1 beside it. Put back the previous
rem version.cmd calls this file with -PutBack, so the way PowerShell is started is written once.
setlocal
set "here=%~dp0"
if not exist "%here%team-install.ps1" (
    echo REFUSED: this file was started from inside the zip. Right click the zip, choose Extract All, and double click this file in the folder that makes. Nothing was changed.
    echo.
    pause
    exit /b 2
)
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%here%team-install.ps1" %*
set "code=%errorlevel%"
echo.
pause
exit /b %code%
