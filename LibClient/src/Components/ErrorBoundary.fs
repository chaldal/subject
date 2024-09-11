[<AutoOpen>]
module LibClient.Components.ErrorBoundary

open Fable.React
open LibClient

[<RequireQualifiedAccess>]
type private State =
| Trying
| Caught of System.Exception

type private Props =
    {
        Try: ReactElement
        Catch: (System.Exception * (unit -> unit)) -> ReactElement
        key: Option<string>
    }

// Using a class-based component rather than a function-based one because React does not yet have a hook equivalent for componentDidCatch.
type private ErrorBoundaryComponent(initialProps: Props) as this =
    inherit Fable.React.PureComponent<Props, State>(initialProps)

    do
        this.setInitState State.Trying

    member this.Reset() =
        this.setState(fun _ _ -> State.Trying)

    override this.componentDidCatch (error, _errorInfo) : unit =
        this.setState(fun _ _ -> State.Caught error)

    override this.render() =
        match this.state with
        | State.Trying -> this.props.Try
        | State.Caught error -> this.props.Catch (error, this.Reset)

type LibClient.Components.Constructors.LC with
    static member ErrorBoundary(
                ``try``: ReactElement,
                catch: (System.Exception * (unit -> unit)) -> ReactElement,
                ?key: string
            ) : ReactElement =
        let props =
            {
                Try = ``try``
                Catch = catch
                key = key
            }
        Fable.React.Helpers.ofType<ErrorBoundaryComponent,_,_> props Seq.empty