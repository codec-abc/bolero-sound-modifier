namespace BlazorSignalRApp.Server.Hubs

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Logging
open SoundServer.Client.Main

type BroadcastHub(log: ILogger<BroadcastHub>) =
    inherit Hub()

    member this.SendSoundServerStatus(soundServerStatus: ServerSoundModel) =
        BroadcastHub.SendSoundServerStatus(this.Clients.All, soundServerStatus)

    static member SendSoundServerStatus(clients: IClientProxy, soundServerStatus: ServerSoundModel) =
        let text = System.Text.Json.JsonSerializer.Serialize(soundServerStatus)
        let task = clients.SendAsync(HubNames.soundHubName, text)
        ()

      