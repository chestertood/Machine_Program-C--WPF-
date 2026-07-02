# Machine_Program (C# WPF)

WPF desktop apps controlling factory test/assembly stations. Each app talks to a PLC (or skips it), a barcode/code scanner, a vision camera, and an MES server, then logs pass/fail results.

## Projects

| Folder | Target | Description |
|---|---|---|
| [`1.Machine(PLC)`](1.Machine(PLC)) | .NET 8 WPF | Station with PLC in the loop. Confirms E-Stop released and PLC reachable via serial before allowing a run. |
| [`2.Machine(Non-PLC)`](2.Machine(Non-PLC)) | .NET 8 WPF | Same station workflow, no PLC dependency. |

Both projects share the same file layout and communication classes.

## How it works

1. **Startup check** (`MainWindow.CheckConnection`) — loads `Config.ini`, then verifies each device is reachable before enabling the run button:
   - PLC (serial, project 1 only)
   - Scanner (TCP)
   - Camera (TCP)
   - MES server (HTTPS)
   - LEDs on the UI (`Led.xaml`) show live status per device.
2. **Run flow** (`Page1` → `Page2` → `Page3`, swapped into `MainWindow`'s content area) — scan a serial number, trigger camera inspection, push the result to MES, write a local log.
3. **Result handling** — `MES.cs` posts/gets data via HTTP to the shopfloor URL endpoints defined in `Config.ini` (register / delete / check data by SN). `Logfile.cs` and `Readfile.cs` persist run results and read config-defined paths.

## Key source files

- `MainWindow.xaml.cs` — app shell, page navigation, connection-check loop.
- `PLC_comunication.cs` — serial (RS-232/RS-485) commands to the PLC (E-Stop check, bit read/write).
- `TCP_Communication.cs` — raw TCP client used for the scanner and camera.
- `MES.cs` — HTTP client wrapping the MES shopfloor API (register/check/delete SN data).
- `Logfile.cs` / `Readfile.cs` — local run-log and config file I/O.
- `MODEL1.cs` / `MODEL2.cs` — per-model parameter sets (RD codes) read from `Config.ini`.

## Configuration

Each project reads `Config.ini` next to the executable:

```ini
[PLC]
comport = COM9
buadrate = 9600
device = EE

[MES]
ip = <mes-host>
Current_Station = 250000

[SR_X300W_1]      ; scanner
ip = 192.168.100.5
Port = 9004

[CV-X490F_1]       ; Keyence vision camera
ip = 192.168.1.24
Port = 8500

[Path]
Log = <local log folder>
MES_Log = <mes log folder>

[URL]
Regist_Data = /des/elm/getparameter.asp?...
```

Update `comport`, IPs, ports, and paths for the target machine before running.

## Build & run

Requires .NET 8 SDK + Windows (WPF).

```bash
cd "1.Machine(PLC)"        # or "2.Machine(Non-PLC)"
dotnet restore
dotnet build
dotnet run
```

Or open the `Program_CT.sln` in Visual Studio 2022+.
