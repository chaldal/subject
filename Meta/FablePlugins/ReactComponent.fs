namespace Fable.React

open Fable
open Fable.AST
open Fable.AST.Fable

// Tell Fable to scan for plugins in this assembly
[<assembly:ScanForPlugins>]
do()

type MemoEq =
    | None = 0
    | React = 1
    | FSharp = 2

module internal ReactComponentHelpers =
    let (|ReactMemo|_|) = function
        | Import({ Selector = "memo"; Path = "react" },_,_) as e -> Some e
        | Get(Import({ Path = "react" },_,_),kind,_,_) as e ->
            match kind with
            | ExprGet(Value(StringConstant "memo",_)) -> Some e
            | FieldGet i when i.Name = "memo" -> Some e
            | _ -> None
        | _ -> None

    let injectReactImport body =
        let body = match body with Sequential body -> body | _ -> [body]
        Sequential [
            AstUtils.makeImport "default as React" "react"
            yield! body
        ]

    let applyImportOrMemo (libraryDir: string) import from (memo: Option<MemoEq>) (decl: MemberDecl) =
        match import, from, memo with
        | Some _, Some _, _ ->
            let reactElType = decl.Body.Type
            // imported component doesn't need to emit any JS
            let tags = "remove-declaration" :: decl.Tags
            { decl with Body = AstUtils.emptyReactElement reactElType; Tags = tags }

        | _, _, Some(MemoEq.React | MemoEq.FSharp as memoEq) ->
            let memoFn = AstUtils.makeImport "memo" "react"
            let body =
                decl.Body
                |> injectReactImport
                |> fun body -> Delegate(decl.Args, body, Some(decl.Name.Replace(".", "_")), ["not-arrow"])
            let memoArgs =
                match memoEq with
                | MemoEq.FSharp ->
                    [
                        body
                        AstUtils.emitJs "(oldProps, newProps) => Object.keys(oldProps).every(k => $0(oldProps[k], newProps[k]))" [
                            AstUtils.makeImport "equals" $"{libraryDir}/Util.js"
                        ]
                    ]
                | _ -> [body]
                |> AstUtils.makeCall memoFn
            // Change declaration kind from function to value
            let info =
                AstUtils.memberName decl.MemberRef
                |> AstUtils.makeMemberInfo false body.Type
                |> GeneratedValue
                |> GeneratedMemberRef

            { decl with MemberRef = info; Args = []; Body = memoArgs }

        | _ -> { decl with Body = injectReactImport decl.Body }

open ReactComponentHelpers

