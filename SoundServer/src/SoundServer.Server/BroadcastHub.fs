namespace BlazorSignalRApp.Server.Hubs

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR

type BroadcastHub() =
    inherit Hub()

    member this.SendMessage() =  
        let task = this.Clients.All.SendAsync("Test")
        ()

    // public async Task SendMessage(string user, string message)
    // {
    //     await Clients.All.SendAsync("ReceiveMessage", user, message);
    // }

