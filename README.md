# MMAway

MMAway is a small **Windows-only** desktop utility that simulates occasional mouse activity and prevents Windows from entering sleep while it is running. It is intended to help keep Microsoft Teams from switching your status to Away due to inactivity.

> **Important:** Teams presence depends on Teams behavior, your organization’s policies, and other settings. MMAway cannot guarantee a particular status. It simulates system-wide mouse input, so the pointer may move and a click may occur while it is active. Stop MMAway before using your computer normally.

## Features

- Start and stop activity from the app window.
- Make small, randomized mouse movements at variable intervals.
- Occasionally perform a larger mouse pattern and a simulated left click near the MMAway window.
- Type random characters into MMAway’s own text box; it does not type messages into Teams.
- Ask Windows to keep the system and display awake while activity is running, and restore the normal sleep behavior when stopped.
- Change the app icon to indicate whether activity is running.

## Platform support

| Platform | Support |
| --- | --- |
| Windows | Supported; built with .NET 8 Windows Forms and Windows APIs. |
| Linux and macOS | Not supported by this project. For a separate multiplatform implementation, see [MMAway-Avalonia](https://github.com/safejrz/MMAway-Avalonia). |

## Download and run

Visit [MMAway Releases](https://github.com/safejrz/MMAway/releases) for versioned source archives. The `v1.2.0` release marks the Windows version field-tested by the project owner. The release does not currently include an installer or prebuilt executable.

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
