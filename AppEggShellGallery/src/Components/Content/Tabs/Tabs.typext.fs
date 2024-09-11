module AppEggShellGallery.Components.Content.Tabs

open LibClient

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type TabItem =
| Home
| Profile
| Contact

type Estate = {
    SelectedTab: TabItem
}

type Tabs(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Tabs>("AppEggShellGallery.Components.Content.Tabs", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        SelectedTab = Home
    }

and Actions(this: Tabs) =
    member _.SelectTab (tab: TabItem) (_e: ReactEvent.Action) : unit =
        this.SetEstate (fun estate _ -> { estate with SelectedTab = tab })

let Make = makeConstructor<Tabs, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
