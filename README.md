
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
