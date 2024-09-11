namespace LibClient
open Fable.React

type ComponentRegistry() =
    static let renderDirectory = System.Collections.Generic.Dictionary<string, obj>()
    static let stylesDirectory = System.Collections.Generic.Dictionary<string, Lazy<ReactXP.LegacyStyles.RuntimeStyles>>()

    static member RegisterRender<'Props, 'EState, 'PState, 'Actions> (fullyQualifiedComponentName: string) (renderFunction: array<ReactElement> * 'Props * 'EState * 'PState * 'Actions * ReactXP.LegacyStyles.RuntimeStyles -> ReactElement) =
        renderDirectory.[fullyQualifiedComponentName] <- renderFunction

    static member RegisterStyles (fullyQualifiedComponentName: string, styles: Lazy<ReactXP.LegacyStyles.RuntimeStyles>) =
        stylesDirectory.[fullyQualifiedComponentName] <- styles

    static member GetRender<'Props, 'EState, 'PState, 'Actions> (fullyQualifiedComponentName: string): array<ReactElement> * 'Props * 'EState * 'PState * 'Actions * ReactXP.LegacyStyles.RuntimeStyles -> ReactElement =
        match renderDirectory.TryGetValue fullyQualifiedComponentName with
        | (true, renderFunction) -> renderFunction :?> (array<ReactElement> * 'Props * 'EState * 'PState * 'Actions * ReactXP.LegacyStyles.RuntimeStyles -> ReactElement)
        | _ -> failwith (sprintf "No render function for %s (did you call registerAllTheThings in Bootstrap.fs?)" fullyQualifiedComponentName)

    static member GetStyles (fullyQualifiedComponentName: string): ReactXP.LegacyStyles.RuntimeStyles =
        match stylesDirectory.TryGetValue fullyQualifiedComponentName with
        | (true, styles) -> styles.Value
        | _ -> failwith (sprintf "No styles registered in the ComponentRegistry for %s (are you passing true for `hasStyles` where in reality you don't have any?)" fullyQualifiedComponentName)

type Themes() =
    static let mutable values: Map<string, obj> = Map.empty

    static member inline Set(value: 'T) : unit =
        Themes.ActuallySet (typeof<'T>.FullName, value)

    static member ActuallySet(typeFullName: string, value: 'T) : unit =
        values <- values.AddOrUpdate(typeFullName, value)

    static member inline GetMaybeUpdatedWith<'T>(maybeUpdater: Option<'T -> 'T>) : 'T =
        Themes.ActuallyGetMaybeUpdatedWith (typeof<'T>.FullName, maybeUpdater)

    static member ActuallyGetMaybeUpdatedWith<'T>(typeFullName: string, maybeUpdater: Option<'T -> 'T>) : 'T =
        let value =
            match values.TryFind typeFullName with
            | Some value -> value :?> 'T
            | None -> failwith $"Not found the default theme for {typeFullName}"

        match maybeUpdater with
        | None -> value
        | Some updater -> updater value
