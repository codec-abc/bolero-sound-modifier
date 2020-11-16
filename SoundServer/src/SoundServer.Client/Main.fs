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

type LocalSoundModel = 
    {
        shouldBePlayingSound: bool
        frequency: int
        hasTimeout: bool
        timeoutValue: int
    }

type ServerSoundModel = 
    {
        isPlaying: bool
        frequency: Option<int>
        remainingTime: Option<int>
    }

type Model = 
    {
        page: Page
        error: string option
        localSoundModel: LocalSoundModel
        serverSoundModel: Option<ServerSoundModel>
    }

let initModel = 
    {
        page = Home
        error = None

        localSoundModel = 
            {
                shouldBePlayingSound = false
                frequency = 15
                hasTimeout = false
                timeoutValue = 60
            }

        serverSoundModel = None
    }

type SoundService = 
    { 
        updateSound : LocalSoundModel -> Async<unit>
        getSoundServerStatus: unit -> Async<ServerSoundModel>
    }

    interface IRemoteService with
        member this.BasePath = "/books"

type LocalSoundMessage =
    | ToggleSound
    | SetFrequency of int
    | SetTimerEnabled of bool
    | SetTimerValue of int
    | ValidateSoundSettings

type Message =
    | SetPage of Page
    | Error of exn
    | LocalSoundMessage of LocalSoundMessage
    | ServerSoundUpdate of ServerSoundModel
    | GetSoundServerState
    | ClearError

let updateLocalSoundMessage remote (message: LocalSoundMessage) (model: LocalSoundModel) =
    match message with
    | ToggleSound ->
        { model with shouldBePlayingSound = (not model.shouldBePlayingSound) }, Cmd.none
    | SetFrequency freq ->
        { model with frequency = freq }, Cmd.none
    | SetTimerEnabled(hasTimer) -> 
        { model with hasTimeout = hasTimer }, Cmd.none
    | SetTimerValue(timerValue) -> 
        { model with timeoutValue = timerValue }, Cmd.none
    | ValidateSoundSettings ->
        let task = Async.StartImmediateAsTask (remote.updateSound(model))
        model, Cmd.none

let update remote message model =
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none
    | LocalSoundMessage sndMsg ->
        let (soundModel, command) = 
            updateLocalSoundMessage remote sndMsg model.localSoundModel
        { model with localSoundModel = soundModel }, command
    | ServerSoundUpdate serverMsg -> 
        Console.WriteLine("Server message received " + serverMsg.ToString())
        // TODO
        { model with error = None }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none
    | GetSoundServerState -> 
        model, 
        Cmd.OfAsync.either
            remote.getSoundServerStatus ()
            (ServerSoundUpdate)
            Error

let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let homePage model dispatch =
    Main.Home().Elt()

let soundPage (model: Model) (dispatch: Dispatch<Message>) =
    Main.Sound()
        .localSoundModelShouldBePlayingSound(
            model.localSoundModel.shouldBePlayingSound, 
            fun n ->
                dispatch <| LocalSoundMessage ToggleSound
        )
        .localSoundModelFrequency(
            model.localSoundModel.frequency.ToString(), 
            fun n -> 
                let freqMsg = SetFrequency (int n)
                dispatch <| LocalSoundMessage freqMsg
        )
        .localSoundModelHasTimeout(
            model.localSoundModel.hasTimeout, 
            fun n -> 
                let msg = SetTimerEnabled n
                dispatch <| LocalSoundMessage msg
        )
        .localSoundModelTimerValue(
            model.localSoundModel.timeoutValue.ToString(), 
            fun n -> 
                let msg = SetTimerValue (int n)
                dispatch <| LocalSoundMessage msg
        )
        .ValidateSoundSettings(fun args -> 
            dispatch <| LocalSoundMessage ValidateSoundSettings
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

        let subscription initial =
            let sub dispatch =
                let fn = fun (serverSoundState: String) ->
                    let serverState = System.Text.Json.JsonSerializer.Deserialize<ServerSoundModel>(serverSoundState)
                    dispatch (ServerSoundUpdate serverState) |> ignore

                hubConnection.On<string>(HubNames.soundHubName, fn) |> ignore
                hubConnection.StartAsync() |> ignore
                ()
                    
            Cmd.ofSub sub

        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSoundServerState) update view
        |> Program.withSubscription subscription
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
