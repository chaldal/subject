[<AutoOpen>]
module LibClient.Components.Money

open Fable.React

open LibClient
open ReactXP.Components
open ReactXP.Styles

module LC =
    module Money =
        type Symbol =
        | Prefix of string
        | Suffix of string
        with
            member this.TryGetPrefix : Option<string> =
                match this with
                | Prefix value -> Some value
                | _            -> None

            member this.TryGetSuffix : Option<string> =
                match this with
                | Suffix value -> Some value
                | _            -> None

        type Value =
        | PrivateInternalRepresentation of decimal
        with
            static member Of(value: PositiveDecimal): Value =
                PrivateInternalRepresentation value.Value

            static member Of(value: UnsignedDecimal): Value =
                PrivateInternalRepresentation value.Value

            static member Of(value: decimal): Value =
                PrivateInternalRepresentation value

        type Format =
        | WithThousandSeparator of Decimals: int
        | Custom of string
        with
            member this.String : string =
                match this with
                | Custom value                   -> value
                | WithThousandSeparator 0        -> "0"
                | WithThousandSeparator decimals -> "0." + (String.replicate decimals "0")

open LC.Money

[<RequireQualifiedAccess>]
module private Styles =
    let view =
        makeViewStyles {
            FlexDirection.Row
            AlignItems.Center
        }

type LibClient.Components.Constructors.LC with
    [<Component>]
    static member Money(value: Value, symbol: Symbol, ?format: Format, ?styles: array<TextStyles>, ?key: string) : ReactElement =
        key |> ignore

        let format = defaultArg format (Format.WithThousandSeparator 2)
        let styles = styles |> Option.defaultValue ([||])

        RX.View(
            styles = [| Styles.view |],
            children =
                elements {
                    match symbol.TryGetPrefix with
                    | Some prefix ->
                        LC.Text (prefix, styles = styles)
                    | None ->
                        ()

                    let (PrivateInternalRepresentation v) = value
                    LC.Text (format.String |> v.ToString, styles = styles)

                    match symbol.TryGetSuffix with
                    | Some suffix ->
                        LC.Text (suffix, styles = styles)
                    | None ->
                        ()
                }
        )