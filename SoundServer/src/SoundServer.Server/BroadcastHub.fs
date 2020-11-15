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
        BroadcastHub.SendMessageToClients(this.Clients.All)
        ()

    static member SendMessageToClients(clients: IClientProxy) =
        let task = clients.SendAsync("ReceiveMessage", "Test")
        ()