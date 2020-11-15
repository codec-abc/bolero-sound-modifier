dotnet run -p SoundServer/src/SoundServer.Server/
dotnet build SoundServer/src/SoundServer.Client/

dotnet publish SoundServer/SoundServer.sln -c Release -r linux-arm64
# then copy ./SoundServer/src/SoundServer.Server/bin/Release/netcoreapp5.0/linux-arm64/publish/ on target
# If you use winscp make sure that it don't mess with the file (ie, changing LF to CRLF even on binary files)
# then go into publish dir and type chmod +x SoundServer.Server
# then launch server with ./SoundServer.Server