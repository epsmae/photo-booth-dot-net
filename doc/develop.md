# Build

## Create Release over CLI

There is some data cashed from the aot compilation to make sure we have a clean build we use git clean.
Note build takes at least 10 times longer!

```
git clean -dxf

dotnet publish --configuration Release --framework net6.0 src/PhotoBooth.Server/PhotoBooth.Server.csproj
```

Move src/PhotoBooth.Server/bin/Release/net6.0/publish folder to raspberry.
Set the execution flag:

```
chmod +x PhotoBooth.Server.dll
```

Run the application:

```
dotnet PhotoBooth.Server.dll
```

You can now access the application over localshost:5050 directly on the raspberry or external ofer hostnameOrIp:8080