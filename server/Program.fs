module Program

open Saturn
open Giraffe
open Shared
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration

/// Function that has dependency on ASP.NET Core logger
let getCounter (logger: ILogger<ICounterApi>) =
    async {
        logger.LogInformation("Executing {Function}", "getCounter")
        do! Async.Sleep 1000
        return { value = 10 }
    }

/// Composition root of the `ICounterApi`, resolves required dependencies from
/// ASP.NET's injected services to construct the API.
///
/// Read https://zaid-ajaj.github.io/Fable.Remoting/src/dependency-injection.html to learn more.
let counterApi =
    reader {
        // resolve injected services (logger and config in this case)
        let! logger = resolve<ILogger<ICounterApi>>()
        let! config = resolve<IConfiguration>()

        // use logger
        let getCounter() = getCounter logger
        // construct typed API
        let counterApi : ICounterApi = {
            getCounter = getCounter
        }

        return counterApi
    }

let webApi =
    Remoting.createApi()
    |> Remoting.fromReader counterApi
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