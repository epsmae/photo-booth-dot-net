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

Install the [.NET 10 SDK and runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).
I use a Rasperry 4 with the arm64 raspberry pi desktop. The 32bit (arm32) image is also supported by .NET 10, but arm64 is recommended.

You can check the architecture with following command

```
arch
```

The result is armv7l for 32bit and aarch64 for 64bit.

The easiest way is to use the official install script, it detects the architecture automatically.
The SDK contains the ASP.NET Core runtime as well.

```
$ wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
$ chmod +x dotnet-install.sh
$ sudo ./dotnet-install.sh --channel 10.0 --install-dir /opt/dotnet
$ sudo ln -s /opt/dotnet/dotnet /usr/local/bin
```

If you only want to run the application (no build on the raspberry) the ASP.NET Core runtime is sufficient:

```
$ sudo ./dotnet-install.sh --channel 10.0 --runtime aspnetcore --install-dir /opt/dotnet
```

Check if dotnet is correctly installed (output shows arm64 and a 10.0.x runtime).

```
$ dotnet --info
...
Host:
  Version:      10.0.x
  Architecture: arm64
...
.NET runtimes installed:
  Microsoft.AspNetCore.App 10.0.x [/opt/dotnet/shared/Microsoft.AspNetCore.App]
  Microsoft.NETCore.App 10.0.x [/opt/dotnet/shared/Microsoft.NETCore.App]
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

`https://<HostNameOrIP>:631/admin/`

You need to accept the non truseted page and then navigate to Administration add printer. A popup will occur where you have to log in with your raspberry pi user.

##### Troubleshoot printer not shown

Check that printer is visible with

`lsusb`

Check that printer is visible with

`lpstat -p`

Check the cups conf:

```
sudo nano /etc/cups/printers.conf
sudo nano /etc/cups/cups.conf
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

Openn Raspberry Pi Configuration, switch to the display tab and disable screen blanking

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
