module SoundServer.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client
open Microsoft.AspNetCore.SignalR.Client

type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/sound">] Sound

type Model = 
    {
        page: Page
        error: string option
        playingSound: bool
        frequency: int   
    }

let initModel = 
    {
        page = Home
        playingSound = false
        frequency = 15000
        error = None
    }

type SoundService = 
    { 
        toggleSound: bool -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/books"

type Message =
    | SetPage of Page
    | Error of exn
    | ToggleSound of bool
    | ClearError

let update remote message model =
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none
    | ToggleSound toggleSoundValue ->
        let task = Async.StartImmediateAsTask (remote.toggleSound(toggleSoundValue))
        { model with playingSound = toggleSoundValue }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let homePage model dispatch =
    Main.Home().Elt()

let soundPage (model: Model) (dispatch: Dispatch<Message>) =
    Main.Sound()
        .ToggleSound(fun args -> 
            dispatch <| ToggleSound (not model.playingSound)
        )
        .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model (dispatch: Dispatch<Message>) =
    Main()
        .Menu(concat [
            menuItem model Home "Home"
            menuItem model Sound "SoundServer"
        ])
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | Sound -> soundPage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch Message.ClearError)
                    .Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let soundService = this.Remote<SoundService>()
        let update = update soundService
        let hubConnectionBuilder = HubConnectionBuilder()

        let uri = this.NavigationManager.ToAbsoluteUri("/broadcasthub")

        let hubConnection = 
            hubConnectionBuilder
                .WithUrl(uri)
                .Build()

        hubConnection.On<string>("ReceiveMessage", fun user -> 
            let toPrint = "message is " + user
            Console.WriteLine(toPrint)
        ) |> ignore
            
        hubConnection.StartAsync() |> ignore

        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
