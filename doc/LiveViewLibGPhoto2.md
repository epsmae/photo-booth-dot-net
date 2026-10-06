# Live View with libgphoto2 (Proof of Concept 2)

This branch drives the camera with **libgphoto2 directly from .NET** (P/Invoke) instead of starting
the `gphoto2` command line tool for every action. One camera connection is shared by the live view
and the capture, so the photo is taken **straight out of the running live view**:

| | PoC 1: `gphoto2` command line ([LiveView.md](LiveView.md)) | PoC 2: libgphoto2 in process (this document) |
| --- | --- | --- |
| Live view | `gphoto2 --capture-movie --stdout` process | `gp_camera_capture_preview` in a loop |
| Capture | stop live view (SIGINT), new `gphoto2` process, reconnect | between two preview frames, same connection |
| Live view interruption | stop + process start + connect + capture + restart (expected 2-4 s) | capture + download only, live view continues right after |
| Autofocus | regular phase detection AF (live view is stopped) | configurable, see [Focus](#focus) |
| Dependencies | `gphoto2` tool | `libgphoto2` library (installed with `gphoto2`) |

Both drivers are part of this branch and can be switched with one setting (`Camera:Driver`), so they
can be compared on the same installation.

> Status: proof of concept. Tested with a simulated camera, the native bindings were verified
> against libgphoto2 2.5.31 (without camera). It still has to be verified with the Nikon D7000,
> see the [hardware test](#hardware-test).

## How it works

```
                       ┌──────────────────────── CameraSession (one thread) ─────────────────────────┐
 LiveViewService ─────>│ preview active: gp_camera_capture_preview -> frame -> LiveViewService       │
 (MJPEG stream)        │                                                                             │
 LibGPhoto2Camera- ───>│ queued command (capture, config): runs between two frames                   │──USB── Nikon D7000
 Service (capture)     │   gp_camera_capture -> gp_camera_file_get -> gp_file_save                   │
                       │ no preview for LiveViewHoldSeconds: gp_camera_exit (live view off)           │
                       └─────────────────────────────────────────────────────────────────────────────┘
```

- `CameraSession` owns the libgphoto2 camera handle. libgphoto2 is not thread safe, so all camera
  calls run on one dedicated thread. While the live view is active the thread fetches preview
  frames back to back; commands such as a capture are queued and executed between two frames.
- The first `gp_camera_capture_preview` starts the live view on the camera (mirror up).
  libgphoto2 captures while the live view is active and restarts the live view after the capture
  (Nikon), so the next preview frame is available right after the download.
- When nobody needs the live view (review, print, error) the session stops fetching frames but
  keeps the camera in live view for `LiveViewHoldSeconds` (default 60 s) for an instant restart.
  After that the camera connection is closed (`gp_camera_exit`), which ends the live view
  (mirror down) and lets the sensor cool down. The next action reconnects automatically.
- USB errors (camera switched off / unplugged) close the connection, the next frame or command
  reconnects.
- The streaming to the browser (MJPEG endpoint, capture page) is the same as in PoC 1.

### Workflow

| Workflow state | Live view |
| -------------- | --------- |
| Ready / CountDown | on |
| Capture | **keeps running**, the capture is executed between two preview frames |
| CountDown between gallery photos | on again right after the download |
| Review / Print / Error | frames paused (camera stays in live view for `LiveViewHoldSeconds`) |
| Idle (`LiveView:IdleTimeoutSeconds`) | off, "Start live view" button on the capture page |

### Code overview

| File | Purpose |
| ---- | ------- |
| `PhotoBooth.Camera/LibGPhoto2/GPhoto2Native.cs` | P/Invoke declarations, library loading (`libgphoto2.so.6`) |
| `PhotoBooth.Camera/LibGPhoto2/GPhoto2Api.cs` | Connect, preview, capture, download, config; libgphoto2 errors incl. camera message (e.g. "Out of Focus") |
| `PhotoBooth.Camera/LibGPhoto2/CameraSession.cs` | Camera thread: preview loop, command queue, focus mode, reconnect, live view hold |
| `PhotoBooth.Camera/LibGPhoto2/LibGPhoto2CameraService.cs` | `ICameraService` for the workflow and the setup wizard, error mapping |
| `PhotoBooth.Camera/LibGPhoto2/LibGPhoto2LiveViewSource.cs` | Live view frames for the `LiveViewService` |
| `PhotoBooth.Abstraction/LibGPhoto2/*` | `IGPhoto2Api`, `CameraDriverOptions`, `GPhoto2Exception` |
| `PhotoBooth.Service/LiveView/SimulatedGPhoto2Api.cs` | Simulated camera with D7000 like timing |
| `PhotoBooth.Console/LibGPhoto2LiveViewCommandHandler.cs` | `liveview-libgphoto2` hardware test |

## Focus

libgphoto2 captures out of the live view **without** the phase detection autofocus (the mirror is
up). Choose the focus mode with `Camera:FocusMode`:

| FocusMode | Behaviour | Live view interruption |
| --------- | --------- | ---------------------- |
| `None` (default) | Capture directly out of the live view. Recommended for a photo booth: focus once on the position of the guests and switch the lens to **manual focus (M)** | capture + download |
| `LiveViewAutofocus` | Contrast autofocus in live view (Nikon config `autofocusdrive`) right before the capture | + autofocus time (D7000 contrast AF is slow, about 1 s) |
| `PhaseDetect` | Leave the live view (`gp_camera_exit`), capture with the regular autofocus, live view restarts with the next frame | + mirror down/up and reconnect (about 1-2 s, no process start) |

## Camera setup (Nikon D7000)

The camera requirements are the same as for PoC 1, see [Camera setup](LiveView.md#camera-setup-nikon-d7000):
mode dial on **P, A, S or M**, memory card inserted, mains adapter recommended, live view monitor
off delay set to the longest value. In addition:

- `FocusMode=None`: switch the lens to **M** after focusing on the guests.
- The photos are stored on the memory card and downloaded (`Camera:CaptureTarget = "Memory card"`,
  same as `--set-config capturetarget=1` of the command line version).

## Raspberry Pi setup

### 1. libgphoto2

libgphoto2 is installed together with the gphoto2 tool:

```bash
sudo apt-get install gphoto2          # installs libgphoto2-6 (libgphoto2-6t64 on newer releases)
ldconfig -p | grep libgphoto2.so      # must list libgphoto2.so.6
```

The application loads `libgphoto2.so.6`. If it is missing, the log shows
`libgphoto2 not found (tried libgphoto2.so.6, ...)`.

### 2. USB permissions

libgphoto2 runs inside the photo booth process, so the user of the photo booth service needs access
to the camera. The libgphoto2 udev rules give PTP cameras to the group `plugdev`:

```bash
groups pi                    # must contain plugdev
sudo usermod -aG plugdev pi  # if not, then log in again / reboot
```

### 3. Desktop must not claim the camera

Same as PoC 1: stop the gvfs gphoto2 monitor, see
[Keep the desktop from claiming the camera](LiveView.md#2-keep-the-desktop-from-claiming-the-camera).
Do not run the `gphoto2` tool while the photo booth is running, only one program can use the
camera.

### 4. Hardware test with the console tool

```bash
dotnet publish src/PhotoBooth.Console/PhotoBooth.Console.csproj -c Release -r linux-arm64 --self-contained false -o publish-console
# copy to the Raspberry Pi, stop the photo booth service, then:
cd publish-console
dotnet PhotoBooth.Console.dll liveview-libgphoto2 --seconds 10 --captures 3
dotnet PhotoBooth.Console.dll liveview-libgphoto2 --captures 3 --focus LiveViewAutofocus
dotnet PhotoBooth.Console.dll liveview-libgphoto2 --captures 3 --focus PhaseDetect
dotnet PhotoBooth.Console.dll liveview-libgphoto2 --captures 1 --output test   # stores photos and frames
```

Example output (simulated camera, `--simulate`):

```
Connected:         306 ms, Nikon DSC D7000 (simulated)
First frame after: 573 ms
Resolution:        640x424
Frames per second: 15.6
Capture 1: capture + download 691 ms, live view gap 700 ms, live view back 4 ms after the download
Capture 2: capture + download 640 ms, live view gap 696 ms, live view back 4 ms after the download
```

| Value | Meaning |
| ----- | ------- |
| capture + download | Time from the capture command until the photo is stored on the Raspberry Pi |
| live view gap | Longest time without live view frame around the capture (what the guests see as freeze) |
| live view back | Time from the end of the download to the next live view frame |

Compare with PoC 1: `dotnet PhotoBooth.Console.dll liveview --seconds 10 --capture`.

### 5. Run the photo booth

`src/PhotoBooth.Server/appsettings.json`:

```json
"Camera": {
  "Driver": "LibGPhoto2",
  "Simulate": null,
  "FocusMode": "None",
  "CaptureTarget": "Memory card",
  "LiveViewHoldSeconds": 60,
  "MaxPreviewErrors": 5
}
```

| Setting | Default | Description |
| ------- | ------- | ----------- |
| `Driver` | `LibGPhoto2` | `LibGPhoto2` (this document) or `Cli` (gphoto2 command line, PoC 1) |
| `Simulate` | `null` | Simulated camera; `null`: simulated in Debug builds, real camera in Release builds |
| `FocusMode` | `None` | `None`, `LiveViewAutofocus` or `PhaseDetect`, see [Focus](#focus) |
| `CaptureTarget` | `Memory card` | Camera config `capturetarget` set after connecting, empty: unchanged |
| `LiveViewHoldSeconds` | `60` | Keep the camera in live view after the last frame, then end it (mirror down) |
| `MaxPreviewErrors` | `5` | Consecutive preview errors before the live view is reported as failed |

The `LiveView` section (enabled, mirror, idle timeout) is described in [LiveView.md](LiveView.md#configuration).
Switch back to the command line driver with `"Driver": "Cli"` (or the environment variable
`Camera__Driver=Cli`).

## Hardware test

- [ ] `liveview-libgphoto2 --captures 3`: frames per second ______, live view gap ______ ms
- [ ] Photos are sharp with the chosen focus mode (`None` + manual focus / `LiveViewAutofocus` / `PhaseDetect`)
- [ ] Capture page: live view in Ready and count down, photo taken without visible switch delay
- [ ] Gallery layout (4 photos): live view between the photos
- [ ] Review / print / error and back to Ready: live view comes back immediately
- [ ] After `LiveViewHoldSeconds` without live view the mirror goes down
- [ ] Switch the camera off and on while the photo booth runs: the live view / capture reconnects
- [ ] 30 min continuous use: no `Temperature too high`
- [ ] CPU load of the Raspberry Pi while streaming (`top`) ______ %
- [ ] Same tests with `Camera:Driver = Cli` for comparison

## Development without a camera

Debug builds use `SimulatedGPhoto2Api` (animated preview, 600 ms capture, generated photo), which
runs the real `CameraSession`, live view source and camera service:

```bash
cd src/PhotoBooth.Server
dotnet run            # http://localhost:5050
```

Real libgphoto2 in a Debug build: `Camera__Simulate=false dotnet run` (needs a camera).
Command line driver: `Camera__Driver=Cli dotnet run`.

### Tests

| Test | Covers |
| ---- | ------ |
| `CameraSessionTest` | Preview loop, capture during live view without reconnect, all camera calls on one thread, focus modes, reconnect after USB errors, error reporting, live view hold |
| `LibGPhoto2CameraServiceTest` | Capture, error mapping (no camera, claim, out of focus, ...), live view source |
| `GPhoto2ApiNativeTest` | Real libgphoto2 without camera: library loading, `gp_camera_init` error (skipped if libgphoto2 is not installed) |
| `WorkflowLiveViewTest` | Live view keeps running during the capture with a shared camera connection |
| `LiveViewTest` (console) | `liveview-libgphoto2 --simulate` |

## Troubleshooting

| Message / symptom | Cause | Fix |
| ----------------- | ----- | --- |
| `libgphoto2 not found` | Library not installed | `sudo apt-get install gphoto2` (or `libgphoto2-6`) |
| `gp_camera_init failed: Unknown model (-105): Could not detect any camera` | No camera, camera off, USB cable | Check `gphoto2 --auto-detect` (with the photo booth stopped) |
| `gp_camera_init failed: Could not claim the USB device (-53)` | gvfs or a `gphoto2` process uses the camera | Stop gvfs (see setup), `pkill -f gphoto2` |
| `Could not lock the device (-60)` / permission errors | User not in `plugdev` | `sudo usermod -aG plugdev <user>` |
| `Liveview cannot start: Exposure Program Mode is not P/A/S/M` | Mode dial | P, A, S or M |
| Blurry photos | Capture out of live view has no phase detection AF | Manual focus, or `FocusMode` `LiveViewAutofocus` / `PhaseDetect` |
| Live view freezes after a capture | Camera did not restart the live view | Check the log (`Preview failed`), the session reconnects after `MaxPreviewErrors` |

## Limitations and next steps

- Only the first detected camera is used (`gp_camera_init` auto detection).
- Storage info and camera status (`FetchStorageInfo`, `FetchCameraStatus`) are not implemented for
  this driver (they are not used by the workflow).
- `LiveViewAutofocus` uses the Nikon config `autofocusdrive`, other brands use other names.
- The live view is hidden on the capture page during the capture state; with this driver it could
  stay visible (frozen frame) for an even smoother transition.
