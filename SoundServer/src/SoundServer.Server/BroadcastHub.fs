namespace BlazorSignalRApp.Server.Hubs

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR
open Microsoft.Extensions.Logging

type BroadcastHub(log: ILogger<BroadcastHub>) =
    inherit Hub()

    member this.SendMessage() =  
        let task = this.Clients.All.SendAsync("ReceiveMessage", "Test")
        log.LogInformation("=== Sending message ===")
        ()
