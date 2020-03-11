module Shared

/// Defines how routes are generated on server and mapped from client
let routerPaths typeName method = sprintf "/api/%s" method

type Counter = { value : int }

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type IServerApi = {
    Counter : unit -> Async<Counter>
}