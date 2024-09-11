[<AutoOpen>]
module LibClient.ColorModule

open System
open LibClient.Logging

let private hexRegex  = System.Text.RegularExpressions.Regex("^#[0-9a-f]{6}$")
let private greyRegex = System.Text.RegularExpressions.Regex("^[0-9a-f]{2}$")

[<RequireQualifiedAccess>]
type Color =
| InternalString of string
| InternalHex    of string
| Rgb            of int * int * int
| Rgba           of int * int * int * float
| BlackAlpha     of float
| WhiteAlpha     of float
| Black
| White
| Transparent
| DevOrange
| DevPink
| DevGreen
| DevGrey
| DevLightGrey
| DevBlue
| DevRed
with
    member this.ToCssString : string =
        this.ToReactXPString

    member this.ToReactXPString : string =
        match this with
        | InternalString value -> value
        | InternalHex  value   -> if value.StartsWith "#" then value else failwith (sprintf "Color %s does not start with #, which is expected of Color.Hex values" value)
        | Rgb  (r, g, b)       -> sprintf "rgb(%d,%d,%d)" r g b
        | Rgba (r, g, b, a)    -> sprintf "rgba(%d,%d,%d,%f)" r g b a
        | BlackAlpha a         -> sprintf "rgba(0,0,0,%f)" a
        | WhiteAlpha a         -> sprintf "rgba(255,255,255,%f)" a
        | Black                -> "black"
        | White                -> "white"
        | Transparent          -> "rgba(0,0,0,0)"
        | DevOrange            -> "orange"
        | DevPink              -> "pink"
        | DevGreen             -> "green"
        | DevGrey              -> "grey"
        | DevLightGrey         -> "lightgrey"
        | DevBlue              -> "blue"
        | DevRed               -> "red"

    member this.WithOpacity (opacity: float) : Color =
        match this with
        | Rgb (r, g, b)
        | Rgba (r, g, b, _) ->
            Rgba (r, g, b, opacity)
        | BlackAlpha _ ->
            BlackAlpha opacity
        | WhiteAlpha _ ->
            WhiteAlpha opacity
        | InternalHex value ->
            let value = value.TrimStart('#')
            let value =
                if value.Length = 3 then
                    // Expand short form, which might technically be present if InternalHex is constructed directly.
                    value
                    |> Seq.zip value
                    |> Seq.map (fun (a, b) ->
                        String([| a; b |])
                    )
                    |> String.concat ""
                else
                    value

            let opacityByte = (max 0.0 (min 1.0 opacity)) * 255.0 |> byte
            let opacityHex = opacityByte.ToString("x2")
            InternalHex $"#{value}{opacityHex}"
        | _ ->
            failwith "Cannot adjust opacity of this color"

    static member Hex (value: string) : Color =
        match hexRegex.IsMatch value with
        | false -> failwith (sprintf "Color is expected to match #[0-oa-f]{6}, but was given %s" value)
        | true ->
            match value with
            | "#ffffff" -> Log.Error (sprintf "Use Color.White instead of Color.Hex \"%s\"" value)
            | "#000000" -> Log.Error (sprintf "Use Color.Black instead of Color.Hex \"%s\"" value)
            | _ ->
                if value.[1..2] = value.[3..4] && value.[1..2] = value.[5..6] then
                    Log.Error (sprintf "Use Color.Grey \"%s\" instead of Color.Hex \"%s\"" value.[1..2] value)
                else
                    ()

            InternalHex value

    static member Grey (value: string) : Color =
        match greyRegex.IsMatch value with
        | false -> failwith (sprintf "Color is expected to match #[0-oa-f]{2}, but was given %s" value)
        | true ->
            match value with
            | "ff" -> Log.Error (sprintf "Use Color.White instead of Color.Grey \"%s\"" value)
            | "00" -> Log.Error (sprintf "Use Color.Black instead of Color.Grey \"%s\"" value)
            | _    -> ()

            InternalHex (sprintf "#%s%s%s" value value value)
