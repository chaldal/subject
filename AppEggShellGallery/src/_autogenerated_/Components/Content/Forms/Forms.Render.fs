module AppEggShellGallery.Components.Content.FormsRender

module FRH = Fable.React.Helpers
module FRP = Fable.React.Props
module FRS = Fable.React.Standard


open LibClient.Components
open LibRouter.Components
open ThirdParty.Map.Components
open ReactXP.Components
open ThirdParty.Recharts.Components
open ThirdParty.Showdown.Components
open ThirdParty.SyntaxHighlighter.Components
open LibUiAdmin.Components
open AppEggShellGallery.Components

open LibLang
open LibClient
open LibClient.Services.Subscription
open LibClient.RenderHelpers
open LibClient.Chars
open LibClient.ColorModule
open LibClient.Responsive
open AppEggShellGallery.RenderHelpers
open AppEggShellGallery.Navigation
open AppEggShellGallery.LocalImages
open AppEggShellGallery.Icons
open AppEggShellGallery.AppServices
open AppEggShellGallery

open AppEggShellGallery.Components.Content.Forms



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.Forms.Props, estate: AppEggShellGallery.Components.Content.Forms.Estate, pstate: AppEggShellGallery.Components.Content.Forms.Pstate, actions: AppEggShellGallery.Components.Content.Forms.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.Form.Base"),
        displayName = ("Forms"),
        notes =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "ReactXP.Components.View"
                    ReactXP.Components.Constructors.RX.View(
                        children =
                            [|
                                makeTextNode2 __parentFQN "We think of forms as a chunk of UI whose job is to produce a value of type 'T.\n            The main building block for a form is the accumulator, shortened to Acc in all of our code.\n            The job of the accumulator is to hold on to the partially filled form's data, as well as\n            provide the Validate function, which produces either an Ok 'T, or an Error ValidationErrors.\n            To help bind validation errors to concrete fields, each form needs a Fields discriminated\n            union type defined."
                            |]
                    )
                    let __parentFQN = Some "ReactXP.Components.View"
                    ReactXP.Components.Constructors.RX.View(
                        children =
                            [|
                                makeTextNode2 __parentFQN "Form elements are regular input elements wrapped to bind them to the form's validation\n            functionality. Currently we have Text, PositiveInteger, UnsignedDecimal, and Picker wrapped.\n            Additional components should be wrapped as and when necessary."
                            |]
                    )
                |]),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                        verticalAlignment = (AppEggShellGallery.Components.ComponentSample.VerticalAlignment.Top),
                        code =
                            (
                                AppEggShellGallery.Components.ComponentSample.Children
                                    (
                                            (castAsElementAckingKeysWarning [|
                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                    language = (AppEggShellGallery.Components.Code.Fsharp),
                                                    children =
                                                        [|
                                                            @"
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
type PickerItem<'T> = LibClient.Components.Form.Legacy.Input.Picker.PickerItem<'T>

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


// Typically you'll be submitting the form over the wire, which is an async, potentially
// errorful operation, so the submit function's is required to reflect that.
member _.Submit (profile: Profile) () : UDActionResult = async {
    Action.alert ""A profile record successfully input!""
    return Ok ()
}
            "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                                let __parentFQN = Some "AppEggShellGallery.Components.Code"
                                                AppEggShellGallery.Components.Constructors.Ui.Code(
                                                    language = (AppEggShellGallery.Components.Code.Render),
                                                    children =
                                                        [|
                                                            @"
                <LC.Form.Base
                 Accumulator='~ManageInternallyInitializingWith Acc.Initial'
                 Submit='actions.Submit'
                 rt-prop-children='Content(form: ~FormHandle&lt;Field, Acc, Profile&gt;)'>
                    <LC.Input.Text
                     Label='""Name""'
                     Validity='form.FieldValidity Field.Name'
                     Value='form.Acc.Name'
                     OnEnterKeyPress='ReactEvent.Action.Make >> form.TrySubmitLowLevel'
                     OnChange='fun value -> form.UpdateAcc (fun acc -> { acc with Name = value })'/>

                    <LC.Input.PositiveInteger
                     Label='""Age""'
                     Validity='form.FieldValidity Field.Age'
                     Value='form.Acc.Age'
                     OnChange='fun value -> form.UpdateAcc (fun acc -> { acc with Age = value })'/>

                    <LC.Legacy.Input.Picker
                     Label='""Gender""'
                     Validity='form.FieldValidity Field.Gender'
                     Items='form.Acc.GenderChoices'
                     Value='form.Acc.Gender |> Option.map ~ByItem'
                     OnChange='~CannotUnselect (fun (_, value) -> form.UpdateAcc (fun acc -> { acc with Gender = Some value }))'/>

                    <LC.Input.Checkbox
                     Label='~String ""Subscribe to email""'
                     Value='Some form.Acc.WantsSpam'
                     OnChange='fun value -> form.UpdateAcc ( fun acc -> { acc with WantsSpam = value})'
                     Validity='form.FieldValidity Field.WantsSpam'/>

                    <LC.Buttons rt-fs='true'>
                        <LC.Button
                         Label='""Submit""'
                         State='^ form.TrySubmit'/>
                    </LC.Buttons>
                </LC.Form.Base>
            "
                                                            |> makeTextNode2 __parentFQN
                                                        |]
                                                )
                                            |])
                                    )
                            ),
                        visuals =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "LibClient.Components.Form.Base"
                                    LibClient.Components.Constructors.LC.Form.Base(
                                        submit = (actions.Submit),
                                        accumulator = (LibClient.Components.Form.Base.ManageInternallyInitializingWith Acc.Initial),
                                        content =
                                            (fun (form: LibClient.Components.Form.Base.FormHandle<Field, Acc, Profile>) ->
                                                    (castAsElementAckingKeysWarning [|
                                                        let __parentFQN = Some "LibClient.Components.Input.Text"
                                                        LibClient.Components.Constructors.LC.Input.Text(
                                                            onChange = (fun value -> form.UpdateAcc (fun acc -> { acc with Name = value })),
                                                            onEnterKeyPress = (ReactEvent.Action.Make >> form.TrySubmitLowLevel),
                                                            value = (form.Acc.Name),
                                                            validity = (form.FieldValidity Field.Name),
                                                            label = ("Name")
                                                        )
                                                        let __parentFQN = Some "LibClient.Components.Input.PositiveInteger"
                                                        LibClient.Components.Constructors.LC.Input.PositiveInteger(
                                                            onChange = (fun value -> form.UpdateAcc (fun acc -> { acc with Age = value })),
                                                            onEnterKeyPress = (ReactEvent.Action.Make >> form.TrySubmitLowLevel),
                                                            value = (form.Acc.Age),
                                                            validity = (form.FieldValidity Field.Age),
                                                            label = ("Age")
                                                        )
                                                        let __parentFQN = Some "LibClient.Components.Legacy.Input.Picker"
                                                        LibClient.Components.Constructors.LC.Legacy.Input.Picker(
                                                            onChange = (LibClient.Components.Legacy.Input.Picker.CannotUnselect (fun (_, value) -> form.UpdateAcc (fun acc -> { acc with Gender = Some value }))),
                                                            value = (form.Acc.Gender |> Option.map LibClient.Components.Legacy.Input.Picker.ByItem),
                                                            items = (form.Acc.GenderChoices),
                                                            validity = (form.FieldValidity Field.Gender),
                                                            label = ("Gender")
                                                        )
                                                        let __parentFQN = Some "LibClient.Components.Input.Checkbox"
                                                        LibClient.Components.Constructors.LC.Input.Checkbox(
                                                            validity = (form.FieldValidity Field.WantsSpam),
                                                            onChange = (fun value -> form.UpdateAcc ( fun acc -> { acc with WantsSpam = value})),
                                                            value = (Some form.Acc.WantsSpam),
                                                            label = (LibClient.Components.Input.Checkbox.String "Subscribe to email")
                                                        )
                                                        let __parentFQN = Some "LibClient.Components.Buttons"
                                                        LibClient.Components.Constructors.LC.Buttons(
                                                            children =
                                                                [|
                                                                    let __parentFQN = Some "LibClient.Components.Button"
                                                                    LibClient.Components.Constructors.LC.Button(
                                                                        state = (LibClient.Components.Button.PropStateFactory.Make form.TrySubmit),
                                                                        label = ("Submit")
                                                                    )
                                                                |]
                                                        )
                                                    |])
                                            )
                                    )
                                |])
                    )
                |])
    )
