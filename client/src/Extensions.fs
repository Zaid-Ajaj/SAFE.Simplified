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

    /// returns whether the `Deferred<'T>` has been resolved or not.
    let isResolved = function
        | HasNotStartedYet -> false
        | InProgress -> false
        | Resolved _ -> true

    /// Verifies that a `Deferred<'T>` value is resolved and the resolved data satisfies a given requirement.
    let exists (predicate: 'T -> bool) = function
        | HasNotStartedYet -> false
        | InProgress -> false
        | Resolved value -> predicate value

type AsyncOperationStatus<'t> =
  | Started
  | Finished of 't

module Cmd =
    /// Converts an asynchronous operation that returns a message into into a command of that message.
    /// Logs unexpected errors to the console while in development mode.
    let fromAsync (operation: Async<'msg>) : Cmd<'msg> =
        let delayedCmd (dispatch: 'msg -> unit) : unit =
            let delayedDispatch = async {
                match! Async.Catch operation with
                | Choice1Of2 msg -> dispatch msg
                | Choice2Of2 error -> if isDevelopment then Browser.Dom.console.log(error)
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
    open Fable.Core.JsInterop
    open Fable.Core

    /// Returns the value of a configured variable using its key
    [<Emit("process.env[$0]")>]
    let inline variable (key: string) : string = jsNative