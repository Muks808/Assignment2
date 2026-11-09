@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Start-RaceDay.ps1"
if errorlevel 1 pause
