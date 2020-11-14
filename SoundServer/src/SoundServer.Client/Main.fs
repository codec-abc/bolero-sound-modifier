module SoundServer.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/sound">] Sound
    // | [<EndPoint "/counter">] Counter
    // | [<EndPoint "/data">] Data

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
        toggleSound: unit -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/books"

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Error of exn
    | ClearError

//let update remote message model =
let update message model =
    // let onSignIn = function
    //     | Some _ -> Cmd.ofMsg GetBooks
    //     | None -> Cmd.none
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    // | Increment ->
    //     { model with counter = model.counter + 1 }, Cmd.none
    // | Decrement ->
    //     { model with counter = model.counter - 1 }, Cmd.none
    // | SetCounter value ->
    //     { model with counter = value }, Cmd.none

    // | GetBooks ->
    //     let cmd = Cmd.OfAsync.either remote.getBooks () GotBooks Error
    //     { model with books = None }, cmd
    // | GotBooks books ->
    //     { model with books = Some books }, Cmd.none


    // | Error RemoteUnauthorizedException ->
    //     { model with error = Some "You have been logged out."; signedInAs = None }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let homePage model dispatch =
    Main.Home().Elt()

let soundPage model dispatch =
    Main.Sound().Elt()

// let counterPage model dispatch =
//     Main.Counter()
//         .Decrement(fun _ -> dispatch Decrement)
//         .Increment(fun _ -> dispatch Increment)
//         .Value(model.counter, fun v -> dispatch (SetCounter v))
//         .Elt()

// let dataPage model (username: string) dispatch =
//     Main.Data()
//         .Reload(fun _ -> dispatch GetBooks)
//         .Username(username)
//         .SignOut(fun _ -> dispatch SendSignOut)
//         .Rows(cond model.books <| function
//             | None ->
//                 Main.EmptyData().Elt()
//             | Some books ->
//                 forEach books <| fun book ->
//                     tr [] [
//                         td [] [text book.title]
//                         td [] [text book.author]
//                         td [] [text (book.publishDate.ToString("yyyy-MM-dd"))]
//                         td [] [text book.isbn]
//                     ])
//         .Elt()

// let signInPage model dispatch =
//     Main.SignIn()
//         .Username(model.username, fun s -> dispatch (SetUsername s))
//         .Password(model.password, fun s -> dispatch (SetPassword s))
//         .SignIn(fun _ -> dispatch SendSignIn)
//         .ErrorNotification(
//             cond model.signInFailed <| function
//             | false -> empty
//             | true ->
//                 Main.ErrorNotification()
//                     .HideClass("is-hidden")
//                     .Text("Sign in failed. Use any username and the password \"password\".")
//                     .Elt()
//         )
//         .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    Main()
        .Menu(concat [
            menuItem model Home "Home"
            menuItem model Sound "SoundServer"
        ])
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | Sound -> soundPage model dispatch
            // | Counter -> counterPage model dispatch
            // | Data ->
            //     cond model.signedInAs <| function
            //     | Some username -> dataPage model username dispatch
            //     | None -> signInPage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        //let bookService = this.Remote<BookService>()
        //let update = update bookService
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
