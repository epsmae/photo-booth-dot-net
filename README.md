
# Photoboot .Net

## Installation

### Basic rasperry installation

Use the Raspberry Pi Imager to flash the raspian.
If you want to use a display directly connected to the raspberry you should use Raspberry Pi Desktop otherwise you can use Raspberry Pi OS Lite.

#### Update the raspberry
```
$ sudo apt-get update
$ sudo apt-get upgrade
```

#### Enable ssh
```
$ sudo systemctl enable ssh
$ sudo systemctl start ssh
```

#### Change password, keyboard
```
$ sudo raspi-config
```
#### Change hostname

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

#### Increase GPU memory
1.Open Preferences/Raspberry Pi Configuration/Performance
2. Set GPU Memory to 256




## Required tools

### .Net sdk and runtime
Download the latest [dotnet core 5 SDK and runtime](https://dotnet.microsoft.com/download/dotnet/5.0).
The links should look similar as below.
```
$ mkdir tmp
$ wget https://download.visualstudio.microsoft.com/download/pr/70bdb5a9-34cc-4f28-aa33-15535f73b593/7d31d53187c8937206bcc3b117b88978/dotnet-sdk-5.0.400-linux-arm.tar.gz
$ wget https://download.visualstudio.microsoft.com/download/pr/08f79414-91fe-4072-a75b-7b7c21d0fced/46c49c781f43901eb7c27c465c448b0a/aspnetcore-runtime-5.0.9-linux-arm.tar.gz
```

Now we have to move it depending on the version the downled archive may have different names.
```
$ sudo mkdir /opt/dotnet
$ sudo tar -xvf dotnet-sdk-5.0.400-linux-arm.tar.gz -C /opt/dotnet/
$ sudo tar -xvf aspnetcore-runtime-5.0.9-linux-arm.tar.gz -C /opt/dotnet/
$ sudo ln -s /opt/dotnet/dotnet /usr/local/bin
```

Check if dotnet is correctly installed.
```
$ dotnet --info
.NET SDK (reflecting any global.json):
 Version:   5.0.301
 Commit:    ef17233f86

Runtime Environment:
 OS Name:     raspbian
 OS Version:  10
 OS Platform: Linux
 RID:         linux-arm
 Base Path:   /opt/dotnet/sdk/5.0.301/

Host (useful for support):
  Version: 5.0.7
  Commit:  556582d964

.NET SDKs installed:
  5.0.301 [/opt/dotnet/sdk]

.NET runtimes installed:
  Microsoft.AspNetCore.App 5.0.7 [/opt/dotnet/shared/Microsoft.AspNetCore.App]
  Microsoft.NETCore.App 5.0.7 [/opt/dotnet/shared/Microsoft.NETCore.App]
```

Install gphoto2
```
$ sudo apt-get install gphoto2
```

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

Disable screen saver:
```
sudo apt-get install xscreensaver
```
Openn Settings/Screensaver and select 'disable screensaver' in the dropdown menu.



Set photobooth as service

```
sudo nano /etc/systemd/system/photobooth.service
```

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


Start and Log
```
sudo systemctl enable photobooth.service
sudo systemctl start photobooth.service
sudo systemctl status photobooth.service
journalctl -u photobooth.service
```


Auto Start kiosk browser


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

Show printers
```
lpstat -t
```

Show all USB devices should contain camera and printer
```
$ lsusb
Bus 002 Device 001: ID 1d6b:0003 Linux Foundation 3.0 root hub
Bus 001 Device 004: ID 0513:0318 digital-X, Inc.
Bus 001 Device 005: ID 04a9:32db Canon, Inc. SELPHY CP1300
Bus 001 Device 003: ID 04b0:0428 Nikon Corp. D7000
Bus 001 Device 002: ID 2109:3431 VIA Labs, Inc. Hub
Bus 001 Device 001: ID 1d6b:0002 Linux Foundation 2.0 root hub
```


Check free storage on the raspberry
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

Check free memory
```
$free
            total        used        free      shared  buff/cache   available
Mem:        3736968      182824     3010704       24120      543440     3401252
Swap:        102396           0      102396

```

GPU Temperature
```
$vcgencmd measure_temp
temp=51.1'C

```

display Cpu temperature
```
cpu=$(</sys/class/thermal/thermal_zone0/temp) echo "CPU Temperature: $((cpu/1000)) °C"
```
cpu=$(</sys/class/thermal/thermal_zone0/temp) echo "$((cpu/1000)) c"
