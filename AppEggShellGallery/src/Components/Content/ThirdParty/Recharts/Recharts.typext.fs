module AppEggShellGallery.Components.Content.ThirdParty.Recharts

open LibLang
open LibClient

let minChartWidth = 200
let minChartHeight = 300

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Data = {
    Name:   string
    Value1: float
    Value2: float
    Fill:   Color
}

type Estate = {
    DataSet1: Data array
}

type Recharts(_initialProps) =
    inherit EstatefulComponent<Props, Estate, Actions, Recharts>("AppEggShellGallery.Components.Content.ThirdParty.Recharts", _initialProps, Actions, hasStyles = true)

    override _.GetInitialEstate(_initialProps: Props) : Estate = {
        DataSet1 =
            [|
                {
                    Name = "Group A"
                    Value1 = 42.0
                    Value2 = 30.5
                    Fill = Color.Rgb (255, 0, 0)
                }
                {
                    Name = "Group B"
                    Value1 = 69.0
                    Value2 = 88.2
                    Fill = Color.Rgb (0, 255, 0)
                }
                {
                    Name = "Group C"
                    Value1 = 12.1
                    Value2 = 19.8
                    Fill = Color.Rgb (0, 0, 255)
                }
                {
                    Name = "Group D"
                    Value1 = 133.01
                    Value2 = 48.75
                    Fill = Color.Rgb (255, 0, 255)
                }
            |]
    }

and Actions(_this: Recharts) =
    class end

let Make = makeConstructor<Recharts, _, _>

// Unfortunately necessary boilerplate
type Pstate = NoPstate
