# IdleCheck

Simple Windows tray tool that locks your PC after a period of inactivity. (lock like WIN+L)

## Features
- Idle detection via Windows API
- Tray icon with visual progress indicator
- Configurable timeout (1–10 minutes)

## How to run
- Build with Visual Studio
- Or download from Releases

## Notes
Uses WinAPI (`GetLastInputInfo`, `LockWorkStation`)

## Download

You can download the prebuilt executable from the [Releases page](https://github.com/SeraByte/IdleCheck/releases).
Just unzip and double-click IdleCheck.exe to run.
