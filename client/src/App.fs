module App

open Feliz
open Elmish
open Shared

type State = { Counter: Deferred<Result<Counter, string>> }

type Msg =
    | Increment
    | Decrement
    | LoadCounter of AsyncOperationStatus<Result<Counter, string>>

let init() = { Counter = HasNotStartedYet }, Cmd.ofMsg (LoadCounter Started)

let update (msg: Msg) (state: State) =
    match msg with
    | LoadCounter Started ->
        let loadCounter = async {
            try
                let! counter = Server.api.getCounter()
                return LoadCounter (Finished (Ok counter))
            with ex ->
                return LoadCounter (Finished (Error "Error while retrieving Counter from server"))
        }

        { state with Counter = InProgress }, Cmd.fromAsync loadCounter

    | LoadCounter (Finished counter) ->
        { state with Counter = Resolved counter }, Cmd.none

    | Increment ->
        let updatedCounter =
            state.Counter
            |> Deferred.map (function
                | Ok counter -> Ok { counter with value = counter.value + 1 }
                | Error error -> Error error)

        { state with Counter = updatedCounter }, Cmd.none

    | Decrement ->
        let updatedCounter =
            state.Counter
            |> Deferred.map (function
                | Ok counter -> Ok { counter with value = counter.value - 1 }
                | Error error -> Error error)

        { state with Counter = updatedCounter }, Cmd.none

let renderCounter = function
    | HasNotStartedYet -> Html.none
    | InProgress -> Html.h1 "Loading..."
    | Resolved (Ok (counter: Counter)) -> Html.h1 counter.value
    | Resolved (Error (errorMsg: string)) ->
        Html.h1 [
            prop.style [ style.color.crimson ]
            prop.text errorMsg
        ]

let inline fableLogo() = StaticFile.import "./imgs/fable_logo.png"

let render (state: State) (dispatch: Msg -> unit) =
    Html.div [
        prop.style [
            style.textAlign.center
            style.padding 40
        ]

        prop.children [
            Html.img [
                prop.src(fableLogo())
                prop.width 250
            ]

            Html.h1 (Config.variable "WELCOME_MESSAGE")
            Html.button [
                prop.onClick (fun _ -> dispatch Increment)
                prop.text "Increment"
            ]

            Html.button [
                prop.onClick (fun _ -> dispatch Decrement)
                prop.text "Decrement"
            ]

            renderCounter state.Counter
        ]
    ]