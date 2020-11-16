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
open SoundServer.Client.Main

type SoundService(log: ILogger<SoundService>, hub: IHubContext<BroadcastHub>, ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.Main.SoundService>()

    let mutable isPlaying = false
    let mutable frequency: Option<int> = None
    let mutable timeout: Option<int> = None

    override this.Handler = 
        {
            updateSound = fun (soundModel: LocalSoundModel) -> async {

                isPlaying <- soundModel.shouldBePlayingSound

                frequency <- 
                    if soundModel.shouldBePlayingSound = true then
                        Some <| soundModel.frequency
                    else
                        None
                
                timeout <- 
                    if soundModel.shouldBePlayingSound = true && soundModel.hasTimeout then
                        Some <| soundModel.timeoutValue
                    else
                        None

                try
                    if isPlaying then
                        log.LogInformation("=== PLAYING SOUND ====\n")
                        let freq = frequency.Value
                        SoundPlayer.playSound(log, freq, timeout) |> ignore
                    else
                        log.LogInformation("=== STOP PLAYING SOUND ====\n")
                        SoundPlayer.killSound(log)
                with | e -> 
                    let msg: String = "Unable to play sound " + e.Message
                    log.LogInformation(msg)
                ()

                BroadcastHub.SendSoundServerStatus(hub.Clients.All, this.GetServerModel())

            }

            getSoundServerStatus = fun () -> async {
                return this.GetServerModel()
            }
        }

    member this.GetServerModel() =
        let result : ServerSoundModel = 
            {
                isPlaying = isPlaying
                frequency = frequency
                remainingTime = None
            } in result
