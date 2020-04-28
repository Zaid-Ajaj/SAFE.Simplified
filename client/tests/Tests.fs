module Tests

open Fable.Mocha
open App

let appTests = testList "App tests" [
    testCase "Increment and Decrement work" <| fun _ ->
        // Simplified update that ignores commands/effects
        let update state msg = fst (App.update msg state)
        let initialState : App.State = { Counter = Resolved (Ok { value = 0 }) }
        let incomingMsgs =  [ Increment; Increment; Decrement; Increment ]
        let updatedState = List.fold update initialState incomingMsgs
        Expect.isTrue (Deferred.resolved updatedState.Counter) "Counter must be resolved"

        let counterHasValue n =
            updatedState.Counter
            |> Deferred.exists (function
                | Ok counter -> counter.value = n
                | Error _ -> false)

        Expect.isTrue (counterHasValue 2) "Expected updated state to be 2"
]

let allTests = testList "All" [
    appTests
]

[<EntryPoint>]
let main (args: string[]) = Mocha.runTests allTests