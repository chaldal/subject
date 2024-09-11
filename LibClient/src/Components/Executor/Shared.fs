[<AutoOpen>]
module LibClient.Components.Executor_Shared

open LibClient

module LC =
    module Executor =
        [<RequireQualifiedAccess>]
        type ShowTopLevelSpinnerForKeys =
        | All
        | Some of Set<UDActionKey>