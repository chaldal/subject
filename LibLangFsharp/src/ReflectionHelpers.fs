[<AutoOpen>]
module ReflectionHelpers

open System.Reflection
open FSharp.Reflection

// NOTE the fact that this method is inline is a byproduct of it being use
// in Fable, where getting type information passed through correctly is only
// possible by inlining the function. So perhaps we need compiler if/else guards,
// and different implementation based on JS vs dotnet runtime.
// TODO convert to use a cached FSharpValue.PreCompute* variant
let inline unionCaseName<'T> (case: 'T) : string =
    match Microsoft.FSharp.Reflection.FSharpValue.GetUnionFields(case, typeof<'T>) with
        | case, _ -> case.Name


// Getting the real field name in Fable land is blocked on this issue:
// https://github.com/fable-compiler/Fable/issues/2017
let private unionPropertyItemRegex = new System.Text.RegularExpressions.Regex("^Item(\d)?$")
let unionCaseFieldName (propertyInfo: PropertyInfo) : Option<string> =
    match unionPropertyItemRegex.IsMatch propertyInfo.Name with
    | true  -> None
    | false -> Some propertyInfo.Name
