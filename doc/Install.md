# Installation

## Basic rasperry installation

Use the Raspberry Pi Imager to flash the raspian.
If you want to use a display directly connected to the raspberry you should use Raspberry Pi Desktop otherwise you can use Raspberry Pi OS Lite.

Note when you want to use an usb ssd the easiest way is to first flash raspian to the sd card and boot up the raspberry pi from sd card with attached ssd. Once booted you can select the raspian imager "sudo apt install rpi-imager" and you can select the ssd to flash the ssd. Once flashed shutdown the raspberry and remove the sdcard. The raspberry will start from the ssd. You can also change the raspberry boot order in the raspi config under advanced settings. 

### Update the raspberry

```
$ sudo apt-get update
$ sudo apt-get upgrade
```

### Enable ssh

```
$ sudo systemctl enable ssh
$ sudo systemctl start ssh
```

### Change password, keyboard

```
$ sudo raspi-config
```

### Change hostname

Be aware only caracters from 0 bis 9, a to z and - are allowed.

```
$ sudo nano /etc/hosts 
$ sudo nano /etc/hostname
$ sudo reboot
```

Get the host name and ip address:

```
$ hostname
$ ifconfig -a
```

### Increase GPU memory

1.Open Preferences/Raspberry Pi Configuration/Performance
2. Set GPU Memory to 256

## Required software

### .Net sdk and runtime

Download the latest [dotnet core 6 SDK and runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
The links should look similar as below.
I use a Rasperry 4 with the arm64 raspberry pi desktop. If you do not use the arm64 image you should select the arm32 binaries.

You can check the architecture with following command

```
arch
```

The ruslt is armv7a for 32bit and armv8a for 64bit.

```
$ mkdir tmp
$ wget https://download.visualstudio.microsoft.com/download/pr/33389348-a7d7-41ae-850f-ec46d3ca9612/36bad11f948b05a4fa9faac93c35e574/dotnet-sdk-6.0.302-linux-arm64.tar.gz
$ wget https://download.visualstudio.microsoft.com/download/pr/b79c5fa9-a08d-4534-9424-4bacfc3cdc3d/449179d6fe8cda05f52b7be0f6828eb0/aspnetcore-runtime-6.0.7-linux-arm64.tar.gz
```

Now we have to move it depending on the version the downled archive may have different names.

```
$ sudo mkdir /opt/dotnet
$ sudo tar -xvf dotnet-sdk-6.0.302-linux-arm64.tar.gz -C /opt/dotnet/
$ sudo tar -xvf aspnetcore-runtime-6.0.7-linux-arm64.tar.gz -C /opt/dotnet/
$ sudo ln -s /opt/dotnet/dotnet /usr/local/bin
```

Check if dotnet is correctly installed (output shows arm64).

```
$ dotnet --info
.NET SDK (reflecting any global.json):
 Version:   6.0.101
 Commit:    ef49f6213a

Runtime Environment:
 OS Name:     debian
 OS Version:  11
 OS Platform: Linux
 RID:         debian.11-arm64
 Base Path:   /opt/dotnet/sdk/6.0.101/

Host (useful for support):
  Version: 6.0.1
  Commit:  3a25a7f1cc

.NET SDKs installed:
  6.0.101 [/opt/dotnet/sdk]

.NET runtimes installed:
  Microsoft.AspNetCore.App 6.0.1 [/opt/dotnet/shared/Microsoft.AspNetCore.App]
  Microsoft.NETCore.App 6.0.1 [/opt/dotnet/shared/Microsoft.NETCore.App]
```

### gphoto2

Install gphoto2

```
$ sudo apt-get install gphoto2
```

### cups printserver

Depending on the version cups is already installed, lets make sure we can access it from remote. 

```
sudo apt install cups
sudo cupsctl --remote-admin
sudo cupsctl --share-printers
sudo cupsctl --remote-any
sudo usermod -aG lpadmin pi
sudo systemctl restart cups
```

Now you should be able to acess it over your raspberry hostname.

```
http://raspberrypi:631/
```

### unclutter

Install unclutter to hide mouse:

```
sudo apt install unclutter
```

Add unclutter to file '/etc/xdg/lxsession/LXDE-pi/autostart'
It should look similar:

```
@lxpanel --profile LXDE-pi
@pcmanfm --desktop --profile LXDE-pi
@xscreensaver -no-splash
@unclutter -idle 1
```

## Application setup

### Disable screen saver

```
sudo apt-get install xscreensaver
```

Openn Settings/Screensaver and select 'disable screensaver' in the dropdown menu.

### Set photobooth as service

create a service file

```
sudo nano /etc/systemd/system/photobooth.service
```

with the following content

```
[Unit]
Description=PhotoBooth App

[Service]
WorkingDirectory=/home/pi/photobooth
ExecStart=dotnet PhotoBooth.Server.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=photobooth
User=root

[Install]
WantedBy=multi-user.target
```

How to start the service and display log file

```
sudo systemctl enable photobooth.service
sudo systemctl start photobooth.service
sudo systemctl status photobooth.service
journalctl -u photobooth.service
```

### Auto Start kiosk browser

Create start script

```
nano /home/pi/start.photobooth.sh
```

The script will kill the gphoto2 camera viewer and start the application in kiosk mode

```
#!/bin/bash
sleep 10
pkill --f gphoto2
chromium-browser --kiosk http://localhost:5050
```

make the script executeable

```
chmod +x /home/pi/start.photobooth.sh
```

Full screen mode can be closed with alt + F4

Add to start script:

```
sudo nano /etc/xdg/lxsession/LXDE-pi/autostart
```

```
@bash /home/pi/start.photobooth.sh
```

## Checking the System

### Show printers

```
lpstat -t
```

### Show all USB devices should contain camera and printer

```
$ lsusb
Bus 002 Device 001: ID 1d6b:0003 Linux Foundation 3.0 root hub
Bus 001 Device 004: ID 0513:0318 digital-X, Inc.
Bus 001 Device 005: ID 04a9:32db Canon, Inc. SELPHY CP1300
Bus 001 Device 003: ID 04b0:0428 Nikon Corp. D7000
Bus 001 Device 002: ID 2109:3431 VIA Labs, Inc. Hub
Bus 001 Device 001: ID 1d6b:0002 Linux Foundation 2.0 root hub
```

### Check free storage on the raspberry

```
$df -h
Filesystem      Size  Used Avail Use% Mounted on
/dev/root        29G  4.0G   24G  15% /
devtmpfs        1.7G     0  1.7G   0% /dev
tmpfs           1.8G     0  1.8G   0% /dev/shm
tmpfs           1.8G  8.6M  1.8G   1% /run
tmpfs           5.0M  4.0K  5.0M   1% /run/lock
tmpfs           1.8G     0  1.8G   0% /sys/fs/cgroup
/dev/mmcblk0p1  253M   48M  205M  19% /boot
tmpfs           365M  4.0K  365M   1% /run/user/1000
```

### Check free memory

```
$free
            total        used        free      shared  buff/cache   available
Mem:        3736968      182824     3010704       24120      543440     3401252
Swap:        102396           0      102396
```

### Display Temperature

```
watch -n1 vcgencmd measure_temp
```
