namespace LibClient.MessageTemplates

// Much of this is based on https://messagetemplates.org/, though we only need rudimentary support (simple, named properties without formatting support).

open System.Text

type NamedHole =
    {
        Name: string
        TemplateStartIndex: int
    }

type MessageTemplate = MessageTemplate of string
with
    member this.Value: string =
        let (MessageTemplate templateStr) = this
        templateStr

    member this.GetNamedHoles(): seq<NamedHole> =
        seq {
            let templateStr = this.Value
            let mutable maybeCurrentNamedHoleStartIndex = None
            let mutable index = 0

            while index < templateStr.Length do
                let ch = templateStr[index]

                match ch, maybeCurrentNamedHoleStartIndex with
                | '{', None ->
                    if index = templateStr.Length - 1 || templateStr[index + 1] = '{' then
                        // Not a named hole - it's an escaped opening brace, or has no terminating closing brace. Either way we jump over the escaped
                        // character.
                        index <- index + 1
                    else
                        maybeCurrentNamedHoleStartIndex <- Some index
                | '}', Some currentNamedHoleStartIndex ->
                    if index - currentNamedHoleStartIndex > 1 then
                        yield
                            {
                                Name = templateStr.Substring(currentNamedHoleStartIndex + 1, index - currentNamedHoleStartIndex - 1)
                                TemplateStartIndex = currentNamedHoleStartIndex
                            }
                    else
                        // Was just an opening and closing brace with no name in it.
                        ()

                    maybeCurrentNamedHoleStartIndex <- None
                | _ -> ()

                index <- index + 1
        }

type Property =
    {
        NamedHole: NamedHole
        Value: obj
    }

type Event =
    {
        MessageTemplate: MessageTemplate
        Arguments: obj[]
    }
with
    member internal this.GetNamedHolesAndValues(): seq<ZipOuterResult<NamedHole, obj>> =
        let namedHoles = this.MessageTemplate.GetNamedHoles()
        Seq.zipOuter namedHoles this.Arguments

    member this.GetProperties(): seq<Property> =
        this.GetNamedHolesAndValues()
        |> Seq.choose (fun zipOuterResult ->
            match zipOuterResult with
            | ZipOuterResult.BothPresent (namedHole, value) ->
                {
                    NamedHole = namedHole
                    Value = value
                }
                |> Some
            | _ -> None
        )

    member this.Append(stringBuilder: StringBuilder, formatProperty: Property -> string): unit =
        let messageTemplateStr = this.MessageTemplate.Value

        if this.Arguments.Length = 0 then
            stringBuilder.Append(messageTemplateStr)
            |> ignore
        else
            let mutable templateIndex = 0

            // Fable's StringBuilder doesn't include support for remove/insert, so we build up our formatted message rather than start from the template
            // and replace named holes.
            this
                .GetProperties()
                |> Seq.iter (fun property ->
                    let namedHole = property.NamedHole

                    stringBuilder
                        .Append(messageTemplateStr.Substring(templateIndex, namedHole.TemplateStartIndex - templateIndex))
                        .Append(formatProperty property)
                    |> ignore

                    templateIndex <- namedHole.TemplateStartIndex + namedHole.Name.Length + 2
                )

            stringBuilder.Append(messageTemplateStr.Substring(templateIndex))
            |> ignore

    member this.Format(formatProperty: Property -> string): string =
        let sb = StringBuilder()
        this.Append(sb, formatProperty)
        sb.ToString()

    override this.ToString(): string =
        this.Format(fun property -> string property.Value)
