# Photoboot .Net

| Description | Link                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| ----------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Sonarcloud  | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=epsmae_photo-booth-dot-net&metric=alert_status)](https://sonarcloud.io/dashboard?id=epsmae_photo-booth-dot-net) [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=epsmae_photo-booth-dot-net&metric=ncloc)](https://sonarcloud.io/dashboard?id=epsmae_photo-booth-dot-net) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=epsmae_photo-booth-dot-net&metric=coverage)](https://sonarcloud.io/dashboard?id=epsmae_photo-booth-dot-net) |
| Build       | [![Build](https://github.com/epsmae/photo-booth-dot-net/actions/workflows/integration_build.yml/badge.svg?branch=master)](https://github.com/epsmae/photo-booth-dot-net/actions/workflows/integration_build.yml)                                                                                                                                                                                                                                                                                                                                                       |

## Technology

The photobooth application is implemented in .Net 6 and uses blazor webassembly for the frontent (PWA).
This means the frontent can be used in any browser which has access to the raspberry.
Personally I use the Raspberry Pi touch panel and run the chromium browser directly in raspian. The [cups print server](http://www.cups.org/) is used for printing. To control the camera I wrote a command line wrapper for [gphoto2](http://www.gphoto.org/).

## Setup

<img src="doc/setup.png" width="50%" />

In my Setup I use:

* Raspberry Pi 4 4G
* Raspberry Pi 7" Touch
* Nikon D7000
* Canon SELPHY CP1300
* fantec WK-200

## Screenshots

| Topic    | Screenshot 1                                | Screenshot 2                                | Screenshot 3                           | Screenshot 4                           |
| -------- | ------------------------------------------- | ------------------------------------------- | -------------------------------------- | -------------------------------------- |
| Settings | ![Settings](doc/screenshot_settings.JPG)    |                                             |                                        |                                        |
| Wizard   | ![Wizard](doc/screenshot_wizard_1.JPG)      | ![Wizard](doc/screenshot_wizard_2.JPG)      | ![Wizard](doc/screenshot_wizard_3.JPG) | ![Wizard](doc/screenshot_wizard_4.JPG) |
| Capture  | ![Ready](doc/screenshot_capture_ready.JPG)  | ![CountDown](doc/screenshot_count_down.JPG) | ![Review](doc/screenshot_review.JPG)   | ![Review](doc/screenshot_review_2.JPG) |
| Error    | ![Error](doc/screenshot_error.JPG)          |                                             |                                        |                                        |
| Progress | ![Progress](doc/screenshot_in_progress.JPG) |                                             |                                        |                                        |

## Features

Version 1.0

- [x] Capture Countdown
- [x] Review Countdown
- [x] Capture Photo
- [x] Review
- [x] Print Photo
- [x] Settings (Countdown duration, rewviw quality, review size, ...)
- [x] Display printer queue
- [x] Clear Printer queue
- [x] Display captured image filenames
- [x] Remove captured image filenames
- [x] Display error messages
- [x] Localization en/de
- [x] File-Logging

Version 2.0

- [x] .Net 6 aot
- [x] Wizard to check printer, camera
- [x] Trigger image by raspberry input
- [x] Support for two raspberry state outputs 
- [x] Skip review count down
- [x] Capture collage

Version 3.0

- [ ] Preview
- [ ] Display capture images
- [ ] Reprint captured images

## State-Machine

![Settings](doc/workflow_controller.png)

## Installation

[Install-Guide](doc/Install.md)

## Development

[Development-Guide](doc/development.md)