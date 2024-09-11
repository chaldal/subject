[<AutoOpen>]
module LibLifeCycleHost.ViewAdapter

open System
open LibLifeCycle

type IViewAdapter =
    abstract member View:            IView
    abstract member ViewName:        string
    abstract member EnableApiAccess: bool

type ViewAdapter<'Input, 'Output, 'OpError when 'OpError :> OpError> = {
    View: IView<'Input, 'Output, 'OpError>
}
with
    interface IViewAdapter with
        member this.View = this.View
        member this.ViewName = this.View.Name
        member this.EnableApiAccess = this.View.EnableApiAccess

type ViewAdapterCollection = ViewAdapterCollection of Map<string, IViewAdapter>
    with
        interface System.Collections.Generic.IEnumerable<IViewAdapter> with
            member this.GetEnumerator(): Collections.Generic.IEnumerator<IViewAdapter> =
                let (ViewAdapterCollection dictionary) = this
                dictionary.Values.GetEnumerator()

            member this.GetEnumerator(): Collections.IEnumerator =
                let (ViewAdapterCollection dictionary) = this
                dictionary.Values.GetEnumerator() :> Collections.IEnumerator

        member this.GetViewAdapterByName name : Option<IViewAdapter> =
            match this with
            | ViewAdapterCollection dictionary ->
                match dictionary.TryGetValue name with
                | true, adapter -> Some adapter
                | false, _ -> None