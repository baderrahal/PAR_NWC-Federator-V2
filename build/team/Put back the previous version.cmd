@echo off
rem Puts back the version of Parsons NWC Federator kept before the last install, through the
rem install file beside it, which starts PowerShell the one way it is written and checks for
rem the two scripts it needs.
setlocal
set "here=%~dp0"
if not exist "%here%Install Parsons NWC Federator.cmd" (
    echo REFUSED: this file was started from inside the zip, or the zip was not extracted whole. Right click the zip, choose Extract All, and double click this file in the folder that makes. Nothing was changed.
    echo.
    pause
    exit /b 2
)
call "%here%Install Parsons NWC Federator.cmd" -PutBack
exit /b %errorlevel%
