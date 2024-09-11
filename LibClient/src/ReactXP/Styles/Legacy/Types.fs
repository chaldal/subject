namespace ReactXP.LegacyStyles

open Fable.Core
open Fable.Core.JsInterop
open LibClient

open ReactXP.Styles.Types
open ReactXP.Styles.Animation

module Config =
    let mutable private isDevMode: bool = false
    let getIsDevMode () : bool = isDevMode
    let setIsDevMode (value: bool) =
        isDevMode <- value

// "&&"-separated class names
type Selector = string

type ClassName = string

[<RequireQualifiedAccess>]
type StyleRuleType =
| Text
| Flexbox
| View
| Transform

[<RequireQualifiedAccess>]
type RawReactXPStyleRule =
| WeOnlyWantOurHelperFunctionsToProduceThese

type TypedReactXPStyleRule = RawReactXPStyleRule * StyleRuleType

type LazilyCreatedReactXpStyleObject (typedValues: seq<TypedReactXPStyleRule>) =
    // NOTE since we do our own caching of styles, by associating them with class names,
    // we completely disable caching on the ReactXP side. This also avoids the annoying
    // run-ins we have with their leak detection system (because of how their fingerprinting
    // mechanism doesn't work when all your createStyle calls come from the same code location.
    let shouldCacheReactXpInternal = false


    [<Emit("(new Error().stack || \"\").toString()")>]
    static let generateStackTrace () : string = jsNative

    // NOTE this is necessary in dev to help developers find where faulty rules they've written are.
    let maybeStackTrace: Option<string> = if Config.getIsDevMode() then Some (generateStackTrace()) else None

    static let fullyQualifiedComponentNameToAllowedTypes = Map.ofList [
        ("ReactXP.Components.Text",      Set.ofList [StyleRuleType.Text; StyleRuleType.Flexbox; StyleRuleType.View; StyleRuleType.Transform])
        ("ReactXP.Components.UiText",    Set.ofList [StyleRuleType.Text; StyleRuleType.Flexbox; StyleRuleType.View; StyleRuleType.Transform])
        ("ReactXP.Components.TextInput", Set.ofList [StyleRuleType.Text; StyleRuleType.Flexbox; StyleRuleType.View; StyleRuleType.Transform])
    ]
    static let allowedTypesFallback = Set.ofList [StyleRuleType.Flexbox; StyleRuleType.View; StyleRuleType.Transform]

    let mutable cache: Map<(* fullyQualifiedComponentName *) string, ReactXPStyleRulesObject> = Map.empty

    member this.GetRawStyleRules : seq<RawReactXPStyleRule> =
        typedValues
        |> Seq.map fst

    member this.CreateForReactXpComponent (fullyQualifiedComponentName: string)  : ReactXPStyleRulesObject =
        match cache.TryFind fullyQualifiedComponentName with
        | Some result -> result
        | None -> this.ReallyCreateForReactXpComponentAndCache fullyQualifiedComponentName

    member private _.ReallyCreateForReactXpComponentAndCache (fullyQualifiedComponentName: string) : ReactXPStyleRulesObject =
        let allowedTypes = fullyQualifiedComponentNameToAllowedTypes.TryFind fullyQualifiedComponentName |> Option.getOrElse allowedTypesFallback

        let allowedRules =
            typedValues
            |> Seq.filterMap
                (fun (currRule, currType) ->
                    if allowedTypes.Contains currType then
                        Some currRule
                    else
                        Log.Error ("Disallowed rule {currRule} of type {currType} detected for component {fullyQualifiedComponentName}. It's been filtered out, but you must go up and clean up whatever was causing it. Fingerprint that may help you locate the rule: {maybeStackTrace}", currRule, currType, fullyQualifiedComponentName, maybeStackTrace)
                        None
                )

        let typelessAllowedRules = allowedRules :?> seq<string * obj>

        // NOTE TO ANYBODY WHO DEBUGS THIS METHOD
        // the createViewStyle method and its friends actually mutate the
        // value that's passed in, so if you break consecutive values out
        // and try to examine their values just before the return at the end
        // of the function, you may be confused that your object doesn't have
        // the fields you expect it to have.
        let result: ReactXPStyleRulesObject =
            match fullyQualifiedComponentName with
            | "ReactXP.Components.Text"
            | "ReactXP.Components.UiText" ->
                ReactXP.Helpers.ReactXPRaw?Styles?createTextStyle(createObj typelessAllowedRules, shouldCacheReactXpInternal)
            | _ -> ReactXP.Helpers.ReactXPRaw?Styles?createViewStyle(createObj typelessAllowedRules, shouldCacheReactXpInternal)

        cache <- cache.Add (fullyQualifiedComponentName, result)

        result

[<RequireQualifiedAccess>]
type RuleFunctionReturnedStyleRules =
| One  of TypedReactXPStyleRule
| Many of array<RuleFunctionReturnedStyleRules>

and [<RequireQualifiedAccess>] Styles =
| Sheet of List<Selector * Styles>
| Rules of LazilyCreatedReactXpStyleObject

// Union types don't work for what we need to do here because
// they don't allow a function to return just one case type.
[<AbstractClass>]
type ISheetBuildingBlock () =
    member this.Match<'T> (oneCase: SheetBuildingBlockOne -> 'T) (manyCase: SheetBuildingBlockMany -> 'T) : 'T =
        match this with
        | :? SheetBuildingBlockOne  as oneBlock  -> oneCase  oneBlock
        | :? SheetBuildingBlockMany as manyBlock -> manyCase manyBlock
        | _ -> failwith "Did you create another subclass of ISheetBuildingBlock and forget to include it here? Standard inheritance doesn't give us exhaustiveness checks."

and SheetBuildingBlockOne (selector: Selector, styles: Styles) =
    inherit ISheetBuildingBlock ()
    member _.Selector = selector
    member _.Styles   = styles
    member _.ToTuple  = (selector, styles)
and SheetBuildingBlockMany (blocks: List<Selector * Styles>) =
    inherit ISheetBuildingBlock ()
    member _.Blocks = blocks

open Fable.Core.JsInterop


[<RequireQualifiedAccess>]
type RuntimeStyles =
| Sheet              of List<Set<ClassName> * RuntimeStyles>
| StaticRules        of LazilyCreatedReactXpStyleObject
| AnimatedRules      of ClassName * AnimatedRulesConstructor
| AnimatedAnimations of ClassName * AnimatedAnimationsConstructor
| None
with
    static member MergeSheets (a: RuntimeStyles) (b: RuntimeStyles) : RuntimeStyles =
        match (a, b) with
        | (RuntimeStyles.Sheet sheetA, RuntimeStyles.Sheet sheetB) -> RuntimeStyles.Sheet (List.append sheetA sheetB)
        | _ -> failwith "Can only merge RuntimeStyle.Sheet instances"

    static member FixmeCrappyStyleSharing (a: Lazy<RuntimeStyles>) (b: Lazy<RuntimeStyles>) : Lazy<RuntimeStyles> =
        lazy (RuntimeStyles.MergeSheets a.Value b.Value)

// for documentation only
type ComponentStylePropValue = List<RuntimeStyles>

type Color = LibClient.ColorModule.Color
