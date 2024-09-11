[<AutoOpen>]
module ReactXP.LegacyStyles.Designtime

open ReactXP.Styles.Types
open ReactXP.LegacyStyles
open Fable.Core.JsInterop
open ReactXP.Helpers

open LibClient

// it's not recommended to override the && operator, but we're building a DSL here
#nowarn "0086"


(*
    Requirements:
    * To leaf ReactXp components, we need to pass either a single object
      of style names to values, or an array of such objects
    * To non-leaf components, we need to pass style sheets, i.e. mappings
      from selector to aforementioned style name to value objects
    * transformation from whatever helper types to raw ReactXp types needs
      to happen as early and infrequently as possible, i.e. only at style
      sheet definition time, not application time. Ideally it would actually
      be at compile time (though reliance on runtime values won't allow for this).
 *)

let rec flattenStyleRules (rules: seq<RuleFunctionReturnedStyleRules>) : List<TypedReactXPStyleRule> =
    let rec loop (acc: List<TypedReactXPStyleRule>) (remainingInput: List<RuleFunctionReturnedStyleRules>) =
        match remainingInput with
        | [] -> List.rev acc
        | r :: rs ->
            match r with
            | RuleFunctionReturnedStyleRules.Many currRules -> loop (List.append (flattenStyleRules currRules) acc) rs
            | RuleFunctionReturnedStyleRules.One  currRule  -> loop (currRule :: acc) rs

    loop [] (rules |> List.ofSeq)


let rec private flattenStyleRulesToArrayHelper (rules: array<RuleFunctionReturnedStyleRules>, acc: array<TypedReactXPStyleRule>) : unit =
    for i in 0 .. rules.Length - 1 do
        match rules[i] with
        | RuleFunctionReturnedStyleRules.Many currRules ->
            flattenStyleRulesToArrayHelper (currRules, acc)
        | RuleFunctionReturnedStyleRules.One  currRule  ->
            acc[acc.Length] <- currRule

let rec flattenStyleRulesToArray (rules: array<RuleFunctionReturnedStyleRules>) : array<TypedReactXPStyleRule> =
   let result: array<TypedReactXPStyleRule> = [||]
   flattenStyleRulesToArrayHelper (rules, result)
   result

let private createReactXPAnimatedViewStyleObject (typedValues: seq<TypedReactXPStyleRule>) : ReactXPStyleRulesObject =
    let rawValues = typedValues |> Seq.map fst
    ReactXPRaw?Styles?createAnimatedViewStyle(createObj (rawValues :?> seq<string * obj>))

let processDynamicStyles (rawValues: seq<RuleFunctionReturnedStyleRules>) : List<RuntimeStyles> =
    rawValues
    |> flattenStyleRules
    |> LazilyCreatedReactXpStyleObject
    |> RuntimeStyles.StaticRules
    |> List.ofOneItem

let processDynamicAniStyles (runtimeStyles: List<RuntimeStyles>) : List<RuntimeStyles> =
    runtimeStyles

let aniRules (rawValues: seq<RuleFunctionReturnedStyleRules>) : ReactXPStyleRulesObject =
    rawValues
    |> flattenStyleRules
    |> createReactXPAnimatedViewStyleObject

let flattenBuildingBlocks(blocks: List<ISheetBuildingBlock>) : List<Selector * Styles> =
    blocks
    |> List.collect (fun block ->
        block.Match
            (fun one  -> [one.ToTuple])
            (fun many -> many.Blocks)
    )

let rec private flattenAndBuildReactXpInternalRepresentation (rawValues: seq<RuleFunctionReturnedStyleRules>) : Styles =
    rawValues
    |> flattenStyleRules
    |> LazilyCreatedReactXpStyleObject
    |> Styles.Rules


let rec private processPairs (selectorToStylesPairs: List<Selector * Styles>) : RuntimeStyles =
    let processed =
        selectorToStylesPairs
        |> List.map (fun (selector, styles) ->
            let selectorSet = selector.Split([|"&&"|], System.StringSplitOptions.None) |> Seq.map (fun s -> s.Trim()) |> Set.ofSeq
            (selectorSet, processStyles styles)
        )

    RuntimeStyles.Sheet processed

and processStyles (styles: Styles) : RuntimeStyles =
    match styles with
    | Styles.Sheet sheet              -> processPairs sheet
    | Styles.Rules reactXpRulesObject -> RuntimeStyles.StaticRules reactXpRulesObject

let private processIntoRuntimeOptimizedStructure(blocks: List<ISheetBuildingBlock>) : RuntimeStyles =
    match blocks with
    | [] ->
        Log.Error "For style sheets, do not pass empty list to `compile`, use `RuntimeStyles.None` instead"
        RuntimeStyles.None
    | _ ->
        blocks
        |> flattenBuildingBlocks
        |> processPairs

let makeSheet (blocks: List<ISheetBuildingBlock>) : Styles =
    blocks |> flattenBuildingBlocks |> Styles.Sheet

// short alias for .style files
let compile = processIntoRuntimeOptimizedStructure
let compileLazy styles = lazy processIntoRuntimeOptimizedStructure styles
let rules = flattenAndBuildReactXpInternalRepresentation

// sadly need this to help type inference along (see Button.styles.fs for example usage)
let asBlocks (blocks: List<ISheetBuildingBlock>) : List<ISheetBuildingBlock> = blocks

let ( ==> ) (selector: Selector) (styles: Styles) : SheetBuildingBlockOne =
    SheetBuildingBlockOne (selector, styles)

let ( => ) (selector: Selector) (rawValues: seq<RuleFunctionReturnedStyleRules>) : SheetBuildingBlockOne =
    selector ==> flattenAndBuildReactXpInternalRepresentation rawValues

let private prependSelectorOne (selectorToPrepend: Selector) (selector: Selector, styles: Styles) : SheetBuildingBlockOne =
    SheetBuildingBlockOne(selectorToPrepend + " && " + selector, styles)

let private prependSelector (selector: Selector) (block: ISheetBuildingBlock) : ISheetBuildingBlock =
    block.Match
        (fun one  -> prependSelectorOne selector one.ToTuple :> ISheetBuildingBlock)
        (fun many -> SheetBuildingBlockMany (many.Blocks |> List.map (fun inputTuple -> (prependSelectorOne selector inputTuple).ToTuple)) :> ISheetBuildingBlock)

let ( && ) (blockToExtend: SheetBuildingBlockOne) (blocks: List<ISheetBuildingBlock>) : ISheetBuildingBlock =
    let newBlocks: List<Selector * Styles> =
        blocks
        |> List.map (prependSelector blockToExtend.Selector)
        |> flattenBuildingBlocks

    (SheetBuildingBlockMany (blockToExtend.ToTuple :: newBlocks)) :> ISheetBuildingBlock
