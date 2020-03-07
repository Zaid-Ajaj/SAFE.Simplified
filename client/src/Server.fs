module Server

open Fable.Remoting.Client

let api = 
    Remoting.createApi()
    |> Remoting.withRouteBuilder Shared.routerPaths
    |> Remoting.buildProxy<Shared.ICounterApi>
