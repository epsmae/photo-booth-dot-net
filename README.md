# Photoboot .Net

|Description      |Link        |
|-----------------|------------|
|Sonarcloud       |[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=epsmae_photo-booth-dot-net&metric=alert_status)](https://sonarcloud.io/dashboard?id=epsmae_photo-booth-dot-net) [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=epsmae_photo-booth-dot-net&metric=ncloc)](https://sonarcloud.io/dashboard?id=epsmae_photo-booth-dot-net) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=epsmae_photo-booth-dot-net&metric=coverage)](https://sonarcloud.io/dashboard?id=epsmae_photo-booth-dot-net)|
|Build            |[![Build](https://github.com/epsmae/photo-booth-dot-net/actions/workflows/integration_build.yml/badge.svg?branch=master)](https://github.com/epsmae/photo-booth-dot-net/actions/workflows/integration_build.yml)|


## Technology

The photobooth application is implemented in .Net 5 and uses blazor for the frontent (PWA).
This means the frontent can be used in any browser which has access to the raspberry.
Personally I use the Raspberry Pi touch panel and run the chromium browser directly in raspian.

## Setup

<img src="doc/setup.png" width="20%" />


![Setup](doc/setup.png)

In my Setup I use:

* Raspberry Pi 4 4G
* Raspberry Pi 7" Touch
* Nikon D7000
* Canon SELPHY CP1300

## Screenshots

|Test    |Screenshot 1|Screenshot 2|Screenshot 3|
---------|------------|------------|------------|
|Capture |![Ready](doc/screenshot_capture_ready.JPG)|![CountDown](doc/screenshot_count_down.JPG)|![Review](doc/screenshot_review.JPG)|
|Settings|![Settings](doc/screenshot_settings.JPG)|
|Error   |![Error](doc/screenshot_error.JPG)|
|Progress|![Progress](doc/screenshot_in_progress.JPG)|


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

- [ ] Wizard to check printer, camera
- [ ] Trigger image by raspberry input
- [ ] Skip review count down
- [ ] Capture collage

Version 3.0

- [ ] .Net 6
- [ ] Preview
- [ ] Display capture images
- [ ] Reprint captured images

## State-Machine

![Settings](doc/workflow_controller.png)

## Installation

[Install-Guide](doc/Install.md)