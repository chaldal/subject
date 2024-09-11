module AstUtils

open Fable
open Fable.AST
open System
open System.Linq
open System.Text.RegularExpressions

let cleanFullDisplayName str =
    Regex.Replace(str, @"`\d+", "").Replace(".", "_")

let makeIdent name: Fable.Ident =
    { Name = name
      Type = Fable.Any
      IsCompilerGenerated = true
      IsThisArgument = false
      IsMutable = false
      Range = None }

let makeUniqueIdent (name: string) =
    let hashToString (i: int) =
        if i < 0
        then "Z" + (abs i).ToString("X")
        else i.ToString("X")
    "$" + name + (Guid.NewGuid().GetHashCode() |> hashToString) |> makeIdent

let makeValue r value =
    Fable.Value(value, r)

let makeStrConst (x: string) =
    Fable.StringConstant x
    |> makeValue None

let nullValue = Fable.Expr.Value(Fable.ValueKind.Null(Fable.Type.Any), None)

let makeCallInfo args: Fable.CallInfo = {
    ThisArg = None
    Args = args
    SignatureArgTypes = []
    GenericArgs = []
    MemberRef = None
    Tags = []
}

let emitJs macro args  =
    let callInfo = makeCallInfo args

    let emitInfo : Fable.AST.Fable.EmitInfo =
        { Macro = macro
          IsStatement = false
          CallInfo = callInfo }

    Fable.Expr.Emit(emitInfo, Fable.Type.Any, None)

let rec flattenList (head: Fable.Expr) (tail: Fable.Expr) =
    [
        yield head;
        match tail with
        | Fable.Expr.Value (value, range) ->
            match value with
            | Fable.ValueKind.NewList(Some(nextHead, nextTail), _listType) ->
                yield! flattenList nextHead nextTail
            | Fable.ValueKind.NewList(None, _listType) ->
                yield! [ ]
            | _ ->
                yield! [ Fable.Expr.Value (value, range) ]

        | _ ->
            yield! [ ]
    ]

let makeImport (selector: string) (path: string) =
    Fable.Import({ Selector = selector.Trim()
                   Path = path.Trim()
                   Kind = Fable.UserImport(false) }, Fable.Any, None)

let isRecordNonImplementingCustomInterfaces (compiler: PluginHelper) (fableType: Fable.Type) =
    match fableType with
    | Fable.Type.AnonymousRecordType _ -> true
    | Fable.Type.DeclaredType (entity, _genericArgs) ->
        let ent = compiler.GetEntity(entity)
        ent.IsFSharpRecord
        && (ent.AllInterfaces |> Seq.exists (fun i -> i.Entity.FullName.StartsWith("System.") |> not) |> not)
    | _ -> false

let isUnionOrNonAnonRecord (compiler: PluginHelper) (fableType: Fable.Type) =
    match fableType with
    | Fable.Type.DeclaredType (entity, _genericArgs) ->
        let ent = compiler.GetEntity(entity)
        ent.IsFSharpRecord || ent.IsFSharpUnion
    | _ -> false

let getRecordFields (compiler: PluginHelper) (fableType: Fable.Type) =
    match fableType with
    | Fable.Type.AnonymousRecordType(fieldNames, fieldTypes, _isStruct) -> List.zip (Array.toList fieldNames) fieldTypes
    | Fable.Type.DeclaredType (entity, _genericArgs) ->
        let ent = compiler.GetEntity(entity)
        if ent.IsFSharpRecord then
            ent.FSharpFields |> List.map (fun fi -> fi.Name, fi.FieldType)
        else []
    | _ -> []

let isPropertyList (_compiler: PluginHelper) (fableType: Fable.Type) =
    match fableType with
    | Fable.Type.List(genericArg) ->
        match genericArg with
        | Fable.Type.DeclaredType (entity, _genericArgs) -> entity.FullName.EndsWith "IReactProperty"
        | _ -> false
    | _ -> false

let isPascalCase (input: string) = not (String.IsNullOrWhiteSpace input) && List.contains input[0] ['A' .. 'Z']
let isCamelCase (input: string) = not (isPascalCase input)

let isAnonymousRecord (fableType: Fable.Type) =
    match fableType with
    | Fable.Type.AnonymousRecordType  _ -> true
    | _ -> false

let isReactElement (fableType: Fable.Type) =
    match fableType with
    | Fable.Type.DeclaredType (entity, _genericArgs) -> entity.FullName.EndsWith "ReactElement"
    | _ -> false

let recordHasField name (compiler: PluginHelper) (fableType: Fable.Type) =
    match fableType with
    | Fable.Type.AnonymousRecordType (fieldNames, _genericArgs, _isStruct) ->
        fieldNames
        |> Array.exists (fun field -> field = name)

    | Fable.Type.DeclaredType (entity, _genericArgs) ->
        compiler.GetEntity(entity).FSharpFields
        |> List.exists (fun field -> field.Name = name)

    | _ ->
        false

let memberName = function
    | Fable.MemberRef(_,m) -> m.CompiledName
    | Fable.GeneratedMemberRef m -> m.Info.Name

let makeCall callee args =
    Fable.Call(callee, makeCallInfo args, Fable.Any, None)

let makeSet target fieldName value =
    Fable.Set(target, Fable.SetKind.FieldSet(fieldName), Fable.Type.String, value, None)

let createElement reactElementType args =
    let callee = makeImport "createElement" "react"
    Fable.Call(callee, makeCallInfo args, reactElementType, None)

let emptyReactElement reactElementType =
    Fable.Expr.Value(Fable.Null(reactElementType), None)

let makeMemberInfo isInstance typ name: Fable.GeneratedMemberInfo = {
    Name = name
    ParamTypes = []
    ReturnType = typ
    IsInstance = isInstance
    HasSpread = false
    IsMutable = false
    DeclaringEntity = None
}

let objValue (k, v): Fable.ObjectExprMember =
    {
        Name = k
        Args = []
        Body = v
        MemberRef =
            makeMemberInfo true v.Type k
            |> Fable.GeneratedValue
            |> Fable.GeneratedMemberRef
        IsMangled = false
    }

let objExpr kvs = Fable.ObjectExpr(List.map objValue kvs, Fable.Any, None)

let capitalize (input: string) =
    if String.IsNullOrWhiteSpace input
    then ""
    else input.First().ToString().ToUpper() + String.Join("", input.Skip(1))

let camelCase (input: string) =
    if String.IsNullOrWhiteSpace input
    then ""
    else input.First().ToString().ToLower() + String.Join("", input.Skip(1))
