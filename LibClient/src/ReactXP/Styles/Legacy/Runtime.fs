namespace ReactXP.LegacyStyles

open ReactXP.Styles.Types
open ReactXP.Styles.Animation

open Fable.Core.JsInterop
open System.Text.RegularExpressions

open LibClient

module Runtime =
    // Whether the instantiator of a component can affect the styles of its top level
    // block should be up to the container itself. So a container can explicitly declare
    // to have `class="{TopLevelBlockClass}"` to opt in. This way, settings like margin
    // can be applied from the outside by providing RuntimeStyles.Rules. Any more internal
    // style customizations have to be done by providing a RuntimeStyles.Sheet via a themeing
    // function provided by the component itself.
    let TopLevelBlockClass = "__TopLevelBlock"

    let private whitespaceRegex = Regex("\\s+")

    let findApplicableStyles (styles: List<RuntimeStyles>) (rawClassString: string) : List<RuntimeStyles> =
        let classes: Set<string> = whitespaceRegex.Split(rawClassString) |> Set.ofArray

        let matchingStyles =
            styles
            |> List.collect
                (fun currStyles ->
                    match currStyles with
                    | RuntimeStyles.Sheet pairs ->
                        pairs
                        |> List.filterMap
                            (fun (selector, styles) ->
                                match Set.isSubset selector classes with
                                | false -> None
                                | true  -> Some styles
                            )
                    | _ ->
                        match classes.Contains TopLevelBlockClass with
                        | true  -> [currStyles]
                        | false -> []
                )

        matchingStyles

    let findTopLevelBlockStyles (styles: List<RuntimeStyles>) : List<RuntimeStyles> =
        styles
        |> List.filterMap (function
            | RuntimeStyles.Sheet _ -> None
            | currStyles            -> Some currStyles
        )

    let mergeComponentAndPropsStyles (componentStyles: RuntimeStyles) (props: obj) : List<RuntimeStyles> =
        match props?__style with
        | Some propsStyles -> List.append [componentStyles] propsStyles
        | None             -> [componentStyles]

    [<RequireQualifiedAccess>]
    type AnimatedComponentPropStyle =
    | StaticRules        of ReactXPStyleRulesObject
    | AnimatedRules      of ClassName * AnimatedRulesConstructor
    | AnimatedAnimations of ClassName * AnimatedAnimationsConstructor

    type AnimatedComponentPropStyles = List<AnimatedComponentPropStyle>

    let prepareStylesForPassingToReactXpComponent<'T> (fullyQualifiedComponentName: string) (styles: List<RuntimeStyles>) : 'T =
        match fullyQualifiedComponentName with
        | "ReactXP.Components.AniView"
        | "ReactXP.Components.AniText"
        | "ReactXP.Components.AniTextInput"
        | "ReactXP.Components.AniImage" ->
            // these components unpack styles internally, because they
            // need to construct instance-bound animation styles. This is done inside
            // each of these components' render methods, which delegate to
            // AnimationManager.parepareStylesForPassingThroughProps
            styles
            |> List.filterMap (function
                | RuntimeStyles.None ->
                    None
                | RuntimeStyles.Sheet _ ->
                    Log.Error("Found RuntimeStyles.Sheet passed to an animated base ReactXP component, which is meaningless. Exception: {exception}", exceptionForStackTrace)
                    None
                | RuntimeStyles.AnimatedRules (className, rulesConstructor) ->
                    AnimatedComponentPropStyle.AnimatedRules (className, rulesConstructor) |> Some
                | RuntimeStyles.AnimatedAnimations (className, animationsConstructor) ->
                    AnimatedComponentPropStyle.AnimatedAnimations (className, animationsConstructor) |> Some
                | RuntimeStyles.StaticRules lazyValue ->
                    AnimatedComponentPropStyle.StaticRules (lazyValue.CreateForReactXpComponent fullyQualifiedComponentName) |> Some
            )
            :> obj :?> 'T

        | _ ->
            styles
            |> List.filterMap (fun currStyles ->
                match currStyles with
                | RuntimeStyles.Sheet _
                | RuntimeStyles.AnimatedRules _
                | RuntimeStyles.AnimatedAnimations _ ->
                    Log.Error("Found RuntimeStyles.{currStyles} passed to a base ReactXP component. Use animated ReactXP components instead. Exception: {exception}", (unionCaseName currStyles), exceptionForStackTrace)
                    None
                | RuntimeStyles.None -> None
                | RuntimeStyles.StaticRules lazyValue ->
                    Some (lazyValue.CreateForReactXpComponent fullyQualifiedComponentName :> obj)
            )
            |> Array.ofList
            :> obj :?> 'T

    let injectImplicitProps (implicitProps: List<string * obj>) (props: 'Props) : 'Props =
        LibClient.JsInterop.extendRecord implicitProps props

    let extractReactXpStyleValue (key: string) (reactXpStyles: array<obj>) : Option<obj> =
        reactXpStyles
        |> Seq.rev
        |> Seq.findMap (fun rules ->
            let maybeValue = rules?(key)
            if maybeValue |> isNullOrUndefined then None else Some maybeValue
        )

    module AnimationManager =
        type Animation (xpAnimation: RawAnimation) =
            let mutable isRunning: bool = false
            member _.Start () : unit =
                if isRunning then
                    xpAnimation.stop()

                isRunning <- true
                xpAnimation.start(Some (fun () -> isRunning <- false))

        type InstanceAnimationDataStore = {
            Values:     Map<string, RawAnimatedValue>
            Rules:      Map<ClassName, ReactXPStyleRulesObject>
            Animations: Map<ClassName, Animation>
        } with
            static member Empty : InstanceAnimationDataStore = {
                Values     = Map.empty
                Rules      = Map.empty
                Animations = Map.empty
            }

        let mutable private instanceStores: Map<System.Guid, InstanceAnimationDataStore> = Map.empty

        let clearData (instanceId: System.Guid) : unit =
            instanceStores <- instanceStores.Remove instanceId

        let private getInstanceStore (instanceId: System.Guid) : InstanceAnimationDataStore =
            match instanceStores.TryFind instanceId with
            | Some store -> store
            | None ->
                let store = InstanceAnimationDataStore.Empty
                instanceStores <- instanceStores.Add (instanceId, store)
                store

        let private getOrCreateValue (instanceId: System.Guid) (key: string) (initialValue: double) : RawAnimatedValue =
            let store = getInstanceStore instanceId
            match store.Values.TryFind key with
            | Some value -> value
            | None ->
                let value = AnimatedValue.Create(initialValue).Raw
                instanceStores <- instanceStores.AddOrUpdate(instanceId, { store with Values = store.Values.Add(key, value) })
                value

        let private getOrCreateRules (instanceId: System.Guid) (className: string) (constructor: AnimatedRulesConstructor) : ReactXPStyleRulesObject =
            match (getInstanceStore instanceId).Rules.TryFind className with
            | Some rules -> rules
            | None ->
                let rules = constructor (getOrCreateValue instanceId)
                // NOTE it's important we get a fresh copy here, because constructor above
                // may have created new values
                let store = getInstanceStore instanceId
                instanceStores <- instanceStores.AddOrUpdate(instanceId, { store with Rules = store.Rules.Add(className, rules) })
                rules

        let private getOrCreateAnimations (instanceId: System.Guid) (className: string) (constructor: AnimatedAnimationsConstructor) : Animation =
            // It seems that we cannot reuse animations because they are not clearnly
            // reset after stopping in the ReactNative implementation
            // match (getInstanceStore instanceId).Animations.TryFind className with
            // | Some animation -> animation
            // | None ->
                let xpAnimation = constructor (getOrCreateValue instanceId)
                let animation = Animation xpAnimation
                // NOTE it's important we get a fresh copy of the store here, because constructor above
                // may have created new values
                let store = getInstanceStore instanceId
                instanceStores <- instanceStores.AddOrUpdate(instanceId, { store with Animations = store.Animations.Add(className, animation) })
                animation

        let parepareStylesForPassingThroughProps (instanceId: System.Guid) (styles: AnimatedComponentPropStyles) : obj =
            styles
            |> List.filterMap (function
                | AnimatedComponentPropStyle.AnimatedRules (className, constructor) ->
                    (getOrCreateRules instanceId className constructor) :> obj |> Some
                | AnimatedComponentPropStyle.AnimatedAnimations (className, constructor) ->
                    (getOrCreateAnimations instanceId className constructor) |> ignore
                    None
                | AnimatedComponentPropStyle.StaticRules rawValue ->
                    rawValue :> obj |> Some
            )
            |> Array.ofList
            :> obj

        let getClassesFromStyleProp (styles: List<RuntimeStyles>) : Set<string> =
            styles
            |> List.filterMap (function
                | RuntimeStyles.None                         -> None
                | RuntimeStyles.Sheet _                      -> None
                | RuntimeStyles.StaticRules _                -> None
                | RuntimeStyles.AnimatedRules (className, _) -> Some className
                | RuntimeStyles.AnimatedAnimations _         -> None // animations without rules won't be applied anyway
            )
            |> Set.ofList

        let startAnimations (instanceId: System.Guid) (classes: Set<string>) : unit =
            let store = getInstanceStore instanceId
            classes
            |> Seq.filterMap (store.Animations.TryFind)
            |> Seq.iter (fun animation ->
                animation.Start ()
            )
