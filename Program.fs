// Learn more about F# at http://fsharp.org

open System
open Prelude

[<EntryPoint>]
let main argv =

    dict [ (1, "a"); (2, "b"); (2, "c") ] |> printfn "%A"

    0 // return an integer exit code
