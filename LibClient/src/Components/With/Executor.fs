[<AutoOpen>]
module LibClient.Components.With_Executor

open Fable.React
open ReactXP.Styles
open LibClient
open LibClient.Components
open LibClient.UniDirectionalDataFlow

type LC.With with
    /// <summary>An ad hoc block with manual executor usage that handles the in-progress state using LC.InProgress.
    /// TODO: plumb through the LC.InProgress theme when implemented</summary>
    /// <param name="executor" type="LibClient.UniDirectionalDataFlow.Executor"/>
    /// <param name="content" type="LibClient.UniDirectionalDataFlow.Executor -> array&lt;ReactElement&gt;"/>
    /// <param name="styles" type="array&lt;ViewStyles&gt;" default="[||]"/>
    /// <example>
    /// Basics
    /// <code>
    ///     LC.Executor.AlertErrors (fun makeExecutor -> element {
    ///         LC.With.Executor (makeExecutor "test", fun executor -> [|
    ///             RX.View (
    ///                 onPress = (fun _ -> executor.MaybeExecute action),
    ///                 children = [|
    ///                     LC.InfoMessage "Press Here"
    ///                 |]
    ///             )
    ///         |])
    ///     })
    /// </code>
    /// </example>
    ///
    /// <remarks>
    ///     Setup code
    ///     <code setup="true">
    ///         let private action () : UDActionResult = async {
    ///             do! Async.Sleep (System.TimeSpan.FromSeconds 1)
    ///             return Ok ()
    ///         }
    ///     </code>
    /// </remarks>

    [<Component>]
    static member Executor (executor: Executor, content: Executor -> array<ReactElement>, ?styles: array<ViewStyles>) : ReactElement =
        LC.InProgress (
            ?styles = styles,
            isInProgress = executor.IsInProgressNow,
            children = elements {
                content executor
            }
        )

    static member Executor (executor: Executor, content: Executor -> ReactElement, ?styles: array<ViewStyles>) : ReactElement =
        LC.InProgress (
            ?styles = styles,
            isInProgress = executor.IsInProgressNow,
            children = elements {
                content executor
            }
        )
