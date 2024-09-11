module AppRenderDslCompiler.Code

// TODO move to LibDsl some kind code helpers library
let toCamelCase (s: string) : string =
    s.Substring(0, 1).ToLower() + s.Substring(1)

let private keywordsToEscape = Set.ofList [
    "to"
    "with"
    "type"
    "for"
    "try"
    "let"
    "else"
    "then"
]

let private toCamelCaseEscaped (s: string) : string =
    let camelCased = toCamelCase s
    if keywordsToEscape.Contains camelCased then
        "``" + camelCased + "``"
    else
        camelCased

let makeFunctionParameterName (propName: string) : string =
    if propName.StartsWith "?" then
        "?" + toCamelCaseEscaped (propName.Substring 1)
    else
        if propName.EndsWith "Option" then
            "?" + toCamelCaseEscaped (propName.Substring (0, propName.Length - "Option".Length))
        else
            toCamelCaseEscaped propName

let legacyMakeFunctionParameterName (propName: string) : string =
    if propName.StartsWith "?" then
        "?p" + (propName.Substring 1)
    else
        if propName.EndsWith "Option" then
            "?p" + (propName.Substring (0, propName.Length - "Option".Length))
        else
            "p" + propName
