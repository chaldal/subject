module AppEggShellGallery.Components.Content.Forms

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Gender =
| Male
| Female
| Other
| Undisclosed

type Profile = {
    Name:      NonemptyString
    Age:       PositiveInteger
    Gender:    Gender
    WantsSpam: bool
}


open LibClient.Components.Form.Base.Types
type PickerItem<'T> = LibClient.Components.Legacy.Input.Picker.PickerItem<'T>

[<RequireQualifiedAccess>]
type Field = Name | Age | Gender | WantsSpam

type Acc = {
    // This is how you can provide choices to a Picker. It's also possible
    // to have just a simple `let choices = ...` binding in the .typext.fs
    // file for that, but often you'll want the picker choices to be based
    // other partially filled fields, so in general putting them in the Acc
    // solves a wide array of use cases.
    GenderChoices: List<PickerItem<Gender>>
    Name:          Option<NonemptyString>
    Age:           LibClient.Components.Input.PositiveInteger.Value
    Gender:        Option<Gender>
    WantsSpam:     bool
} with
    static member Initial : Acc =
        {
            GenderChoices = [Male; Female; Other; Undisclosed] |> List.map (fun g -> { Item = g; Label = g.ToString() })
            Name          = None
            Age           = LibClient.Components.Input.PositiveInteger.empty
            Gender        = None
            WantsSpam     = true
        }

    interface AbstractAcc<Field, Profile> with
        member this.Validate () : Result<Profile, ValidationErrors<Field>> = validateForm {
            // using and! and not let! for successive fields is important, it lets you
            // collect all errors in one go, as opposed to one at a time which would lead
            // the the poor user experience of fix-one-error-get-notified-of-the-next-one.
            let! name   = Forms.GetFieldValue2 this.Name       Field.Name
            and! age    = Forms.GetFieldValue2 this.Age.Result Field.Age
            and! gender = Forms.GetFieldValue2 this.Gender     Field.Gender

            return {
                Name      = name
                Age       = age
                Gender    = gender
                WantsSpam = this.WantsSpam // if no validation is necessary, we just assign the value
            }
        }

type Estate = {
    SomeEphemeralValue: int
}

type Forms(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Forms>("AppEggShellGallery.Components.Content.Forms", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        SomeEphemeralValue = 42
    }

and Actions(_this: Forms) =
    // Typically you'll be submitting the form over the wire, which is an async, potentially
    // errorful operation, so the submit function's is required to reflect that.
    member _.Submit (_profile: Profile) (_e: ReactEvent.Action) () : UDActionResult = async {
        Action.alert "A profile record successfully input!"
        return Ok ()
    }

let Make = makeConstructor<Forms, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate