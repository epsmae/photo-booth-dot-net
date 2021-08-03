
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


## Required tools

### .Net sdk and runtime
Download the latest [dotnet core 5 SDK and runtime](https://dotnet.microsoft.com/download/dotnet/5.0).
The links should look similar as below.
```
$ mkdir tmp
$ wget https://download.visualstudio.microsoft.com/download/pr/97820d77-2dba-42f5-acb5-74c810112805/84c9a471b5f53d6aaa545fbeb449ad2a/dotnet-sdk-5.0.301-linux-arm.tar.gz
$ wget https://download.visualstudio.microsoft.com/download/pr/2f690848-1342-4768-a7d7-45fa476a4a22/50dd1c50ed7864140b04fec057bb8bd6/aspnetcore-runtime-5.0.7-linux-arm.tar.gz
```

Now we have to move it depending on the version the downled archive may have different names.
```
$ sudo mkdir /opt/dotnet
$ sudo tar -xvf dotnet-sdk-5.0.301-linux-arm.tar.gz -C /opt/dotnet/
$ sudo tar -xvf aspnetcore-runtime-5.0.7-linux-arm.tar.gz -C /opt/dotnet/
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



