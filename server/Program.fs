module Program

open Saturn
open Giraffe
open Shared
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration

let counter (logger: ILogger<IServerApi>) =
    async {
        logger.LogInformation("Executing {Function}", "counter")
        do! Async.Sleep 1000
        return { value = 10 }
    }

/// Composition root of the `IServerApi`, resolves required dependencies from
/// ASP.NET's injected services to construct the API.
///
/// Read https://zaid-ajaj.github.io/Fable.Remoting/src/dependency-injection.html to learn more.
let serverApi = reader {
    // resolve injected services (logger and config in this case)
    let! logger = resolve<ILogger<IServerApi>>()
    let! config = resolve<IConfiguration>()

    // use logger
    let counter() = counter logger
    // construct typed API
    let serverApi : IServerApi = {
        counter = counter
    }

    return serverApi
}

let webApi =
    Remoting.createApi()
    |> Remoting.fromReader serverApi
    |> Remoting.withRouteBuilder routerPaths
    |> Remoting.buildHttpHandler

let webApp = choose [ webApi; GET >=> text "Hello to full STACK F#" ]

let serviceConfig (services: IServiceCollection) =
    services.AddLogging()

let application = application {
    use_router webApp
    use_static "wwwroot"
    use_gzip
    use_iis
    service_config serviceConfig
    host_config Env.configureHost
}

run application