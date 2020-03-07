module Retry

let rec retry n f =
    if n = 0 then
        ignore()
    else
        try f()
        with ex ->
            System.Threading.Thread.Sleep(1000)
            retry (n - 1) f