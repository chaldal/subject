namespace LibUiSubject.Components

open LibClient
open Fable.React
open LibClient.Components
open LibUiSubject
open LibUiSubject.Components.With.View
open Fable.Core.JsInterop

// Don't warn about incorrect usage of PascalCased function parameter names
#nowarn "0049"

[<AutoOpen>]
module With_ViewTypeExtensions =
    type LibUiSubject.Components.Constructors.UiSubject.With with
        static member View(service: LibUiSubject.Services.ViewService.IViewService<'Input, 'Output, 'OpError>, input: 'Input, ``with``: WithView<'Output>, ?children: ReactChildrenProp, ?useCache: UseCache, ?key: string, ?xLegacyStyles: List<ReactXP.LegacyStyles.RuntimeStyles>) =
            let __props =
                {
                    Service = service
                    Input = input
                    With = ``with``
                    UseCache = defaultArg useCache (UseCache.No)
                    key = key |> Option.orElse (JsUndefined)
                }
            match xLegacyStyles with
            | Option.None | Option.Some [] -> ()
            | Option.Some styles -> __props?__style <- styles
            LibUiSubject.Components.With.View.Make
                __props
                (Option.map tellReactArrayKeysAreOkay children |> Option.getOrElse [||])
            