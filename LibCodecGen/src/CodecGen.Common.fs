[<AutoOpen>]
module CodecGen.Common

open System.Text.RegularExpressions
open FSharp.Compiler.Symbols
open System
open FSharp.Data
open FSharpPlus
open FSharp.Compiler.CodeAnalysis
open System.IO

let (+-+) (a : string) (b : string) = Path.Combine(a, b).Replace("\\", "/")

type SuiteInputs = {
    TypesProjectPath: string
    TypesProjectName: string
    TypesProjectSourceShouldCompile: string -> bool // return false for .fs files that break codec generation, other modules shouldn't depend on them
    ShouldIncludeCodecTypeLabel: FSharpEntity -> bool
    AbbreviateGenericParamWitness: bool // use false if not sure
    CrossEcosystemTypeLabelPrefix: string // to qualify type labels for cross-ecosystems calls, applies only to LifeEvent and SubjectId
}

[<Literal>]
let SourceDir = __SOURCE_DIRECTORY__

// template fsproj is incidental here - it's just any type project is good to use it as a sample for type provider
// TODO: use a sample xml text instead
type ProjSettings = XmlProvider<"../../Meta/Templates/Ecosystem/Ecosystem/T__EC__T.Types/T__EC__T.Types.fsproj ", Global=true, ResolutionFolder=SourceDir>

module Inputs =

    let dllName      = __SOURCE_DIRECTORY__ +-+ "Suite-compiled.dll"
    let projFileName = __SOURCE_DIRECTORY__ +-+ "Suite.fsproj"
    let fscore       = typedefof<list<_>>.Assembly.Location
    let fsplus       = typeof<FSharpPlus.Data.All>.Assembly.Location
    let fleece       = typeof<Fleece.AdHocEncoding>.Assembly.Location
    let numerics     = typeof<System.Numerics.BigInteger>.Assembly.Location


type String with
    static member Space = " "

type Field =
    | Anon    of fieldName: string * fieldType: FSharpType
    | Nominal of field: FSharpField
    member x.DisplayName =
        match x with
        | Anon    (n, _) -> n
        | Nominal f -> f.DisplayName

type Fields =
    | Anons    of fields: (string * FSharpType) []
    | Nominals of fields: list<FSharpField> with
    static member ToSeq x = x |> function
                | Anons    x -> Array.map Anon   x |> toSeq
                | Nominals x -> List.map Nominal x |> toSeq


[<RequireQualifiedAccess>]
type DuCaseQualifyStyle =
    | Unqualified
    | Prefixed    of Prefix: string
    | QualifyFunc

let private newLineDetectBufferSize = 128
let private newLineDetectBuffer = Array.zeroCreate newLineDetectBufferSize
let private tryAutoDetectLineEndings (fileName: string) =
    use fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read)
    use streamReader = new StreamReader(fileStream)
    let newLineChars = [| '\r'; '\n' |]

    let rec doRead () =
        let bytesRead = streamReader.Read(newLineDetectBuffer, 0, newLineDetectBufferSize)
        let currentRead = newLineDetectBuffer.AsSpan(0, bytesRead)
        let firstLineEndingIndex =
            currentRead.IndexOfAny (ReadOnlySpan(newLineChars))

        if firstLineEndingIndex > -1 then
            match currentRead.[firstLineEndingIndex] with
            | '\n' -> "\n" |> Some
            | '\r' ->
                if currentRead.Length > firstLineEndingIndex + 1 then
                    match currentRead.[firstLineEndingIndex + 1] with
                    | '\n' -> "\r\n" |> Some
                    | _ -> "\r" |> Some
                else
                    let singleRead = streamReader.Read()
                    if singleRead = (int '\n') then
                        "\r\n" |> Some
                    else
                        "\r" |> Some
            | _ -> None
        elif bytesRead < newLineDetectBufferSize then
            None
        else
            doRead ()

    doRead ()


let private getCode (maybeFileName: Option<string>) (tys: FSharpEntity list) (suiteInputs: SuiteInputs) =
    let nl =
        maybeFileName
        |> Option.bind tryAutoDetectLineEndings
        |> Option.defaultValue System.Environment.NewLine

    let spaces x = String.replicate x " "
    let toCamel (s: string) =
        [
            "End", "``end``"
            "Type", "``type``"
            "Break", "``break``"
            "To", "``to``"
        ]
        |> Seq.tryFind (fst >> ((=) s))
        |> Option.map snd
        |> Option.defaultWith (fun () ->
            (Char.ToLower s.[0] |> string) + s.[1..])

    let anonRecordName (t: FSharpType) = "Anon_" + String.concat "_" t.AnonRecordTypeDetails.SortedFieldNames

    let mapGenParamName lowercase (n: string) =
        if lowercase then
            (n.[0] |> Char.ToLower).ToString() + (if suiteInputs.AbbreviateGenericParamWitness then "" else n.[1..])
        else
            n

    let genImplicitParams gn =
        match gn with
        | [] -> ""
        | _  -> intercalate ", " (gn |> List.map (konst "_"))

    let rec genDefaultCodecTypeArg (t: FSharpType) =
        // TODO: fix logic duplication with getExplicitCodec somehow e.g. whitelist of some System types and check for CodecAutoGenerate
        let genTypeArgs (args: list<FSharpType>) = args |> List.map genDefaultCodecTypeArg |> intercalate ", "
        if t.IsGenericParameter then
            $"'{mapGenParamName true t.GenericParameter.DisplayName}"
        elif t.HasTypeDefinition && not t.IsAnonRecordType then
            match t.TypeDefinition.TryGetFullName () with
            | Some s ->
                if String.contains '`' s then
                    let name =
                        let x = t.TypeDefinition.LogicalName
                        x.Substring (0, x.IndexOf '`')
                    $"{name}<{genTypeArgs (toList t.GenericArguments)}>"
                else
                    t.TypeDefinition.LogicalName
            | None ->
                let name = t.TypeDefinition.LogicalName
                match name with
                | "DateOnly" | "TimeOnly" | "Guid" | "DateTimeOffset" | "TimeSpan" | "String" | "Char"
                | "String" | "Char" | "Int32" | "Double" | "Int16" | "Int64" | "UInt16" | "UInt32" | "UInt64" | "Boolean" | "Decimal" | "Unit" | "Byte"
                | "string" | "char" | "int"   | "double" | "int16" | "int64" | "uint16" | "uint32" | "uint64" | "bool"    | "decimal" | "unit" | "byte" ->
                    name
                | _ ->
                    "_"
        else
            "_"

    // This function optimizes compile time by generating some explicit codecs
    // to turn off this optimization, just return None
    let rec getExplicitCodec (rawType: FSharpType) : string option =
        let nonAbbreviatedType = if rawType.IsAbbreviation then rawType.AbbreviatedType else rawType

        // TODO: is there a better way to detect a type with measure?? Is it reliable at all
        let hasMeasure = (not nonAbbreviatedType.IsTupleType) && (nonAbbreviatedType.ErasedType <> nonAbbreviatedType)

        let t = if hasMeasure then nonAbbreviatedType.ErasedType else nonAbbreviatedType

        let r =
            if t.HasTypeDefinition && not t.IsAnonRecordType then
                match (t.TypeDefinition.LogicalName, t.TypeDefinition.TryGetFullName (), t.TypeDefinition.DeclaringEntity) with
                | _, Some s, Some m when m.Attributes |> Seq.exists  (fun x -> x.AttributeType.CompiledName = "CodecAutoGenerate") && not (String.contains '`' s)
                                  -> Some $"codecFor<_, {t.TypeDefinition.LogicalName}>"
                | _, Some _, Some m when m.Attributes |> Seq.exists  (fun x -> x.AttributeType.CompiledName = "CodecAutoGenerate") ->
                    Some $"codecFor<_, {genDefaultCodecTypeArg t}>"
                | "Int32"  , _, _ -> Some $"Codecs.int"
                | "Double" , _, _ ->
                    Some (if hasMeasure then "(CodecsWithMeasure.float())" else $"Codecs.float")
                | "DateOnly" , _, _ -> Some $"Codecs.date"
                | "TimeOnly" , _, _ -> Some $"Codecs.time"
                | ("String" | "Guid" | "DateTimeOffset" | "TimeSpan" | "Char" | "Int16" | "Int64" | "UInt16" | "UInt32" | "UInt64" | "Boolean" | "Decimal" | "Unit" | "Byte") , _, _
                    -> Some $"Codecs.{String.toLower t.TypeDefinition.LogicalName.[0..1] + t.TypeDefinition.LogicalName.[2..]}"

                |  ( "DayOfTheWeek" |  "NonemptyString" | "BlobId" ) , _, _
                    -> Some $"codecFor<_, {t.TypeDefinition.LogicalName}>"

                | "EmailAddress", _, _
                | "PhoneNumber" , _, _
                | "SubjectTransactionId" , _, _
                | "CallId"      , _, _ -> Some $"codecFor<_, {t.TypeDefinition.LogicalName}>"
                | "UrlSlug`1"   , _, _ -> Some $"codecFor<_, {t.TypeDefinition.LogicalName.Substring(0, t.TypeDefinition.LogicalName.Length - 2)}>"

                | ("PositiveInteger" | "PositiveDecimal"  | "UnsignedInteger" | "PositivePercentage" | "UnsignedDecimal" | "NonemptyLowerCaseString" | "Date" | "MimeType" | "ResultPage" | "OrderDirection" ) , _, _
                    -> Some $"codecFor<_, {t.TypeDefinition.LogicalName}>"

                | ("StringBloomFilter", _, _) -> Some $"codecFor<_, {t.TypeDefinition.LogicalName}>"

                | "[]`1", _, _ when t.GenericArguments.[0].TypeDefinition.LogicalName.ToLower() = "byte" ->
                    // base64 string codec is much faster than (Codecs.array Codecs.byte)
                    Some "Codecs.base64Bytes"

                | s, _, _ when (String.contains '`' s) ->
                    let tg =
                        // Keyed sets admit only one combinator. On the values, which is the 2nd generic parameter.
                        if String.isSubString "KeyedSet" t.TypeDefinition.LogicalName then [item 1 t.GenericArguments]
                        else toList t.GenericArguments
                    match traverse getExplicitCodec (toList tg) with
                    | Some lst ->
                        let c =
                            match t.TypeDefinition.LogicalName with
                            | "Map`2" ->
                                // if (toList t.GenericArguments) .[1].TypeDefinition.LogicalName = "String" then Some $"i.map"
                                // else
                                Some $"Codecs.gmap"

                            | "[]`1" ->
                                Some "Codecs.array"

                            | "Choice`3" ->
                                Some "Codecs.choice3"

                            | ("Option`1" | "List`1" | "Result`2" | "Set`1" | "Choice`2" | "Choice`4") ->
                                ("Codecs." + (t.TypeDefinition.LogicalName |> rev |> skip 2 |> rev |> String.toLower)) |> Some

                            | ("NonemptyList`1" | "NonemptySet`1" | "OrderedSet`1" | "KeyedSet`2" | "OrderedKeyedSet`2" | "NonemptyOrderedSet`1" | "NonemptyKeyedSet`2" | "NonemptyOrderedKeyedSet`2" | "AsyncSettableValue`1" | "NonemptyMap`2" | "Or`2") ->
                                Some $"{t.TypeDefinition.LogicalName.Substring(0, t.TypeDefinition.LogicalName.Length - 2)}.codec"

                            | _ -> None
                        c |> Option.map (fun c -> $"({c} {lst |> String.intercalate String.Space})")
                    | None -> None

                | _, _, _  -> None
            elif t.IsTupleType then
                match traverse getExplicitCodec (toList t.GenericArguments) with
                | Some lst when length lst >= 2 && length lst < 8 ->
                    Some $"(Codecs.tuple{length lst} {String.intercalate String.Space lst})"
                | _ ->
                    None
            elif t.IsGenericParameter then
                // NOTE: this used to generate defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE, which will works for wider range of types than codecFor
                // however since net7 and beyond this slows down compilation big times. So I optimistically generate code which will compile faster at the expense of
                // risk that generated generic codec might not compile e.g. when a primitive type used as a 't.
                let genericArgTypeName = mapGenParamName true t.GenericParameter.DisplayName
                let genericCodecFor = $"codecFor<_, '{genericArgTypeName}>"

                printfn $"WARNING! Generated `{genericCodecFor}` will NOT compile when `{genericArgTypeName}` is not a record or union type (e.g. primitive, tuple, list etc.)"
                printfn $"If this is your case, then replace it with `defaultCodec_UNIVERSAL_BUT_SLOW_COMPILE_FOR_UNCONSTRAINED_TYPE<_, '{genericArgTypeName}>`"
                printfn $"However this code will compile considerably slower.{nl}"

                Some genericCodecFor
            else
                None

        // See which types are not being processed
        // if not r.IsSome then
        //     if not t.HasTypeDefinition then printfn "No type definition"
        //     elif t.IsAnonRecordType then printfn "Anonymous type"
        //     else
        //         printfn "Full: %A" (t.TypeDefinition.TryGetFullName ())
        //         printfn "Logical: %A" (t.TypeDefinition.LogicalName)
        //         match toList t.GenericArguments with
        //         | [] ->  printfn "No generics"
        //         | lst ->
        //             printfn "Generics: %i" (length lst)
        //             let _ = List.map getExplicitCodec lst
        //             ()
        //     printfn ""

        r

    let printRecCombinator (t: FSharpType list) =
        // already handled
        // match t with
        // | [] -> "reqWith i.unit"
        // | t  ->
                let combinators = traverse getExplicitCodec t
                match combinators with
                | Some [c] -> $"reqWith {c}"
                | Some lst when length lst < 8 ->
                    $"reqWith (Codecs.tuple{length t} {String.intercalate String.Space lst})"
                | _ ->
                    let _combinators = traverse getExplicitCodec t
                    "reqWith codecFor<_, 'fix_the_gen>"

    let printDuCase isSingleCase qualifyOptionNone (case: FSharpUnionCase) =

        let printCombinator (case: FSharpUnionCase) =

            if length case.Fields = 0 then
                "reqWith Codecs.unit"
            elif length case.Fields = 1 && case.Fields.[0].FieldType.IsAnonRecordType then
                $"reqWith ({anonRecordName case.Fields.[0].FieldType}.get_Codec ())"
            else
                printRecCombinator (case.Fields |> toList |> map ( fun x -> x.FieldType))

        let qualifiedCaseName = $"\"{case.DisplayName}\""

        let optionNone = if qualifyOptionNone then "Option.None" else "None"
        if case.HasFields then
            let binding =
                if case.Fields.Count > 1 then "(" + intercalate ", " ({1..case.Fields.Count} |>> (fun x -> "x" + string x) ) + ")"
                else "x"
            if isSingleCase then
                $"| {case.DisplayName} _ ->"
                    + nl + spaces 12 + "codec {"
                    + nl + spaces 16 + $"""let! _version = reqWith Codecs.int "__v1" (function {case.DisplayName} _ -> Some 0)"""
                    + nl + spaces 16 + $"and! payload = {printCombinator case} {qualifiedCaseName} (function {case.DisplayName} {binding} -> Some {binding})"
                    + nl + spaces 16 + $"return {case.DisplayName} payload"
                    + nl + spaces 12 + "}"
            else
                $"| {case.DisplayName} _ ->"
                    + nl + spaces 12 + "codec {"
                    + nl + spaces 16 + $"""let! _version = reqWith Codecs.int "__v1" (function {case.DisplayName} _ -> Some 0 | _ -> {optionNone})"""
                    + nl + spaces 16 + $"and! payload = {printCombinator case} {qualifiedCaseName} (function {case.DisplayName} {binding} -> Some {binding} | _ -> {optionNone})"
                    + nl + spaces 16 + $"return {case.DisplayName} payload"
                    + nl + spaces 12 + "}"
        else
            if isSingleCase then
                $"| {case.DisplayName} ->"
                    + nl + spaces 12 + "codec {"
                    + nl + spaces 16 + $"""let! _version = reqWith Codecs.int "__v1" (function {case.DisplayName} -> Some 0)"""
                    + nl + spaces 16 + $"and! _ = {printCombinator case} {qualifiedCaseName} (function {case.DisplayName} -> Some ())"
                    + nl + spaces 16 + $"return {case.DisplayName}"
                    + nl + spaces 12 + "}"
            else
                $"| {case.DisplayName} ->"
                    + nl + spaces 12 + "codec {"
                    + nl + spaces 16 + $"""let! _version = reqWith Codecs.int "__v1" (function {case.DisplayName} -> Some 0 | _ -> {optionNone})"""
                    + nl + spaces 16 + $"and! _ = {printCombinator case} {qualifiedCaseName} (function {case.DisplayName} -> Some () | _ -> {optionNone})"
                    + nl + spaces 16 + $"return {case.DisplayName}"
                    + nl + spaces 12 + "}"

    let printFieldBinding (fld: Field) =
        let fDisplayName, fieldType =
            match fld with
            | Nominal f -> f.DisplayName, f.FieldType
            | Anon (fDisplayName, fieldType) -> (fDisplayName, fieldType)
        $" {toCamel fDisplayName} = {printRecCombinator [fieldType]} \"{fDisplayName}\" (fun x -> Some x.{fDisplayName})"

    let printFieldConstructor (f: Field) =
        //if f.FieldType.TypeDefinition.Assembly.SimpleName = "LibLangFsharp" then
        //    $"{f.DisplayName} = (tryParse {toCamel f.DisplayName}).Value"
        //else
            $"{f.DisplayName} = {toCamel f.DisplayName}"

    let _printEnclosingEntity s =
        let lst = String.split ["."] s |> toList
        lst.[lst.Length - 2]

    let encloseGen str = if String.length str = 0 then "" else "<" + str + ">"

    let getConstraint (param:FSharpGenericParameter) =

        let chopStringTo (s:string) (c:char) =
            // chopStringTo "abcdef" 'c' --> "def"
            if s.IndexOf c <> -1
            then
                let i =  s.IndexOf c + 1
                s.Substring(i, s.Length - i)
            else s
        let tryChopPropertyName (s: string) =
            // member names start with get_ or set_ when the member is a property
            let s =
                if s.StartsWith("get_", StringComparison.Ordinal) || s.StartsWith("set_", StringComparison.Ordinal)
                then s
                else chopStringTo s '.'
            if s.Length <= 4 || (let s = s.Substring(0,4) in s <> "get_" && s <> "set_")
            then None
            else Some(s.Substring(4,s.Length - 4))
        let getGenericParamName (param: FSharpGenericParameter) =
            (if param.IsSolveAtCompileTime then "^" else "'") + param.Name

        let getConstraint (genericParameterConstraint: FSharpGenericParameterConstraint) =
            let memberConstraint (memberConstraint: FSharpGenericParameterMemberConstraint) =

                let isProperty (memberConstraint: FSharpGenericParameterMemberConstraint) =
                    (memberConstraint.MemberIsStatic && memberConstraint.MemberArgumentTypes.Count = 0) ||
                    (not memberConstraint.MemberIsStatic && memberConstraint.MemberArgumentTypes.Count = 1)
                let formattedMemberName, isProperty =
                    match isProperty memberConstraint, tryChopPropertyName memberConstraint.MemberName with
                    | true, Some(chopped) when chopped <> memberConstraint.MemberName ->
                        chopped, true
                    | _, _ -> memberConstraint.MemberName, false
                seq {
                    yield " : ("
                    if memberConstraint.MemberIsStatic then yield "static "
                    yield $"member %s{formattedMemberName} : "
                    if isProperty then yield (memberConstraint.MemberReturnType.Format FSharpDisplayContext.Empty)
                    else
                        if memberConstraint.MemberArgumentTypes.Count <= 1
                        then yield "unit"
                        else yield getGenericParamName param
                        yield $" -> {(memberConstraint.MemberReturnType.Format FSharpDisplayContext.Empty).TrimStart()}"
                    yield ")"
                } |> String.concat ""
            let constraints =
                match genericParameterConstraint with
                | _ when genericParameterConstraint.IsCoercesToConstraint ->
                    $"%s{getGenericParamName param} :> %s{genericParameterConstraint.CoercesToTarget.Format FSharpDisplayContext.Empty}"
                | _ when genericParameterConstraint.IsMemberConstraint ->
                     $"%s{getGenericParamName param}%s{memberConstraint genericParameterConstraint.MemberConstraintData}"
                | _ when genericParameterConstraint.IsSupportsNullConstraint ->
                    $"%s{getGenericParamName param} : null"
                | _ when genericParameterConstraint.IsRequiresDefaultConstructorConstraint ->
                    $"%s{getGenericParamName param} : (new : unit -> '%s{param.DisplayName})"
                | _ when genericParameterConstraint.IsReferenceTypeConstraint ->
                    $"%s{getGenericParamName param} : not struct"
                | _ when genericParameterConstraint.IsEnumConstraint ->
                    $"%s{getGenericParamName param} : enum<%s{genericParameterConstraint.EnumConstraintTarget.Format FSharpDisplayContext.Empty}>"
                | _ when genericParameterConstraint.IsComparisonConstraint ->
                    $"%s{getGenericParamName param} : comparison"
                | _ when genericParameterConstraint.IsEqualityConstraint ->
                    $"%s{getGenericParamName param} : equality"
                | _ when genericParameterConstraint.IsDelegateConstraint ->
                    let tc = genericParameterConstraint.DelegateConstraintData
                    let delegateType = tc.DelegateTupledArgumentType.Format FSharpDisplayContext.Empty
                    let delegateReturn = tc.DelegateReturnType.Format FSharpDisplayContext.Empty
                    $"%s{getGenericParamName param} : delegate<%s{delegateType}, %s{delegateReturn}>"
                | _ when genericParameterConstraint.IsUnmanagedConstraint ->
                    $"%s{getGenericParamName param} : unmanaged"
                | _ when genericParameterConstraint.IsNonNullableValueTypeConstraint ->
                    $"%s{getGenericParamName param} : struct"
                | _ -> ""
            constraints

        if param.Constraints.Count > 0 then
                param.Constraints
                |> Seq.toList
                |> List.map getConstraint
                |> Some
        else None



    let genParams lowercase includeConstraints (gn: List<FSharpGenericParameter>) =
        match gn with
        | [] -> ""
        | _  ->
            let constraints = gn |> List.choose getConstraint |> List.concat
            let whenConstraints =
                if includeConstraints then
                    match constraints with
                    | [] -> ""
                    | constraints ->
                        " when " + intercalate " and " constraints
                else ""

            let genericParams = gn |> List.map (fun  p -> $"'{mapGenParamName lowercase p.Name}")
            intercalate ", " genericParams + whenConstraints

    let genWitnessParams (gn: List<FSharpGenericParameter>) =
        let mapName = mapGenParamName true
        match gn with
        | [] -> "typeLabel: string, _typeParams: _"
        | _  -> "typeLabel: string, _typeParams: " + intercalate " * " (gn |> List.map (fun p -> $"'{mapName p.Name}")) + ""

    let usedAsInterface (x: FSharpEntity) =
        toList x.AllInterfaces
        |> List.tryFind (fun x -> toList x.AllInterfaces |> List.exists (fun x -> x.TypeDefinition.DisplayName.StartsWith "IInterfaceCodec"))
        |> Option.map (
            fun x ->
                let genGenericArg (arg: FSharpType) =
                    if arg.IsGenericParameter then
                        $"'{arg.GenericParameter.DisplayName}"
                    else
                        $"{arg.TypeDefinition.DisplayName}{genParams true false (toList arg.TypeDefinition.GenericParameters) |> encloseGen}"

                x.TypeDefinition.DisplayName
                +
                    match toList x.GenericArguments with
                    | [] -> ""
                    | gs -> "<" + intercalate ", " (gs |> map genGenericArg) + ">"

                )

    let implementsStaticInterfaceWithCodecMember (x: FSharpEntity): Option<string * list<FSharpType>> =
        toList x.AllInterfaces
        |> Seq.choose (fun x ->
            let name = x.TypeDefinition.DisplayName
            if name.StartsWith "TimeSeriesDataPoint" then
                Some ("TimeSeriesDataPoint", x.GenericArguments |> toList)
            else if name.StartsWith "ViewInput" then
                Some ("ViewInput", x.GenericArguments |> toList)
            else if name.StartsWith "ViewOutput" then
                Some ("ViewOutput", x.GenericArguments |> toList)
            else
                None)
        |> Seq.tryHead

    let isRec (x: FSharpEntity) =
        let location = x.DeclarationLocation
        // printfn "location is %A" location.FileName
        let lines = File.ReadLines location.FileName |> toList
        // printfn "line is %A" lines.[location.StartLine-1]
        lines.[location.StartLine-1]
        |> String.trimStartWhiteSpaces
        |> String.startsWith "and"


    let rec printType
                isRec name includeCodecTypeLabel
                (usedAsInterface: Option<string>)
                (implementsStaticInterfaceWithCodecMember: Option<string * List<FSharpType>>)
                (fields: Fields)
                (cases: list<FSharpUnionCase>)
                (gn: List<FSharpGenericParameter>)
                (witness_gn: List<FSharpGenericParameter>) : string =
        // let fields = toList t.FSharpFields
        // let cases  = toList t.UnionCases
        let gen      = genParams false false gn
        let genConstraints = genParams false true gn
        let genLower = genParams true false gn
        let genImplicit = genImplicitParams gn

        let isRecordVsUnion = List.length cases = 0
        let objCodecVersion = if isRecordVsUnion then "V1" else "AllCases"

        let codecOrObjCodecMethod, codecPrivateMethod, maybeCodecOfObjCodecDetails =
            match fields, usedAsInterface with
            | Anons _, _ -> "get_Codec ()", konst $"get_ObjCodec_{objCodecVersion} ()", None
            | _, None ->
                if length gen = 0 then
                    "get_Codec ()", konst $"get_ObjCodec_{objCodecVersion} ()", None
                else
                    $"inline get_Codec ()", (fun generate -> if generate then $"inline get_ObjCodec_{objCodecVersion}<_, {genImplicit}> ()" else $"inline get_ObjCodec_{objCodecVersion} ()"), None
            | _, Some i ->
                if length gen = 0 then
                    (if length witness_gn = 0 then "private get_ObjCodec ()" else "get_ObjCodec ()"), konst $"get_ObjCodec_{objCodecVersion} ()",
                        Some (
                            "get_Codec ()",
                            (fun _ -> $"{name}.get_ObjCodec_{objCodecVersion} ()"),
                            (fun _ -> $"{name}.get_ObjCodec ()"),
                            i)
                else
                    "inline get_ObjCodec ()", konst $"inline get_ObjCodec_{objCodecVersion} ()",
                    Some (
                        "inline get_Codec ()",
                        (fun implicit -> if implicit then $"({name}<{genImplicit}>.get_ObjCodec_{objCodecVersion} ())" else $"({name}<{genLower}>.get_ObjCodec_{objCodecVersion} ())"),
                        (fun implicit -> if implicit then $"({name}<{genImplicit}>.get_ObjCodec ())" else $"({name}<{genLower}>.get_ObjCodec ())"),
                        i)

        // Check anons

        let anons =
            cases
            |> List.collect (fun case ->
                toList case.Fields
                |> List.collect (fun field -> if field.FieldType.IsAnonRecordType then [field.FieldType] else []))
            |> List.distinctBy (fun t -> t.AnonRecordTypeDetails.CompiledName) //SortedFieldNames)
            |> List.mapi (fun _ t ->
                let tys = (toArray t.GenericArguments)
                let anons = Array.zip t.AnonRecordTypeDetails.SortedFieldNames tys
                printType false "" false None None (Anons anons) [] [] [] + nl + nl)

        let explicitTypeAnnotation =
            match fields, usedAsInterface, genLower with
            // anon types need annotation
            | Anons fieldNames, _, _ -> ": Codec<_, {| " + (fieldNames |> Array.map (fun (n, t) -> n + ": " + t.TypeDefinition.DisplayName) |> intercalate "; ") +  " |}> "
            // non-generic types, or generic but not inheriting interface don't need the annotation
            | _, _, "" -> ""
            | _, None, _ -> ""
            // generics that used as interface do need the annotation
            | _, Some _, _ -> $": Codec<MultiObj<'RawEncoding>, {name}<{genLower}>> "

        let retOpen, retClose = match fields with Anons _ -> ("{|", " |}") | _ -> ("{", " }")



        String.concat "" anons + nl +
        (match fields with | Anons _ -> $"[<System.Runtime.CompilerServices.Extension; AbstractClass; Sealed>]{nl}" | _ -> "") +
        (if isRec then "and" else "type") + " " + name + encloseGen genConstraints +
        (match fields with | Anons _ -> $" ={nl}{spaces 4}[<System.Runtime.CompilerServices.Extension>]{nl}" | _ -> $" with{nl}") +
        (
            if includeCodecTypeLabel || usedAsInterface.IsSome then
                let prefix =
                    match usedAsInterface with
                    | Some "SubjectId"
                    | Some "LifeEvent" ->
                        // with F# 7 we should be able to eliminate most of interface-based encoding, except subscriptions
                        // were subscriber truly needs to encode Publisher Id and life event via interface
                        suiteInputs.CrossEcosystemTypeLabelPrefix
                    | _ -> ""
                $"{spaces 4}static member TypeLabel () = \"{prefix}{name}\"" + nl + nl
            else ""
        ) +
        let m = "private " + codecPrivateMethod false |> String.replace "private inline " "inline private "
        $"{spaces 4}static member {m} {explicitTypeAnnotation}=" +

        // qualify None for conflicting custom None
        let qualifyOptionNone = cases |> List.exists (fun c -> c.DisplayName = "None")
        if isRecordVsUnion then
            // record
            $"{nl}" + spaces 8 + "codec {"
            + nl + spaces 12 + $"""let! _version = reqWith Codecs.int "__v1" (fun _ -> Some 0)"""
            + nl + spaces 12 + "and!"
            + intercalate $"{nl}{spaces 12}and!" (printFieldBinding <!> toSeq fields) +  nl + spaces 12 + "return " + retOpen + nl + spaces 16 + intercalate (nl + spaces 16) (printFieldConstructor <!> toSeq fields) + nl + spaces 12 + retClose
            + nl + spaces 8 + "}"
        else
            // discriminated unions
            let isSingeCaseUnion = List.length cases = 1
            $"{nl}" + spaces 8 + $"function{nl}"
            + spaces 8 + intercalate (nl + spaces 8) (printDuCase isSingeCaseUnion qualifyOptionNone <!> cases) + nl
            + spaces 8 + "|> mergeUnionCases"
        + nl
        + (
            let s =
                match maybeCodecOfObjCodecDetails with
                | None   ->
                    let r = replace "inline " "" (codecPrivateMethod true)
                    $"{spaces 4}static member {codecOrObjCodecMethod} {explicitTypeAnnotation}= ofObjCodec ({name}.{r})"
                | Some (codecOfObjCodecMethod, objCodecInvocation, initObjCodecInvocation, i) ->
                    let label = """attachCodecTypeLabel ("__type_" + typeLabel)"""
                    let (initMethod, witnessParams) =
                        if length witness_gn = 0 then
                            "Init", genWitnessParams witness_gn
                        else
                            "inline Init", genWitnessParams witness_gn
                    nl +
                        $"    static member {codecOrObjCodecMethod} = {objCodecInvocation true}" + nl +
                        $"    static member {codecOfObjCodecMethod} = ofObjCodec <| {initObjCodecInvocation true}" + nl +
                        $"    static member {initMethod} ({witnessParams}) = initializeInterfaceImplementation<{i}, {name}{encloseGen genLower}> (fun () -> {label} <| {initObjCodecInvocation false})" + nl
            s
          )
        + (
            match implementsStaticInterfaceWithCodecMember with
            | Some (staticInterfaceName, typeArgs) ->
                // TODO: this prints 'UnitOfMeasure arg incorrectly e.g.  MeasureProduct instead of second/meter. Didn't find an easy fix yet
                let args = typeArgs |> List.map (fun arg -> arg.TypeDefinition.DisplayName) |> intercalate ", "
                nl +
                nl +
                $"    interface {staticInterfaceName}<{args}> with" + nl +
                $"         static member Codec () = {name}.get_Codec ()" + nl
            | None ->
                ""
          )

    let witness_gn =
        tys
        |> List.filter (fun x -> toList x.AllInterfaces |> List.exists (fun x -> x.TypeDefinition.DisplayName.StartsWith "IInterfaceCodec"))
        |> List.map (fun x -> x.GenericParameters)
        |> List.sortByDescending (fun gn -> gn.Count)
        |> List.map toList
        |> List.fold (
            fun (acc: list<FSharpGenericParameter>) gn ->
                let extra = gn |> List.filter (fun x -> acc |> List.exists (fun y -> y.Name = x.Name) |> not)
                acc @ extra)
                []

    nl + nl + nl + nl
    + "////////////////////////////////" + nl
    + "// Generated code starts here //" + nl
    + "////////////////////////////////" + nl + nl
    + "#if !FABLE_COMPILER" + nl + nl
    + "open CodecLib" + nl + nl
    + @"#nowarn ""69"" // disable Interface implementations should normally be given on the initial declaration of a type" + nl + nl
    + intercalate (nl+nl) (tys |> List.map (fun x -> printType (isRec x) x.DisplayName (suiteInputs.ShouldIncludeCodecTypeLabel x) (usedAsInterface x) (implementsStaticInterfaceWithCodecMember x) (Nominals (toList x.FSharpFields)) (toList x.UnionCases) (toList x.GenericParameters) witness_gn) )
    + nl
    + "#endif // !FABLE_COMPILER"

let generateCodecs (suiteInputs: SuiteInputs) =

    // delete the previous autogenerated code
    (Directory.GetFiles (suiteInputs.TypesProjectPath, "*.fs", IO.SearchOption.AllDirectories))
    |> Seq.iter
           (fun f ->
                let code = File.ReadAllText f
                let code =
                    Regex
                        .Replace(code, @"\s*\/+[\s\/]+Generated\s+code\s+starts\s+here.*$", "", RegexOptions.Singleline)
                        // uncomment static Codec stub so compiler services can compile it without codec section TODO: can you do better?
                        .Replace ("// static member Codec() = QQQ", "static member Codec() = QQQ")
                File.WriteAllText (f, code, System.Text.Encoding.UTF8))

    let sourceDlls, sourceFiles =
        let xml = ProjSettings.Load(suiteInputs.TypesProjectPath +-+ suiteInputs.TypesProjectName)
        xml.ItemGroups
        |> Array.collect (fun x -> x.ProjectReferences)
        |> Array.map (
            fun x ->
                let projFile = suiteInputs.TypesProjectPath +-+ x.Include
                let commonDenominatorTypesTargetFramework = "net7.0"
                let p = Path.GetDirectoryName projFile +-+ @$"bin/Debug/{commonDenominatorTypesTargetFramework}" +-+ Path.GetFileName projFile
                // note that FSharpCheck needs forward slashes in Linux
                (p.[0..p.Length-7] + "dll").Replace("\\", "/")
            )
        ,
        xml.ItemGroups
        |> Array.collect (fun x -> x.Compiles)
        |> Array.filter (fun c -> (c.Include.Contains "EcosystemDef.fs" |> not) && suiteInputs.TypesProjectSourceShouldCompile c.Include)
        // note that FSharpCheck needs forward slashes in Linux
        |> Array.map (fun x -> (suiteInputs.TypesProjectPath +-+ x.Include).Replace("\\", "/"))


    // Create an interactive checker instance
    let checker = FSharpChecker.Create()

    let projectOptions =
        let sysLib nm =
            let sysDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
            sysDir +-+ nm + ".dll"

        checker.GetProjectOptionsFromCommandLineArgs
           (Inputs.projFileName,
            [|
               yield "--noframework"
               yield "--define:RELEASE"
               yield "--define:NETSTANDARD"
               yield "--define:NETSTANDARD2_0"
               yield "--optimize-"
               yield "--out:" + Inputs.dllName
               yield "--doc:test.xml"
               yield "--warn:3"
               yield "--fullpaths"
               yield "--flaterrors"
               yield "--target:library"

               yield! sourceFiles

               let references =
                 [
                   sysLib "mscorlib"
                   sysLib "System"
                   sysLib "System.Core"
                   sysLib "System.Runtime"
                   sysLib "System.Private.CoreLib"
                   sysLib "System.Collections"
                   sysLib "System.Net.Requests"
                   sysLib "System.Net.WebClient"
                   sysLib "System.Text.RegularExpressions"
                   sysLib "System.Security.Cryptography"
                   sysLib "System.Text.Json"
                   Inputs.numerics
                   Inputs.fsplus
                   Inputs.fleece
                   Inputs.fscore
                   yield! sourceDlls
                 ]
               for r in references do yield "-r:" + r |])



    let wholeProjectResults = checker.ParseAndCheckProject(projectOptions) |> Async.RunSynchronously

    let errors =
        wholeProjectResults.Diagnostics
        |> Seq.filter (fun d -> d.Severity = FSharp.Compiler.Diagnostics.FSharpDiagnosticSeverity.Error)
        |> List.ofSeq

    if errors.Length > 0 then
        printfn "=================> Errors (%i)" (errors.Length)
        if errors.Length >= 2 then
            failwithf "Too many errors %A %A" errors.[0] errors.[1]
        else
            failwithf "The error is: %A" errors.[0]
    else printfn "=================> Code generation for codecs succeeded."

    let shouldSkipCodecAutoGenerate (x: FSharpEntity) =
        seq {
            yield! x.Attributes
            yield! x.BaseType |> Option.map (fun t -> toList t.TypeDefinition.Attributes) |> Option.defaultValue []
            yield! x.AllInterfaces |> Seq.collect (fun t -> t.TypeDefinition.Attributes)
        }
        |> Seq.exists (fun a -> a.AttributeType.CompiledName = "SkipCodecAutoGenerate")

    let codeToGenerate =
        wholeProjectResults.AssemblySignature.Entities
        |> filter (fun x -> x.Attributes |> Seq.exists  (fun x -> x.AttributeType.CompiledName = "CodecAutoGenerate"))
        |> toList
        |> List.map (fun x -> x.ImplementationLocation, toList x.NestedEntities
                                    |> groupBy (fun x -> x.DisplayName)
                                    |> List.map (function (_, x::_) -> x | (_, []) -> failwith "unexpected") // abbreviations come in pairs (abbreviation + original type)
                                    |> filter (fun x -> not x.IsFSharpModule && not x.IsMeasure && (* printfn "type: %s - isAb: %A" x.DisplayName x.IsFSharpAbbreviation;*) not x.IsFSharpAbbreviation && not (shouldSkipCodecAutoGenerate x) && (x.MembersFunctionsAndValues |> forall (fun x -> x.CompiledName <> "Codec" && x.CompiledName <> "get_Codec" ))))
        |> List.filter (fun (_, e) -> e <> [])
        |> List.map (fun (loc, e) -> loc, getCode (loc |> Option.map (fun x -> x.FileName)) e suiteInputs)

    codeToGenerate |> iter (
        function
        | (Some x, y) ->
            let code = File.ReadAllText x.FileName

            // TODO: can we do better ?  before generation must specify stub member for static Codec ()
            let nl =
                x.FileName
                |> tryAutoDetectLineEndings
                |> Option.defaultValue System.Environment.NewLine

            let code = code.Replace(@"static member Codec() = QQQ", "// static member Codec() = QQQ")
            File.WriteAllText (x.FileName, code, System.Text.Encoding.UTF8)
            File.AppendAllText (x.FileName, y)
        | _ -> ())

