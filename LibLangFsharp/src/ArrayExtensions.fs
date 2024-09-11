[<AutoOpen>]
module ArrayExtensions

module Array =
    let maybeAppend<'T> (baseStyles: array<'T>) (maybeAdditionalStyles: Option<array<'T>>) : array<'T> =
        match maybeAdditionalStyles with
        | Some additionalStyles -> Array.append baseStyles additionalStyles
        | None                  -> baseStyles

    let ofOneItem<'T> (item: 'T) : array<'T> =
        [|item|]