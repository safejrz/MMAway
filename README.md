# MMAway

MMAway is a small **Windows-only** desktop utility that simulates occasional mouse activity and prevents Windows from entering sleep while it is running. It is intended to help keep Microsoft Teams from switching your status to Away due to inactivity.

> **Important:** Teams presence depends on Teams behavior, your organization’s policies, and other settings. MMAway cannot guarantee a particular status. It simulates system-wide mouse input, so the pointer may move and a click may occur while it is active. Stop MMAway before using your computer normally.

## Features

- Start and stop activity from the app window.
- Make randomized mouse movements inside MMAway's text box at variable intervals, including a zero-distance mouse input to signal activity to Windows.
- Open near the right side of the primary work area, matching the Avalonia version.
- Draw one of ten cursor patterns (square, circle, triangle, two hourglasses, spirals, oval, ice-cream cone, or figure eight); selected patterns do not repeat consecutively unless only one is selected. Pattern duration is 2–5 seconds, scaled by the configurable speed (default 1.5x).
- Configure activity intervals, pattern selection, movement speed, typing bursts, and spaces using Options. Settings are saved in `%APPDATA%\MMAway\settings.json`. Options are available while activity is stopped.
- Occasionally perform a simulated left click in the text box only when MMAway is foreground and the target is unobstructed.
- Pause generated activity while the window is minimized or the text box has no usable bounds; restore the window to resume.
- Pause automated activity after real mouse movement or clicks, resuming after the configured idle delay (default 10 seconds). Injected events and recent cursor-warp echoes are filtered.
- Use eased Bézier motion for random moves and optionally show session time and activity counts.
- Type random characters into MMAway’s own text box; every three or four typing cycles, select a short portion for the next text to replace. It does not type messages into Teams.
- Ask Windows to keep the system and display awake while activity is running, and restore the normal sleep behavior when stopped.
- Change the app icon and show an animated running indicator while activity is running.

Long activity intervals or repeated mouse interaction can delay synthetic activity and allow Teams to become Away; typing and movement can also add time between activity signals. Global hotkey and auto-stop values can be saved but are not active yet.

To observe Windows idle time during a field test, run `powershell -File .\scripts\windows-idle-probe.ps1` while MMAway is running. The probe writes a CSV and refuses to overwrite an existing file; it measures Windows idle time, not Teams presence.

## Platform support

| Platform | Support |
| --- | --- |
| Windows | Supported; built with .NET 8 Windows Forms and Windows APIs. |
| Linux and macOS | Not supported by this project. For a separate multiplatform implementation, see [MMAway-Avalonia](https://github.com/safejrz/MMAway-Avalonia). |

## Releases

Visit [MMAway Releases](https://github.com/safejrz/MMAway/releases) for versioned source archives. Releases do not currently include an installer or prebuilt executable.

- **v1.2.0** — Windows-only baseline, field-tested by the project owner. Includes the original WinForms app and basic activity simulation.
- **v1.3.0 — Movement contained** — Extracted activity engine and Windows input services; cursor movement and clicks are contained within the app's output area; adds ten randomized movement patterns, variable pacing, text selection/replacement, and right-side startup placement.
- **v1.4.0 — Movement contained and Enhanced UI** — Adds the fixed 350×500 dark-themed window with a custom draggable title bar and minimize/close controls; removes the output area's scrollbar.

## Download and run

To build and run from source, install the **.NET 8 SDK** on Windows, clone this repository, then run:

```powershell
dotnet run --project .\MMA.csproj
```

In the app, select **Start** to begin and **Stop** to end activity.

## Build

From the repository directory on Windows:

```powershell
dotnet build .\MMA.sln
```

The app targets `net8.0-windows` and uses Windows Forms, so it cannot be run as this project on other operating systems.
