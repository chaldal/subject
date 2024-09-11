module AppEggShellGallery.Components.Content.Input.UnsignedInteger

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Estate = {
    Values: Map<string, LibClient.Components.Input.UnsignedInteger.Value>
}

type UnsignedInteger(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, UnsignedInteger>("AppEggShellGallery.Components.Content.Input.UnsignedInteger", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
            Values = Map.ofList [
                ("A",            "3.14"      )
                ("A.Zero",       "0"         )
                ("A.NegZero",    "-0"        )
                ("A.MAX",        "2147483647")
                ("A.Overflow",   "2147483648")
                ("A.Whitespace", "   1"      )
                ("B",            "thirteen"  )
                ("C",            "-13"       )
                ("D",            ""          )
                ("E",            "12"        )
                ("F",            "11"        )
            ]
            |> Map.map (fun _ v -> v |> NonemptyString.ofString |> LibClient.Components.Input.UnsignedInteger.parse)
        }

and Actions(this: UnsignedInteger) =
    member _.OnChange (key: string) (value: LibClient.Components.Input.UnsignedInteger.Value) : unit =
        this.SetEstate (fun estate _ -> { estate with Values = estate.Values.AddOrUpdate (key, value) })

let Make = makeConstructor<UnsignedInteger, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate