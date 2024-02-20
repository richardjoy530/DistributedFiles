### Contribution guidelines

_Requirements: .Net 8, Visual Studio, or Jetbrains Rider. or if you are an elite programmer, Use Vim : )_

For simplicity, let's say the path of the repo is `C:/Projects/DistributedFiles`

open cmd to the above location

to start the Master server, run
```
dotnet run --project FileServerMaster --urls http://0.0.0.0:5000
```
- you might need to allow your firewall to enable hosting on the given IPs. (`0.0.0.0` means hosting on all available IPs in the system)
- you can use `localhost` to host the application. However, this will not be available to other devices.
- `5000` can be any free port on your server. The slave application needs to be configured to communicate with this port.
---

to start 2 Slave servers, run
```
dotnet run --project FileServerSlave --urls http://0.0.0.0:5001 --save C:\DistributedLocation\1\
```
and in another terminal
```
dotnet run --project FileServerSlave --urls http://0.0.0.0:5002 --save C:\DistributedLocation\2\
```

`5001`/`5002` can be any available ports on the system
`--save C:\DistributedLocation\1\` is to specify the folder location where the slave application will distribute and download files. 

FileServerSlave/appsettings.Development.json is where you need to configure the master's host address. 

