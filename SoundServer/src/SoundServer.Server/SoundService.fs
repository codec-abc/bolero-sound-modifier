namespace SoundServer.Server

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Hosting
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Server
open SoundServer
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.SignalR
open BlazorSignalRApp.Server.Hubs

type SoundService(log: ILogger<SoundService>, hub: IHubContext<BroadcastHub>, ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.Main.SoundService>()

    override this.Handler = 
        {
            toggleSound = fun () -> async {
                BroadcastHub.SendMessageToClients(hub.Clients.All)
                try
                    SoundPlayer.playSound()
                with | e -> 
                    let msg: String = "Unable to play sound " + e.Message
                    log.LogInformation(msg)
                log.LogInformation("=== Sending message ===")
                ()
            }
        }
