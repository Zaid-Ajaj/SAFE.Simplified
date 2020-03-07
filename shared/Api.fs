module Shared

let routerPaths typeName method = sprintf "/api/counter/%s" method

type Counter = { value : int }

type ICounterApi = {
    getCounter : unit -> Async<Counter>
}