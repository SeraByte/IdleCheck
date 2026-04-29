# IdleCheck

Simple Windows tray tool that locks your PC after a period of inactivity.

## Features
- Idle detection via Windows API
- Tray icon with visual progress indicator
- Configurable timeout (1–10 minutes)

## How to run
- Build with Visual Studio
- Or download from Releases

## Notes
Uses WinAPI (`GetLastInputInfo`, `LockWorkStation`)