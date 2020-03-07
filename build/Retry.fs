module Retry

let rec retry f n = 
    if n = 0 then 
        ignore()
    else 
        try f()
        with ex -> 
            System.Threading.Thread.Sleep(1000)
            retry f (n - 1)