/// <summary>Transforms a function into a React function component. Make sure the function is defined at the module level</summary>
type RawReactComponentAttribute(?exportDefault: bool, ?import: string, ?from:string, ?memo: MemoEq) =
    inherit MemberDeclarationPluginAttribute()
    override _.FableMinimumVersion = "4.0"
    new() = RawReactComponentAttribute(exportDefault=false)
    new(exportDefault: bool) = RawReactComponentAttribute(exportDefault=exportDefault,?import=None, ?from=None)
    new(import: string, from: string) = RawReactComponentAttribute(exportDefault=false,import=import, from=from)

    /// <summary>Transforms call-site into createElement calls</summary>
    override this.TransformCall(compiler, memb, expr) =
        let reactElType = expr.Type
        let membArgs = memb.CurriedParameterGroups |> List.concat
        match expr with
        | Call(callee, info, _typeInfo, _range) ->
            let reactComponent =
                match import, from with
                | Some importedMember, Some externalModule ->
                    AstUtils.makeImport importedMember externalModule
                | _ ->
                    callee

            if List.length membArgs = info.Args.Length
                && info.Args.Length = 1
                && AstUtils.isRecordNonImplementingCustomInterfaces compiler info.Args[0].Type then
                // F# Component { Value = 1 }
                // JSX <Component Value={1} />
                // JS createElement(Component, { Value: 1 })
                if AstUtils.recordHasField "Key" compiler info.Args[0].Type then
                    // When the key property is upper-case (which is common in record fields)
                    // then we should rewrite it
                    let modifiedRecord = AstUtils.emitJs "(($value) => { $value.key = $value.Key; return $value; })($0)" [info.Args[0]]
                    AstUtils.createElement reactElType [reactComponent; modifiedRecord]
                else
                    AstUtils.createElement reactElType [reactComponent; info.Args[0]]
            elif info.Args.Length = 1 && info.Args[0].Type = Type.Unit then
                // F# Component()
                // JSX <Component />
                // JS createElement(Component, null)
                AstUtils.createElement reactElType [reactComponent; AstUtils.nullValue]
            else
                let mutable keyBinding = None

                let propsObj =
                    List.zip (List.take info.Args.Length membArgs) info.Args
                    |> List.collect (fun (arg, expr) ->
                        match arg.Name, expr with
                        | Some "key", IdentExpr _ -> ["key", expr; "$key", expr]
                        | Some "key", _ ->
                            let keyIdent = AstUtils.makeUniqueIdent "key"
                            keyBinding <- Some(keyIdent, expr)
                            ["key", IdentExpr keyIdent; "$key", IdentExpr keyIdent]
                        | Some name, _ -> [name, expr]
                        | None, _ -> [])
                    |> AstUtils.objExpr

                let reactEl = AstUtils.createElement reactElType [reactComponent; propsObj]
                let expr =
                    match keyBinding with
                    | None -> reactEl
                    | Some(ident, value) -> Let(ident, value, reactEl)
                match [|memo, callee|] with
                // If the call is memo and the function has an identifier, we can set the displayName
                | [|(Some(MemoEq.React | MemoEq.FSharp as memoEq)), (IdentExpr i)|] ->
                    Sequential [
                        (AstUtils.makeSet (IdentExpr(i)) "displayName" (AstUtils.makeStrConst i.Name))
                        expr
                    ]
                | _ -> expr
        | _ ->
            // return expression as is when it is not a call expression
            expr

    override this.Transform(compiler, file, decl) =

        let info = compiler.GetMember(decl.MemberRef)
        if info.IsValue || info.IsGetter || info.IsSetter then
            // Invalid attribute usage
            let errorMessage = sprintf "Expecting a function declaration for %s when using [<Component>]" decl.Name
            compiler.LogWarning(errorMessage, ?range=decl.Body.Range)
            decl
        else if not (AstUtils.isReactElement decl.Body.Type) then
            // output of a React function component must be a ReactElement
            let errorMessage = sprintf "Expected function %s to return a ReactElement when using [<Component>]. Instead it returns %A" decl.Name decl.Body.Type
            compiler.LogWarning(errorMessage, ?range=decl.Body.Range)
            decl
        else
            if (AstUtils.isCamelCase decl.Name) then
                compiler.LogWarning(sprintf "React function component '%s' is written in camelCase format. Please consider declaring it in PascalCase (i.e. '%s') to follow conventions of React applications and allow tools such as react-refresh to pick it up." decl.Name (AstUtils.capitalize decl.Name))

            let decl =
                match exportDefault with
                | Some true -> { decl with Tags = "export-default"::decl.Tags }
                | Some false | None -> decl

            // do not rewrite components accepting records as input
            if decl.Args.Length = 1 && AstUtils.isRecordNonImplementingCustomInterfaces compiler decl.Args[0].Type then
                // check whether the record type is defined in this file
                // trigger warning if that is case
                let definedInThisFile =
                    file.Declarations
                    |> List.tryPick (fun declaration ->
                        match declaration with
                        | Declaration.ClassDeclaration classDecl ->
                            let classEntity = compiler.GetEntity(classDecl.Entity)
                            match decl.Args[0].Type with
                            | Type.DeclaredType (entity, _genericArgs) ->
                                let declaredEntity = compiler.GetEntity(entity)
                                if classEntity.IsFSharpRecord && declaredEntity.FullName = classEntity.FullName
                                then Some declaredEntity.FullName
                                else None

                            | _ -> None

                        | Declaration.ActionDeclaration _action ->
                            None
                        | _ ->
                            None
                    )

                match definedInThisFile with
                | Some recordTypeName ->
                    let errorMsg = String.concat "" [
                        sprintf "Function component '%s' is using a record type '%s' as an input parameter. " decl.Name recordTypeName
                        "This happens to break React tooling like react-refresh and hot module reloading. "
                        "To fix this issue, consider using an anonymous record instead or multiple simpler values as input parameters (can be tupled). "
                        "Future versions of [<Component>] might not emit this warning anymore, in which case you can assume that the issue is fixed. "
                        "To learn more about the issue, see https://github.com/pmmmwh/react-refresh-webpack-plugin/issues/258"
                    ]

                    compiler.LogWarning(errorMsg, ?range=decl.Body.Range)

                | None -> () // nothing to report

                decl
                |> applyImportOrMemo compiler.LibraryDir import from memo

            else if decl.Args.Length = 1 && decl.Args[0].Type = Type.Unit then
                // remove arguments from functions requiring unit as input
                { decl with Args = [ ] }
                |> applyImportOrMemo compiler.LibraryDir import from memo

            else
                // rewrite all other arguments into getters of a single props object
                // TODO: transform any callback into into useCallback(callback) to stabilize reference
                let propsArg = AstUtils.makeIdent (sprintf "%sInputProps" (AstUtils.camelCase decl.Name))
                let propBindings =
                    ([], decl.Args) ||> List.fold (fun bindings arg ->
                        let getterKey = (if arg.DisplayName = "key" then "$key" else arg.DisplayName).Replace("`", "")
                        let getterKind = ExprGet(AstUtils.makeStrConst getterKey)
                        let getter = Get(IdentExpr propsArg, getterKind, Any, None)
                        (arg, getter)::bindings)
                    |> List.rev

                let body =
                    match decl.Body with
                    // If the body is surrounded by a memo call we put the bindings within the call
                    // because Fable will later move the surrounding function into memo
                    | Call(ReactMemo reactMemo, ({ Args = arg::restArgs } as callInfo), t, r) ->
                        let arg = propBindings |> List.fold (fun body (k,v) -> Let(k, v, body)) arg
                        Call(reactMemo, { callInfo with Args = arg::restArgs }, t, r)
                    | _ ->
                        propBindings |> List.fold (fun body (k,v) -> Let(k, v, body)) decl.Body

                { decl with Args = [propsArg]; Body = body }
                |> applyImportOrMemo compiler.LibraryDir import from memo


type ReactShallEqualityMemoComponentAttribute(exportDefault: bool) =
    inherit RawReactComponentAttribute(exportDefault=exportDefault, ?import=None, ?from=None, memo=MemoEq.React)
    new() = ReactShallEqualityMemoComponentAttribute(exportDefault=false)

/// Like ReactMemoComponent but uses a custom equality function that will use F# equality on props that
type ComponentAttribute(exportDefault: bool) =
    inherit RawReactComponentAttribute(exportDefault=exportDefault, ?import=None, ?from=None, memo=MemoEq.FSharp)
    new() = ComponentAttribute(exportDefault=false)
