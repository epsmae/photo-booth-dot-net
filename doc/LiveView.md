# Live View (Proof of Concept)

> This branch contains two camera drivers, selected with `Camera:Driver` in `appsettings.json`:
>
> - `Cli`: `gphoto2` command line tool, described in this document (live view is stopped for
>   every capture, expected 2-4 s interruption).
> - `LibGPhoto2` (default on this branch): libgphoto2 in process, captures straight out of the
>   live view, see [LiveViewLibGPhoto2.md](LiveViewLibGPhoto2.md).
>
> The camera setup, the streaming to the browser and the `LiveView` settings are the same for both.

This branch adds a live view (camera preview) to the capture page. While the photo booth is
**ready** and during the **count down** the guests see themselves on the screen; the camera
switches back to normal photo mode right before the picture is taken.

> Status: proof of concept. Tested with a simulator and a fake `gphoto2` (see
> [Development without a camera](#development-without-a-camera)). It still has to be verified on
> the real hardware (Raspberry Pi 4 + Nikon D7000), see the [hardware test checklist](#hardware-test-checklist).

## Contents

- [How it works](#how-it-works)
- [Camera setup (Nikon D7000)](#camera-setup-nikon-d7000)
- [Raspberry Pi setup](#raspberry-pi-setup)
- [Hardware test checklist](#hardware-test-checklist)
- [Configuration](#configuration)
- [HTTP API](#http-api)
- [Development without a camera](#development-without-a-camera)
- [Troubleshooting](#troubleshooting)
- [Limitations and next steps](#limitations-and-next-steps)

## How it works

```
 Nikon D7000 ──USB──> gphoto2 --capture-movie --stdout ──MJPEG──> MjpegFrameParser
                                                                        │ JPEG frames
                                                                        v
 Browser <img src="api/LiveView/Stream"> <──multipart/x-mixed-replace── LiveViewService
                                                                  (keeps the newest frame)
```

- `gphoto2 --capture-movie --stdout` streams the camera live view frames (JPEG, about 640x424 on
  the D7000) as one continuous MJPEG stream until it receives `SIGINT` (Ctrl-C). On `SIGINT` gphoto2
  exits cleanly and ends the live view on the camera (mirror down).
- `MjpegFrameParser` splits the stream into single JPEG frames. It follows the JPEG marker
  structure, so thumbnails embedded in a frame cannot split it.
- `LiveViewService` starts/stops gphoto2, keeps the newest frame and hands it to all connected
  browsers. The frames are passed through without re-encoding, so the CPU load on the Raspberry Pi
  stays low.
- The server sends the frames as MJPEG stream (`multipart/x-mixed-replace`). Chromium shows such a
  stream natively in an `<img>` element.

### Live view and the photo workflow

Only one gphoto2 process can use the camera at a time. The live view is therefore stopped (and the
camera released) before every capture:

| Workflow state | Live view |
| -------------- | --------- |
| Initializing   | off |
| Ready          | **on** (started automatically) |
| CountDown      | **on** |
| Capture        | stopped (and awaited) before gphoto2 captures the photo |
| Review / Print | off |
| Error          | off |
| Ready again    | **on** (restarted) |

In addition:

- After `IdleTimeoutSeconds` (default 5 minutes) without a capture the live view stops, to protect
  the camera from overheating and to save battery. The capture page then shows a
  **"Start live view"** button.
- The setup wizard (test capture) stops the live view as well.
- The live view is shown mirrored (selfie view) by default.

### Code overview

| File | Purpose |
| ---- | ------- |
| `PhotoBooth.Abstraction/LiveView/*` | Interfaces, options and status DTO |
| `PhotoBooth.Camera/LiveView/GPhoto2LiveViewSource.cs` | Runs `gphoto2 --capture-movie --stdout`, stops it with `SIGINT` |
| `PhotoBooth.Camera/LiveView/MjpegFrameParser.cs` | Splits the MJPEG stream into JPEG frames |
| `PhotoBooth.Service/LiveView/LiveViewService.cs` | Start/stop, newest frame, idle timeout, status |
| `PhotoBooth.Service/LiveView/SimulatedLiveViewSource.cs` | Animated test frames (Debug builds) |
| `PhotoBooth.Service/WorkflowController.cs` | Starts the live view in Ready/CountDown, stops it before the capture |
| `PhotoBooth.Server/Controllers/LiveViewController.cs` | MJPEG stream, status, start, stop |
| `PhotoBooth.Client/Pages/Capture.razor(.cs/.css)` | Shows the stream behind the capture controls |
| `PhotoBooth.Console/LiveViewCommandHandler.cs` | `liveview` command for hardware tests |
| `tools/fake-gphoto2/gphoto2` | Fake gphoto2 for development and tests |

## Camera setup (Nikon D7000)

libgphoto2 lists the D7000 with live view support. Check the following on the camera:

| Setting | Why |
| ------- | --- |
| Mode dial on **P, A, S or M** | Nikon refuses live view in AUTO and scene modes (`Liveview cannot start: Exposure Program Mode is not P/A/S/M`) |
| Memory card inserted, not write protected | Card errors block the live view; the app stores the photos on the card (`capturetarget=1`) |
| Mains adapter **EH-5b + EP-5B** (recommended) | The live view drains the battery quickly |
| Focus | The app stops the live view before every capture, so the normal (phase detection) autofocus is used. For a photo booth a fixed distance with manual focus (switch on the lens / `M` on the body) is the most reliable choice |
| Monitor off delay for live view (custom settings menu) | Set it to the longest value so the camera does not end the live view by itself. libgphoto2 restarts the live view on the D7000 automatically if the camera drops it |
| Image quality | Unchanged, the live view does not affect the captured photo |

You do **not** need to flip the live view (Lv) lever on the camera, gphoto2 starts the live view
over USB.

Overheating: Nikon bodies stop the live view when the sensor gets too hot
(`Liveview cannot start: Temperature too high`). Keep the idle timeout enabled for events.

## Raspberry Pi setup

### 1. Install gphoto2

```bash
sudo apt-get update
sudo apt-get install gphoto2
gphoto2 --version
```

Raspberry Pi OS ships libgphoto2 2.5.x, which supports the Nikon live view (`--capture-movie`).
Newer versions contain additional Nikon fixes.

### 2. Keep the desktop from claiming the camera

The desktop (gvfs) mounts the camera as soon as it is connected and blocks gphoto2
(`Could not claim the USB device`). The kiosk start script in the [install guide](Install.md)
already kills it (`pkill --f gphoto2`). To disable it permanently:

```bash
systemctl --user stop gvfs-gphoto2-volume-monitor.service
systemctl --user mask gvfs-gphoto2-volume-monitor.service
```

### 3. Check the camera

```bash
gphoto2 --auto-detect
# Model                          Port
# Nikon DSC D7000 (PTP mode)     usb:001,004

gphoto2 --abilities
# should list "Capture choices : Image" and "Preview"
```

### 4. Test the live view with gphoto2 only

```bash
# 5 seconds of live view into a file
gphoto2 --capture-movie=5s --stdout > /tmp/liveview.mjpg
ls -lh /tmp/liveview.mjpg

# optional, play it (VLC or ffmpeg):
ffplay -f mjpeg /tmp/liveview.mjpg
```

The camera mirror goes up while the command runs and comes down afterwards.

### 5. Test with the photo booth console tool

The `liveview` command measures everything needed to judge the live view on the real hardware.

```bash
# on the development machine
dotnet publish src/PhotoBooth.Console/PhotoBooth.Console.csproj -c Release -r linux-arm64 --self-contained false -o publish-console
# copy publish-console to the Raspberry Pi, then on the Raspberry Pi:
cd publish-console
dotnet PhotoBooth.Console.dll liveview --seconds 10
dotnet PhotoBooth.Console.dll liveview --seconds 10 --capture
dotnet PhotoBooth.Console.dll liveview --seconds 5 --output frames   # stores every frame as JPEG
```

Example output (simulator):

```
Frames:            46
Resolution:        640x424
Frames per second: 15.0
Average size:      7 KB (0.9 Mbit/s)
First frame after: 811 ms
Stop duration:     5 ms
Capture duration:  209 ms
Live view -> photo downloaded: 214 ms
```

| Value | Meaning |
| ----- | ------- |
| Frames per second | Smoothness of the live view |
| First frame after | Time from start until the first frame is on screen (mirror up) |
| Stop duration | Time from `SIGINT` until gphoto2 released the camera |
| Live view -> photo downloaded | Delay between end of the count down and the downloaded photo |

### 6. Run the photo booth

Deploy the server as described in the [install guide](Install.md). The live view is enabled by
default (`LiveView:Enabled` in `appsettings.json`). Set `"Camera": { "Driver": "Cli" }` to use the
gphoto2 command line live view described here.

## Hardware test checklist

- [ ] `gphoto2 --capture-movie=5s --stdout > /tmp/liveview.mjpg` creates a file with frames
- [ ] `liveview --seconds 10`: frames per second ______, resolution ______
- [ ] `liveview --capture`: stop duration ______ ms, live view -> photo ______ ms
- [ ] Capture page shows the live view in Ready and during the count down
- [ ] Photo is sharp (autofocus / manual focus) after the live view
- [ ] Gallery layout (4 photos): live view comes back between the photos
- [ ] Print and error dialogs work, live view comes back afterwards
- [ ] Idle timeout stops the live view, "Start live view" restarts it
- [ ] 30 min continuous use: no `Temperature too high`, no `Could not claim the USB device`
- [ ] CPU load of the Raspberry Pi while streaming (`top`) ______ %

## Configuration

`src/PhotoBooth.Server/appsettings.json`:

```json
"LiveView": {
  "Enabled": true,
  "Source": "",
  "Mirror": true,
  "IdleTimeoutSeconds": 300,
  "StopTimeoutMilliseconds": 5000,
  "Camera": "",
  "GPhoto2Path": "gphoto2"
}
```

| Setting | Default | Description |
| ------- | ------- | ----------- |
| `Enabled` | `true` | Enables the live view |
| `Source` | empty | Only with `Camera:Driver = Cli`: `GPhoto2` or `Simulator`; empty: simulator in Debug builds, gphoto2 in Release builds |
| `Mirror` | `true` | Show the live view mirrored (selfie view) |
| `IdleTimeoutSeconds` | `300` | Stop the live view after this time without a capture, `0` disables the timeout |
| `StopTimeoutMilliseconds` | `5000` | Time to wait for gphoto2 after `SIGINT` before it is killed |
| `Camera` | empty | Camera model for `gphoto2 --camera` (only needed with more than one camera) |
| `GPhoto2Path` | `gphoto2` | gphoto2 executable |

All settings can be overridden with environment variables, e.g. `LiveView__Enabled=false`.

## HTTP API

| Request | Description |
| ------- | ----------- |
| `GET api/LiveView/Stream` | MJPEG stream (`multipart/x-mixed-replace`), ends when the live view stops |
| `GET api/LiveView/Status` | `{ enabled, running, mirror, frameCount, framesPerSecond, viewers, lastError }` |
| `POST api/LiveView/Start` | Starts the live view (only in Ready / CountDown, otherwise `409`) |
| `POST api/LiveView/Stop` | Stops the live view |

The status is also pushed to the clients via SignalR (`ReceiveLiveViewStatusChanged`).

Quick checks:

```bash
curl http://localhost:5050/api/LiveView/Status
# open in a browser:
http://<raspberry>:5050/api/LiveView/Stream
```

## Development without a camera

### Simulator

Debug builds use `SimulatedLiveViewSource` (animated 640x424 frames, 15 fps):

```bash
cd src/PhotoBooth.Server
Camera__Driver=Cli dotnet run
# http://localhost:5050
```

### Fake gphoto2

`tools/fake-gphoto2/gphoto2` is a small Python script that behaves like gphoto2 for the commands
used by the photo booth (live view stream, `SIGINT` handling, auto detect, capture). It uses a real
D7000 photo as frame (with embedded EXIF thumbnails, a good test for the frame parser). This runs
the real `GPhoto2LiveViewSource` code:

```bash
cd src/PhotoBooth.Server
Camera__Driver=Cli LiveView__Source=GPhoto2 LiveView__GPhoto2Path=$(realpath ../../tools/fake-gphoto2/gphoto2) dotnet run
```

Simulate camera problems with environment variables:

```bash
# live view refused by the camera
FAKE_GPHOTO2_ERROR="Liveview cannot start: Exposure Program Mode is not P/A/S/M" LiveView__Source=GPhoto2 ...
# gphoto2 hangs on SIGINT (tests the forced kill after StopTimeoutMilliseconds)
FAKE_GPHOTO2_IGNORE_SIGINT=1 LiveView__Source=GPhoto2 ...
# frame rate
FAKE_GPHOTO2_FPS=25 ...
```

### Tests

```bash
dotnet test src/PhotoBooth.sln
```

| Test | Covers |
| ---- | ------ |
| `MjpegFrameParserTest` | Frame splitting: embedded thumbnails, byte-by-byte reads, stuffed bytes, restart markers, truncated frames |
| `GPhoto2LiveViewSourceTest` | Real gphoto2 code against the fake gphoto2: frames, graceful stop via `SIGINT`, error message, forced kill (Linux/macOS, needs `python3`) |
| `LiveViewServiceTest` | Start/stop, frame hand-over, stop timeout, error and restart, idle timeout, viewers |
| `WorkflowLiveViewTest` | Live view is stopped before the capture and restarted when ready |
| `LiveViewTest` (console) | `liveview` command with the simulator |

## Troubleshooting

| Message / symptom | Cause | Fix |
| ----------------- | ----- | --- |
| `Liveview cannot start: Exposure Program Mode is not P/A/S/M` | Mode dial on AUTO or a scene mode | Set the mode dial to P, A, S or M |
| `Liveview cannot start: Battery exhausted` | Battery empty | Charge the battery / use the mains adapter |
| `Liveview cannot start: Temperature too high` | Sensor too hot | Let the camera cool down, keep the idle timeout enabled |
| `Liveview cannot start: Card error` / `Card unformatted` / `Card protected` | Memory card problem | Check / format the card in the camera |
| `Could not claim the USB device` | Another process (gvfs, a second gphoto2) uses the camera | See [step 2](#2-keep-the-desktop-from-claiming-the-camera), `pkill -f gphoto2` |
| `Live view did not stop within 5000ms, killing it` (log) | gphoto2 hangs | Check the camera / USB cable; the camera may stay in live view until it is power cycled |
| Live view does not come back | Idle timeout or camera error | Press "Start live view", check `api/LiveView/Status` (`lastError`) and the log |
| Live view stutters | USB / CPU limit | Check `framesPerSecond` in the status, use a short USB cable directly on the Raspberry Pi |

## Limitations and next steps

- **Switching delay**: gphoto2 is started for the live view and again for the capture, the
  camera needs to leave live view and be claimed again (expected 1-3 s, to be measured with
  `liveview --capture`). If this is too slow, the camera could be driven by **one long running
  process** via libgphoto2 directly (P/Invoke of about 10 functions: `gp_camera_init`,
  `gp_camera_capture_preview`, `gp_camera_capture`, `gp_camera_file_get`, ...). libgphoto2 then
  captures in live view without a restart (Nikon: without autofocus).
- The live view is started when the workflow enters Ready; settings page actions that use the
  camera (camera list) are not coordinated with the live view yet, only the wizard test capture.
- Enable/disable is a configuration file setting, not yet part of the settings page.
- The countdown could be shortened by the live view switch time, so the photo is taken exactly at
  "0".
- No overlay (frame, logo) on the live view yet.
