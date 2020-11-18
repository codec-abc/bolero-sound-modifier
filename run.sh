dotnet run -p SoundServer/src/SoundServer.Server/
dotnet build SoundServer/src/SoundServer.Client/

# Remove existing dir ./SoundServer/src/SoundServer.Server/bin/Release/netcoreapp5.0/linux-arm64/ on build machine
dotnet clean SoundServer/SoundServer.sln && dotnet publish SoundServer/SoundServer.sln -c Release -r linux-arm64
# then copy ./SoundServer/src/SoundServer.Server/bin/Release/netcoreapp5.0/linux-arm64/ on target (erase all existing files beforehand to start clean)
# If you use winscp make sure that it don't mess with the file (ie, changing LF to CRLF even on binary files)
# then go into publish dir and launch server with dotnet SoundServer.Server.dll