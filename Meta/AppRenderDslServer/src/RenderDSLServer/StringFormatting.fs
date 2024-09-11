module RenderDSLServer.StringFormatting

let findMaxes (tuples: List<string * string * string>) : Option<int * int * int> =
    match tuples with
    | [] -> None
    | _  ->
        tuples
        |> List.fold
            (fun (acc1, acc2, acc3) (curr1, curr2, curr3) ->
                (
                    System.Math.Max(acc1, curr1.Length),
                    System.Math.Max(acc2, curr2.Length),
                    System.Math.Max(acc3, curr3.Length)
                )
            )
            (0, 0, 0)
        |> Some

let pad (value: string) (length: int) : string =
    value + (String.replicate (System.Math.Max(0, length - value.Length)) " ")
