module AppEggShellGallery.Components.ComponentSample

open LibClient
open ReactXP.Styles.RulesBasic
open ThirdParty.SyntaxHighlighter.Components

// aliases for ~ access
let Render = SyntaxHighlighter.Language.Render
let Fsharp = SyntaxHighlighter.Language.Fsharp

type Code =
| SingleBlock of SyntaxHighlighter.Language * ReactElement
| Children of ReactElement

let singleBlock (language: SyntaxHighlighter.Language) (children: ReactElement) : Code =
    SingleBlock (language, children)

type VerticalAlignment =
| Top
| Middle
with
    member this.Class : string =
        unionCaseName this

type Layout =
| SideBySide
| CodeBelowSamples

type Props = (* GenerateMakeFunction *) {
    Visuals:           ReactElement
    Code:              Code
    Heading:           string option     // defaultWithAutoWrap None
    Notes:             ReactElement      // default noElement
    VerticalAlignment: VerticalAlignment // default VerticalAlignment.Middle
    Layout:            Layout            // default Layout.SideBySide
}

type ComponentSample(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, ComponentSample>("AppEggShellGallery.Components.ComponentSample", _initialProps, Actions, hasStyles = true)

and Actions(_this: ComponentSample) =
    class end

let Make = makeConstructor<ComponentSample, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate