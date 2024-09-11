module ``Union Case Tests``

open Xunit
open FsUnit.Xunit

type SomeRecord = {
    Whatever: string
    DoesNotMatter: Option<int>
    WhoCares: List<Option<string>>
}

let someRecordInstance = {
    Whatever = "whatever"
    DoesNotMatter = Some 42
    WhoCares = [None; Some "foo"]
}

type SomePrivateRecord = private {
    Whatever: string
    DoesNotMatter: Option<int>
    WhoCares: List<Option<string>>
}

module SomePrivateRecord =
    let someRecordInstance = {
        Whatever = "whatever"
        DoesNotMatter = Some 42
        WhoCares = [None; Some "foo"]
    }

type SomeUnion =
| NoFields
| SingleField of int
| MultiField of int * string

type SomePrivateUnion =
    private
    | NoFields
    | SingleField of int
    | MultiField of int * string

module SomePrivateUnion =
    let noFields = SomePrivateUnion.NoFields

    let singleField v = SomePrivateUnion.SingleField v

    let multiField v1 v2 = SomePrivateUnion.MultiField (v1, v2)

type SingleCaseUnionWithNoFields =
| Case
with interface Union

type SingleCaseUnionWithSinglePrimitiveField =
| Case of int
with interface Union

type SingleCaseUnionWithSingleRecordField =
| Case of SomeRecord
with interface Union

type SingleCaseUnionWithSinglePrivateRecordField =
| Case of SomePrivateRecord
with interface Union

type SingleCaseUnionWithSingleUnionField =
| Case of SomeUnion
with interface Union

type SingleCaseUnionWithSinglePrivateUnionField =
| Case of SomePrivateUnion
with interface Union

type SingleCaseUnionWithMultiplePrimitiveFields =
| Case of int * float * string
with interface Union

type SingleCaseUnionWithMultipleVariedFields =
| Case of int * SomeRecord * SomeUnion * List<Option<SomeRecord>>
with interface Union

type MultiCaseUnion =
| Case1
| Case2 of int
| Case3 of SomeRecord
| Case4 of SomePrivateRecord
| Case5 of SomeUnion
| Case6 of SomePrivateUnion
| Case7 of int * float * string
| Case8 of int * SomeRecord * SomeUnion * List<Option<SomeRecord>>
with interface Union

[<Fact>]
let ``ofCase works against single-case union with no fields`` () =
    let result = UnionCase.ofCase SingleCaseUnionWithNoFields.Case
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, primitive field when field's value is specified`` () =
    let result = UnionCase.ofCase (SingleCaseUnionWithSinglePrimitiveField.Case 42)
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, primitive field when field's value is not specified`` () =
    let result = UnionCase.ofCase SingleCaseUnionWithSinglePrimitiveField.Case
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, record field when field's value is specified`` () =
    let result = UnionCase.ofCase (SingleCaseUnionWithSingleRecordField.Case someRecordInstance)
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, record field when field's value is not specified`` () =
    let result = UnionCase.ofCase SingleCaseUnionWithSingleRecordField.Case
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, private record field when field's value is specified`` () =
    let result = UnionCase.ofCase (SingleCaseUnionWithSinglePrivateRecordField.Case SomePrivateRecord.someRecordInstance)
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, private record field when field's value is not specified`` () =
    let result = UnionCase.ofCase SingleCaseUnionWithSinglePrivateRecordField.Case
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, union field when field's value is specified as a union case with no fields`` () =
    let result = UnionCase.ofCase (SingleCaseUnionWithSingleUnionField.Case SomeUnion.NoFields)
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, union field when field's value is specified as a union case with one field`` () =
    let result = UnionCase.ofCase (SingleCaseUnionWithSingleUnionField.Case (SomeUnion.SingleField 42))
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, union field when field's value is specified as a union case with multiple fields`` () =
    let result = UnionCase.ofCase (SingleCaseUnionWithSingleUnionField.Case (SomeUnion.MultiField (42, "foo")))
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, union field when field's value is not specified`` () =
    let result = UnionCase.ofCase SingleCaseUnionWithSingleUnionField.Case
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, private union field when field's value is specified as a union case with no fields`` () =
    let result = UnionCase.ofCase (SingleCaseUnionWithSinglePrivateUnionField.Case SomePrivateUnion.noFields)
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, private union field when field's value is specified as a union case with one field`` () =
    let result = UnionCase.ofCase (SingleCaseUnionWithSinglePrivateUnionField.Case (SomePrivateUnion.singleField 42))
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, private union field when field's value is specified as a union case with multiple fields`` () =
    let result = UnionCase.ofCase (SingleCaseUnionWithSinglePrivateUnionField.Case (SomePrivateUnion.multiField 42 "foo"))
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with single, private union field when field's value is not specified`` () =
    let result = UnionCase.ofCase SingleCaseUnionWithSinglePrivateUnionField.Case
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with multiple, primitive fields when field values are specified`` () =
    let result = UnionCase.ofCase (SingleCaseUnionWithMultiplePrimitiveFields.Case (42, 42.0, "42"))
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with multiple, primitive fields when field values are not specified`` () =
    let result = UnionCase.ofCase SingleCaseUnionWithMultiplePrimitiveFields.Case
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with multiple, varied fields when field values are specified`` () =
    let result = UnionCase.ofCase (SingleCaseUnionWithMultipleVariedFields.Case (42, someRecordInstance, SomeUnion.NoFields, [ None; Some someRecordInstance ]))
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against single-case union with multiple, varied fields when field values are not specified`` () =
    let result = UnionCase.ofCase SingleCaseUnionWithMultipleVariedFields.Case
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case")

