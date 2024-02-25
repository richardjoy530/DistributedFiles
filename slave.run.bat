@REM slave.run.bat 10 <-- this will start up 10 slaves

cls
FOR /L %%A IN (1,1,%1) DO (
start cmd.exe /k dotnet run --project FileServerSlave --urls http://0.0.0.0:501%%A --save C:\DistributedLocation\%%A\
)