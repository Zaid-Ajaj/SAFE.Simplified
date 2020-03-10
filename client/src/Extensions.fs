[<AutoOpen>]
module Extensions

open Elmish

let isDevelopment =
    #if DEBUG
    true
    #else
    false
    #endif

/// Type that represents data which is loaded from an external source. Initially the process of retrieving that data
/// is `HasNotStartedYet`, then when data is loading, the state should become `InProgress`. After some delay
/// the data becomes available in the `Resolved` state.
type Deferred<'t> =
  | HasNotStartedYet
  | InProgress
  | Resolved of 't

/// Utility functions around `Deferred<'T>` types.
module Deferred =
    let map (transform: 'T -> 'U) (deferred: Deferred<'T>) : Deferred<'U> =
        match deferred with
        | HasNotStartedYet -> HasNotStartedYet
        | InProgress -> InProgress
        | Resolved value -> Resolved (transform value)

    /// Returns whether the `Deferred<'T>` value has been resolved or not.
    let resolved = function
        | HasNotStartedYet -> false
        | InProgress -> false
        | Resolved _ -> true

    /// Returns whether the `Deferred<'T>` value is in progress or not.
    let inProgress = function
        | HasNotStartedYet -> false
        | InProgress -> true
        | Resolved _ -> false

    /// Verifies that a `Deferred<'T>` value is resolved and the resolved data satisfies a given requirement.
    let exists (predicate: 'T -> bool) = function
        | HasNotStartedYet -> false
        | InProgress -> false
        | Resolved value -> predicate value

    /// Like `map` but instead of transforming just the value into another type in the `Resolved` case, it will transform the value into potentially a different case of the the `Deferred<'T>` type.
    let bind (transform: 'T -> Deferred<'U>) (deferred: Deferred<'T>) : Deferred<'U> =
        match deferred with
        | HasNotStartedYet -> HasNotStartedYet
        | InProgress -> InProgress
        | Resolved value -> transform value

type AsyncOperationStatus<'t> =
  | Started
  | Finished of 't

module Log =
    /// Logs error to the console during development
    let developmentError (error: exn) =
        if isDevelopment
        then Browser.Dom.console.error(error)

module Cmd =
    /// Converts an asynchronous operation that returns a message into into a command of that message.
    /// Logs unexpected errors to the console while in development mode.
    let fromAsync (operation: Async<'msg>) : Cmd<'msg> =
        let delayedCmd (dispatch: 'msg -> unit) : unit =
            let delayedDispatch = async {
                match! Async.Catch operation with
                | Choice1Of2 msg -> dispatch msg
                | Choice2Of2 error -> Log.developmentError error
            }

            Async.StartImmediate delayedDispatch

        Cmd.ofSub delayedCmd

[<RequireQualifiedAccess>]
module StaticFile =

    open Fable.Core.JsInterop

    /// Function that imports a static file by it's relative path. Ignores the file when compiled for mocha tests.
    let inline import (path: string) : string =
        #if !MOCHA_TESTS
        importDefault<string> path
        #else
        path
        #endif

[<RequireQualifiedAccess>]
module Config =
    open System
    open Fable.Core

    /// Returns the value of a configured variable using its key.
    /// Retursn empty string when the value does not exist
    [<Emit("process.env[$0] ? process.env[$0] : ''")>]
    let variable (key: string) : string = jsNative

    /// Tries to find the value of the configured variable if it is defined or returns a given default value otherwise.
    let variableOrDefault (key: string) (defaultValue: string) =
        let foundValue = variable key
        if String.IsNullOrWhiteSpace foundValue
        then defaultValue
        else foundValue