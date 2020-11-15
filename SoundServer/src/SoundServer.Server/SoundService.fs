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
                //let task = hub.Clients.All.SendAsync("ReceiveMessage", "Test")
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

    // let serializerOptions = JsonSerializerOptions()
    // do serializerOptions.Converters.Add(JsonFSharpConverter())

    //let books =
        // let json = Path.Combine(env.ContentRootPath, "data/books.json") |> File.ReadAllText
        // JsonSerializer.Deserialize<Client.Main.Book[]>(json, serializerOptions)
        // |> ResizeArray

        // getBooks = ctx.Authorize <| fun () -> async {
        //     return books.ToArray()
        // }

        // addBook = ctx.Authorize <| fun book -> async {
        //     books.Add(book)
        // }

        // removeBookByIsbn = ctx.Authorize <| fun isbn -> async {
        //     books.RemoveAll(fun b -> b.isbn = isbn) |> ignore
        // }

        // signIn = fun (username, password) -> async {
        //     if password = "password" then
        //         do! ctx.HttpContext.AsyncSignIn(username, TimeSpan.FromDays(365.))
        //         return Some username
        //     else
        //         return None
        // }

        // signOut = fun () -> async {
        //     return! ctx.HttpContext.AsyncSignOut()
        // }

        // getUsername = ctx.Authorize <| fun () -> async {
        //     return ctx.HttpContext.User.Identity.Name
        // }
