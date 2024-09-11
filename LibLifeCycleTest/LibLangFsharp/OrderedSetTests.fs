module LibLifeCycleTest.OrderedSetTests

open Xunit
open FsUnit.Xunit

[<Fact>]
let ``Add item``() =
    let input = [1; 3; 4]
    let expected = [1; 3; 4; 2]

    input
    |> OrderedSet.ofList
    |> (fun orderedSet -> orderedSet.Add 2)
    |> OrderedSet.toList
    |> should equal expected

[<Fact>]
let ``Add items``() =
    let input = [1; 3; 4]
    let expected = [1; 3; 4; 2; 5]

    input
    |> OrderedSet.ofList
    |> (fun orderedSet -> orderedSet.Add [2; 5])
    |> OrderedSet.toList
    |> should equal expected

[<Fact>]
let ``Remove item``() =
    let input = [1; 3; 4; 2]
    let expected = [1; 3; 4]

    input
    |> OrderedSet.ofList
    |> (fun orderedSet -> orderedSet.Remove 2)
    |> OrderedSet.toList
    |> should equal expected

[<Fact>]
let ``Toggle item``() =
    let input = [1; 3; 4; 2]
    let expected = [1; 3; 4]

    input
    |> OrderedSet.ofList
    |> (fun orderedSet -> orderedSet.Toggle 2)
    |> OrderedSet.toList
    |> should equal expected

    expected
    |> OrderedSet.ofList
    |> (fun orderedSet -> orderedSet.Toggle 2)
    |> OrderedSet.toList
    |> should equal input

[<Fact>]
let ``Contains item``() =
    let input = [1; 3; 4; 2]

    input
    |> OrderedSet.ofList
    |> (fun orderedSet -> orderedSet.Contains 4)
    |> should equal true

[<Fact>]
let ``Contains one of``() =
    let input = [1; 3; 4; 2]
    let items = [4; 5]

    input
    |> OrderedSet.ofList
    |> (fun orderedSet -> orderedSet.ContainsOneOf items)
    |> should equal true

[<Fact>]
let ``Not contains item``() =
    let input = [1; 3; 4; 2]

    input
    |> OrderedSet.ofList
    |> (fun orderedSet -> orderedSet.NotContains 5)
    |> should equal true

[<Fact>]
let ``Is empty``() =
    let input = []

    input
    |> OrderedSet.ofList
    |> (fun orderedSet -> orderedSet.IsEmpty)
    |> should equal true

[<Fact>]
let ``Is not empty``() =
    let input = [1; 3; 4; 2]

    input
    |> OrderedSet.ofList
    |> (fun orderedSet -> orderedSet.IsNonempty)
    |> should equal true

[<Fact>]
let ``Count``() =
    let input = [1; 3; 4; 2]

    input
    |> OrderedSet.ofList
    |> (fun orderedSet -> orderedSet.Count)
    |> should equal 4

[<Fact>]
let ``Filter item``() =
    let input = [1; 3; 4; 2]
    let expected = [4; 2]

    input
    |> OrderedSet.ofList
    |> OrderedSet.filter (fun item -> item % 2 = 0)
    |> OrderedSet.toList
    |> should equal expected

[<Fact>]
let ``Codec test after removal of an item``() =
    let input = [1; 3; 4; 2] |> OrderedSet.ofList
    let expected = [1; 3; 4] |> OrderedSet.ofList

    input
    |> (fun orderedSet -> orderedSet.Remove 2)
    |> should equal expected
