module Server

open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open Shared

/// An implementation of the Shared IServerApi protocol.
/// Can require ASP.NET injected dependencies in the constructor and uses the Build() function to return value of `IServerApi`.
type ServerApi(logger: ILogger<ServerApi>, config: IConfiguration) =
    member this.Counter() =
        async {
            logger.LogInformation("Executing {Function}", "counter")
            do! Async.Sleep 1000
            return { value = 10 }
        }

    member this.Build() : IServerApi =
        {
            Counter = this.Counter
        }