[<Fact>]
let ``ofCase works against multi-case union for case with no fields`` () =
    let result = UnionCase.ofCase MultiCaseUnion.Case1
    (result.TagNumber, result.CaseName)
    |> should equal (0, "Case1")

[<Fact>]
let ``ofCase works against multi-case union for case with single, primitive field when field's value is specified`` () =
    let result = UnionCase.ofCase (MultiCaseUnion.Case2 42)
    (result.TagNumber, result.CaseName)
    |> should equal (1, "Case2")

[<Fact>]
let ``ofCase works against multi-case union for case with single, primitive field when field's value is not specified`` () =
    let result = UnionCase.ofCase MultiCaseUnion.Case2
    (result.TagNumber, result.CaseName)
    |> should equal (1, "Case2")

[<Fact>]
let ``ofCase works against multi-case union for case with single, record field when field's value is specified`` () =
    let result = UnionCase.ofCase (MultiCaseUnion.Case3 someRecordInstance)
    (result.TagNumber, result.CaseName)
    |> should equal (2, "Case3")

[<Fact>]
let ``ofCase works against multi-case union for case with single, record field when field's value is not specified`` () =
    let result = UnionCase.ofCase MultiCaseUnion.Case3
    (result.TagNumber, result.CaseName)
    |> should equal (2, "Case3")

[<Fact>]
let ``ofCase works against multi-case union for case with single, private record field when field's value is specified`` () =
    let result = UnionCase.ofCase (MultiCaseUnion.Case4 SomePrivateRecord.someRecordInstance)
    (result.TagNumber, result.CaseName)
    |> should equal (3, "Case4")

[<Fact>]
let ``ofCase works against multi-case union for case with single, private record field when field's value is not specified`` () =
    let result = UnionCase.ofCase MultiCaseUnion.Case4
    (result.TagNumber, result.CaseName)
    |> should equal (3, "Case4")

[<Fact>]
let ``ofCase works against multi-case union for case with single, union field when field's value is specified as a union case with no fields`` () =
    let result = UnionCase.ofCase (MultiCaseUnion.Case5 SomeUnion.NoFields)
    (result.TagNumber, result.CaseName)
    |> should equal (4, "Case5")

[<Fact>]
let ``ofCase works against multi-case union for case with single, union field when field's value is specified as a union case with one field`` () =
    let result = UnionCase.ofCase (MultiCaseUnion.Case5 (SomeUnion.SingleField 42))
    (result.TagNumber, result.CaseName)
    |> should equal (4, "Case5")

[<Fact>]
let ``ofCase works against multi-case union for case with single, union field when field's value is specified as a union case with multiple fields`` () =
    let result = UnionCase.ofCase (MultiCaseUnion.Case5 (SomeUnion.MultiField (42, "foo")))
    (result.TagNumber, result.CaseName)
    |> should equal (4, "Case5")

[<Fact>]
let ``ofCase works against multi-case union for case with single, union field when field's value is not specified`` () =
    let result = UnionCase.ofCase MultiCaseUnion.Case5
    (result.TagNumber, result.CaseName)
    |> should equal (4, "Case5")

[<Fact>]
let ``ofCase works against multi-case union for case with single, private union field when field's value is specified as a union case with no fields`` () =
    let result = UnionCase.ofCase (MultiCaseUnion.Case6 SomePrivateUnion.noFields)
    (result.TagNumber, result.CaseName)
    |> should equal (5, "Case6")

[<Fact>]
let ``ofCase works against multi-case union for case with single, private union field when field's value is specified as a union case with one field`` () =
    let result = UnionCase.ofCase (MultiCaseUnion.Case6 (SomePrivateUnion.singleField 42))
    (result.TagNumber, result.CaseName)
    |> should equal (5, "Case6")

[<Fact>]
let ``ofCase works against multi-case union for case with single, private union field when field's value is specified as a union case with multiple fields`` () =
    let result = UnionCase.ofCase (MultiCaseUnion.Case6 (SomePrivateUnion.multiField 42 "foo"))
    (result.TagNumber, result.CaseName)
    |> should equal (5, "Case6")

[<Fact>]
let ``ofCase works against multi-case union for case with single, private union field when field's value is not specified`` () =
    let result = UnionCase.ofCase MultiCaseUnion.Case6
    (result.TagNumber, result.CaseName)
    |> should equal (5, "Case6")

[<Fact>]
let ``ofCase works against multi-case union for case with multiple, primitive fields when field values are specified`` () =
    let result = UnionCase.ofCase (MultiCaseUnion.Case7 (42, 42.0, "42"))
    (result.TagNumber, result.CaseName)
    |> should equal (6, "Case7")

[<Fact>]
let ``ofCase works against multi-case union for case with multiple, primitive fields when field values are not specified`` () =
    let result = UnionCase.ofCase MultiCaseUnion.Case7
    (result.TagNumber, result.CaseName)
    |> should equal (6, "Case7")

[<Fact>]
let ``ofCase works against multi-case union for case with multiple, varied fields when field values are specified`` () =
    let result = UnionCase.ofCase (MultiCaseUnion.Case8 (42, someRecordInstance, SomeUnion.NoFields, [ None; Some someRecordInstance ]))
    (result.TagNumber, result.CaseName)
    |> should equal (7, "Case8")

[<Fact>]
let ``ofCase works against multi-case union for case with multiple, varied fields when field values are not specified`` () =
    let result = UnionCase.ofCase MultiCaseUnion.Case8
    (result.TagNumber, result.CaseName)
    |> should equal (7, "Case8")
