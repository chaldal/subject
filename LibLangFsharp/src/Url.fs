[<AutoOpen>]

module Url

open System.Text.RegularExpressions

let urlRegex = Regex(@"^([a-z][a-z0-9\+\.-]*):(\/\/[\w|\.|\-]*)?(\/[\w|%|\.|\-|\/]*)?(\?[\w|\-|=|\?|&]*)?(#[\w|-|\?|&]*)?$")
let tryParse (url: string) =
    let m = urlRegex.Match url
    if m.Success then
        match List.tail [ for g in m.Groups -> g.Value ] with
        | [ theProtocol; theHost; thePath; theQuery; theFragment ] ->
            Some
                {|
                    Protocol = theProtocol
                    Host = theHost.TrimStart('/').TrimStart('/')
                    Path = thePath
                    Query = theQuery.TrimStart('?')
                    Fragment = theFragment.TrimStart('#')
                |}
        | _ ->
            None
    else
        None


let regexShouldParseAllValidUrl () =
    let validUrls =
        [
            "http://example.app"
            "https://example.app"
            "https://example.app/dish/700_gm_dim_er_halwa"
            "https://example.app/dish/6_pieces_%E0%A6%96_%E0%A6%B0%E0%A6%B8_%E0%A6%AD_%E0%A6%AA_%E0%A6%AA_%E0%A6%A0_"
            "https://example.app/dish/%E0%A7%AB%E0%A7%A6%E0%A7%A6_%E0%A6%97%E0%A7%8D%E0%A6%B0_%E0%A6%AE_%E0%A6%86%E0%A6%B2%E0%A7%81_%E0%A6%8F%E0%A6%AC_%E0%A6%9F%E0%A6%AE_%E0%A6%9F_%E0%A6%A6_%E0%A7%9F_%E0%A6%B6_%E0%A6%AE_%E0%A6%9B_%E0%A6%AD%E0%A7%81%E0%A6%A8_"
            "https://example.app/dish/৫০০_গ্র_ম_আলু_এব_টম_ট_দ_য়_শ_ম_ছ_ভুন_"
        ]

    validUrls
    |> List.tryFind(
        fun url ->
            match tryParse url with
            | Some _ -> false
            | None   -> true
    )
    |> fun option ->
        match option with
        | None       -> Ok "Success: Regex validate all valid url"
        | Some value -> Error value