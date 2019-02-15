module Prelude
open System

type Rate = private Rate of decimal

module Rate =
    let tryCreate koeff =
        if (koeff >= 0M)
            then Ok (Rate koeff)
            else Error "koeff < 0"

type Rur =
    private
    | Rur of decimal

    static member (+.) (Rur lamount, Rur ramount) = Rur (lamount + ramount)
    static member (-.) (Rur lamount, Rur ramount) = Rur (lamount - ramount)
    static member (>*) (Rate koeff, Rur amount) = Rur (koeff * amount)

module Rur =
    let create amount = Rur (Math.Round(amount * 100M) / 100M)

    let min (Rur lamount) (Rur ramount) =
        if lamount <= ramount then (create lamount) else (create ramount)
    
    let max (Rur lamount) (Rur ramount) =
        if lamount <= ramount then (create ramount) else (create lamount)

    let zero = Rur 0M

 

