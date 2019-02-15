module TaxCalculation

open Prelude

[<AutoOpen>]
module Common =
    type NonnegativeRur =
        private
        | Nonnegative of Rur

    let (|Nonnegative|) = function Nonnegative rur -> rur

    let tryCreateNonnegativeRur rur =
        if rur >= Rur.zero
            then rur |> Nonnegative |> Ok
            else Error "rur < 0"

    type SelfInsuranceContributions =
        {   Pfr     : NonnegativeRur
            Ffoms   : NonnegativeRur
        }

    type EmployeeInsuranceContributions =
        {   Pfr     : NonnegativeRur
            Ffoms   : NonnegativeRur
            Fss     : NonnegativeRur
        }

type TaxParameters =
    {   Income  : NonnegativeRur
        Expense : NonnegativeRur
        Salary  : NonnegativeRur
    }

type TaxSystem =
    | IndividualUsnD        of usnRate: Rate
    | IndividualUsnDR       of usnRate: Rate
    | OrganizationUsnD      of usnRate: Rate

[<AutoOpen>]
module Helpers =
    let calculateUsnChargedTax usnRate aggregate params' =
        let {   Income  = (Nonnegative income)
                Expense = (Nonnegative expense);
                Salary  = (Nonnegative salary) } = params'
 
        match aggregate with
        | IndividualUsnD _
        | OrganizationUsnD _    -> usnRate >* income

        | IndividualUsnDR _     -> usnRate >* (income -. expense)


        
        
