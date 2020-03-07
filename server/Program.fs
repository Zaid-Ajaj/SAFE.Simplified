module Program

open Saturn
open Giraffe
open Shared
open Fable.Remoting.Server
open Fable.Remoting.Giraffe

let getCounter() = async {
    do! Async.Sleep 1000
    return { value = 10 }
}

let counterApi : ICounterApi = {
    getCounter = getCounter
}

let webApi =
    Remoting.createApi()
    |> Remoting.fromValue counterApi
    |> Remoting.withRouteBuilder routerPaths
    |> Remoting.buildHttpHandler

let webApp = choose [ webApi; GET >=> text "Hello to full STACK F#" ]

let application = application {
    use_router webApp
    use_static "wwwroot"
}

run application