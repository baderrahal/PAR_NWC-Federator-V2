@echo off
rem Installs Parsons NWC Federator for this Windows user from the folder this file sits in, a
rem team zip extracted. It starts Windows PowerShell 5.1 for this one run with the script policy
rem bypassed, needs no admin rights, and runs team-install.ps1 beside it, which needs
rem install-common.ps1 beside it too. Put back the previous version.cmd calls this file with
rem -PutBack, so the way PowerShell is started is written once.
setlocal
set "here=%~dp0"
set "whole=yes"
if not exist "%here%team-install.ps1" set "whole="
if not exist "%here%install-common.ps1" set "whole="
if not defined whole (
    echo REFUSED: this file was started from inside the zip, or the zip was not extracted whole. Right click the zip, choose Extract All, and double click this file in the folder that makes. Nothing was changed.
    echo.
    pause
    exit /b 2
)
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%here%team-install.ps1" %*
set "code=%errorlevel%"
echo.
pause
exit /b %code%
