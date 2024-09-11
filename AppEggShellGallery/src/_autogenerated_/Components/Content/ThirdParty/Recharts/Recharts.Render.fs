module AppEggShellGallery.Components.Content.ThirdParty.RechartsRender

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

open AppEggShellGallery.Components.Content.ThirdParty.Recharts



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.ThirdParty.Recharts.Props, estate: AppEggShellGallery.Components.Content.ThirdParty.Recharts.Estate, pstate: AppEggShellGallery.Components.Content.ThirdParty.Recharts.Pstate, actions: AppEggShellGallery.Components.Content.ThirdParty.Recharts.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        displayName = ("Recharts"),
        props =
            (
                AppEggShellGallery.Components.ComponentContent.Manual
                    (
                            (castAsElementAckingKeysWarning [|
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("ThirdParty.Recharts.Components.CartesianGrid"),
                                    heading = ("CartesianGrid")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("ThirdParty.Recharts.Components.Cell"),
                                    heading = ("Cell")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("ThirdParty.Recharts.Components.Legend"),
                                    heading = ("Legend")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("ThirdParty.Recharts.Components.Line"),
                                    heading = ("Line")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("ThirdParty.Recharts.Components.LineChart"),
                                    heading = ("LineChart")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("ThirdParty.Recharts.Components.Pie"),
                                    heading = ("Pie")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("ThirdParty.Recharts.Components.PieChart"),
                                    heading = ("PieChart")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("ThirdParty.Recharts.Components.ResponsiveContainer"),
                                    heading = ("ResponsiveContainer")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("ThirdParty.Recharts.Components.Tooltip"),
                                    heading = ("Tooltip")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("ThirdParty.Recharts.Components.XAxis"),
                                    heading = ("XAxis")
                                )
                                let __parentFQN = Some "AppEggShellGallery.Components.ScrapedComponentProps"
                                AppEggShellGallery.Components.Constructors.Ui.ScrapedComponentProps(
                                    fullyQualifiedName = ("ThirdParty.Recharts.Components.YAxis"),
                                    heading = ("YAxis")
                                )
                            |])
                    )
            ),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Line Charts"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                <Recharts.ResponsiveContainer MinWidth='minChartWidth' MinHeight='minChartHeight'>
                    <Recharts.LineChart Data='estate.DataSet1 |> Array.map box'>
                        <Recharts.Line DataKey='""Value1""'/>
                    </Recharts.LineChart>
                </Recharts.ResponsiveContainer>
            "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    #if EGGSHELL_PLATFORM_IS_WEB
                                                    let __parentFQN = Some "ThirdParty.Recharts.Components.ResponsiveContainer"
                                                    ThirdParty.Recharts.Components.Constructors.Recharts.ResponsiveContainer(
                                                        minHeight = (minChartHeight),
                                                        minWidth = (minChartWidth),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "ThirdParty.Recharts.Components.LineChart"
                                                                ThirdParty.Recharts.Components.Constructors.Recharts.LineChart(
                                                                    data = (estate.DataSet1 |> Array.map box),
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Line"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Line(
                                                                                dataKey = ("Value1")
                                                                            )
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                    #endif
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                #if EGGSHELL_PLATFORM_IS_WEB
                                                                @"
                <Recharts.ResponsiveContainer MinWidth='minChartWidth' MinHeight='minChartHeight'>
                    <Recharts.LineChart Data='estate.DataSet1 |> Array.map box'>
                        <Recharts.CartesianGrid StrokeDashArray='[| 3.; 3. |]'/>
                        <Recharts.XAxis DataKey='""Name""'/>
                        <Recharts.YAxis/>
                        <Recharts.Tooltip/>
                        <Recharts.Legend/>
                        <Recharts.Line Type='~Monotone' DataKey='""Value1""' Stroke='~InternalString ""#8884d8""'/>
                        <Recharts.Line Type='~Monotone' DataKey='""Value2""' Stroke='~InternalString ""#82ca9d""'/>
                    </Recharts.LineChart>
                </Recharts.ResponsiveContainer>
            "
                                                                |> makeTextNode2 __parentFQN
                                                                #endif
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    #if EGGSHELL_PLATFORM_IS_WEB
                                                    let __parentFQN = Some "ThirdParty.Recharts.Components.ResponsiveContainer"
                                                    ThirdParty.Recharts.Components.Constructors.Recharts.ResponsiveContainer(
                                                        minHeight = (minChartHeight),
                                                        minWidth = (minChartWidth),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "ThirdParty.Recharts.Components.LineChart"
                                                                ThirdParty.Recharts.Components.Constructors.Recharts.LineChart(
                                                                    data = (estate.DataSet1 |> Array.map box),
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.CartesianGrid"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.CartesianGrid(
                                                                                strokeDashArray = ([| 3.; 3. |])
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.XAxis"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.XAxis(
                                                                                dataKey = ("Name")
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.YAxis"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.YAxis()
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Tooltip"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Tooltip()
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Legend"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Legend()
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Line"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Line(
                                                                                stroke = (ThirdParty.Recharts.Components.Line.InternalString "#8884d8"),
                                                                                dataKey = ("Value1"),
                                                                                ``type`` = (ThirdParty.Recharts.Components.Line.Type.Monotone)
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Line"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Line(
                                                                                stroke = (ThirdParty.Recharts.Components.Line.InternalString "#82ca9d"),
                                                                                dataKey = ("Value2"),
                                                                                ``type`` = (ThirdParty.Recharts.Components.Line.Type.Monotone)
                                                                            )
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                    #endif
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Area Charts"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                #if EGGSHELL_PLATFORM_IS_WEB
                                                                @"
                <Recharts.ResponsiveContainer MinWidth='minChartWidth' MinHeight='minChartHeight'>
                    <Recharts.AreaChart Data='estate.DataSet1 |> Array.map box'>
                        <Recharts.Area DataKey='""Value1""'/>
                    </Recharts.AreaChart>
                </Recharts.ResponsiveContainer>
            "
                                                                |> makeTextNode2 __parentFQN
                                                                #endif
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    #if EGGSHELL_PLATFORM_IS_WEB
                                                    let __parentFQN = Some "ThirdParty.Recharts.Components.ResponsiveContainer"
                                                    ThirdParty.Recharts.Components.Constructors.Recharts.ResponsiveContainer(
                                                        minHeight = (minChartHeight),
                                                        minWidth = (minChartWidth),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "ThirdParty.Recharts.Components.AreaChart"
                                                                ThirdParty.Recharts.Components.Constructors.Recharts.AreaChart(
                                                                    data = (estate.DataSet1 |> Array.map box),
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Area"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Area(
                                                                                dataKey = ("Value1")
                                                                            )
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                    #endif
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                #if EGGSHELL_PLATFORM_IS_WEB
                                                                @"
                <Recharts.ResponsiveContainer MinWidth='minChartWidth' MinHeight='minChartHeight'>
                    <Recharts.AreaChart Data='estate.DataSet1 |> Array.map box'>
                        <Recharts.CartesianGrid StrokeDashArray='[| 3.; 3. |]'/>
                        <Recharts.XAxis DataKey='""Name""'/>
                        <Recharts.YAxis/>
                        <Recharts.Tooltip/>
                        <Recharts.Legend/>
                        <Recharts.Area Type='~Type.Monotone' DataKey='""Value1""' Stroke='~InternalString ""#8884d8""' Fill='~InternalString ""#8884d8""'/>
                        <Recharts.Area Type='~Type.Monotone' DataKey='""Value2""' Stroke='~InternalString ""#82ca9d""' Fill='~InternalString ""#82ca9d""'/>
                    </Recharts.AreaChart>
                </Recharts.ResponsiveContainer>
            "
                                                                |> makeTextNode2 __parentFQN
                                                                #endif
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    #if EGGSHELL_PLATFORM_IS_WEB
                                                    let __parentFQN = Some "ThirdParty.Recharts.Components.ResponsiveContainer"
                                                    ThirdParty.Recharts.Components.Constructors.Recharts.ResponsiveContainer(
                                                        minHeight = (minChartHeight),
                                                        minWidth = (minChartWidth),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "ThirdParty.Recharts.Components.AreaChart"
                                                                ThirdParty.Recharts.Components.Constructors.Recharts.AreaChart(
                                                                    data = (estate.DataSet1 |> Array.map box),
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.CartesianGrid"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.CartesianGrid(
                                                                                strokeDashArray = ([| 3.; 3. |])
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.XAxis"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.XAxis(
                                                                                dataKey = ("Name")
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.YAxis"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.YAxis()
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Tooltip"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Tooltip()
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Legend"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Legend()
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Area"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Area(
                                                                                fill = (ThirdParty.Recharts.Components.Area.InternalString "#8884d8"),
                                                                                stroke = (ThirdParty.Recharts.Components.Area.InternalString "#8884d8"),
                                                                                dataKey = ("Value1"),
                                                                                ``type`` = (ThirdParty.Recharts.Components.Area.Type.Monotone)
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Area"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Area(
                                                                                fill = (ThirdParty.Recharts.Components.Area.InternalString "#82ca9d"),
                                                                                stroke = (ThirdParty.Recharts.Components.Area.InternalString "#82ca9d"),
                                                                                dataKey = ("Value2"),
                                                                                ``type`` = (ThirdParty.Recharts.Components.Area.Type.Monotone)
                                                                            )
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                    #endif
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                #if EGGSHELL_PLATFORM_IS_WEB
                                                                @"
                <Recharts.ResponsiveContainer MinWidth='minChartWidth' MinHeight='minChartHeight'>
                    <Recharts.AreaChart Data='estate.DataSet1 |> Array.map box'>
                        <Recharts.CartesianGrid StrokeDashArray='[| 3.; 3. |]'/>
                        <Recharts.XAxis DataKey='""Name""'/>
                        <Recharts.YAxis/>
                        <Recharts.Tooltip/>
                        <Recharts.Legend/>
                        <Recharts.Area Type='~Type.Monotone' DataKey='""Value1""' Stroke='~InternalString ""#8884d8""' Fill='~InternalString ""#8884d8""'/>
                        <Recharts.Area Type='~Type.Monotone' DataKey='""Value2""' Stroke='~InternalString ""#82ca9d""' Fill='~InternalString ""#82ca9d""'/>
                    </Recharts.AreaChart>
                </Recharts.ResponsiveContainer>
            "
                                                                |> makeTextNode2 __parentFQN
                                                                #endif
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    #if EGGSHELL_PLATFORM_IS_WEB
                                                    let __parentFQN = Some "ThirdParty.Recharts.Components.ResponsiveContainer"
                                                    ThirdParty.Recharts.Components.Constructors.Recharts.ResponsiveContainer(
                                                        minHeight = (minChartHeight),
                                                        minWidth = (minChartWidth),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "ThirdParty.Recharts.Components.AreaChart"
                                                                ThirdParty.Recharts.Components.Constructors.Recharts.AreaChart(
                                                                    data = (estate.DataSet1 |> Array.map box),
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.CartesianGrid"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.CartesianGrid(
                                                                                strokeDashArray = ([| 3.; 3. |])
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.XAxis"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.XAxis(
                                                                                dataKey = ("Name")
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.YAxis"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.YAxis()
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Tooltip"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Tooltip()
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Legend"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Legend()
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Area"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Area(
                                                                                fill = (ThirdParty.Recharts.Components.Area.InternalString "#8884d8"),
                                                                                stroke = (ThirdParty.Recharts.Components.Area.InternalString "#8884d8"),
                                                                                dataKey = ("Value1"),
                                                                                stackId = (ThirdParty.Recharts.Components.Area.Number 1),
                                                                                ``type`` = (ThirdParty.Recharts.Components.Area.Type.StepAfter)
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Area"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Area(
                                                                                fill = (ThirdParty.Recharts.Components.Area.InternalString "#82ca9d"),
                                                                                stroke = (ThirdParty.Recharts.Components.Area.InternalString "#82ca9d"),
                                                                                dataKey = ("Value2"),
                                                                                stackId = (ThirdParty.Recharts.Components.Area.Number 1),
                                                                                ``type`` = (ThirdParty.Recharts.Components.Area.Type.StepAfter)
                                                                            )
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                    #endif
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Pie Charts"),
                        samples =
                                (castAsElementAckingKeysWarning [|
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                #if EGGSHELL_PLATFORM_IS_WEB
                                                                @"
                        <Recharts.ResponsiveContainer MinWidth='minChartWidth' MinHeight='minChartHeight'>
                            <Recharts.PieChart>
                                <Recharts.Pie Data='estate.DataSet1 |> Array.map box' NameKey='""Name""' DataKey='""Value1""'/>
                            </Recharts.PieChart>
                        </Recharts.ResponsiveContainer>
                        "
                                                                |> makeTextNode2 __parentFQN
                                                                #endif
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    #if EGGSHELL_PLATFORM_IS_WEB
                                                    let __parentFQN = Some "ThirdParty.Recharts.Components.ResponsiveContainer"
                                                    ThirdParty.Recharts.Components.Constructors.Recharts.ResponsiveContainer(
                                                        minHeight = (minChartHeight),
                                                        minWidth = (minChartWidth),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "ThirdParty.Recharts.Components.PieChart"
                                                                ThirdParty.Recharts.Components.Constructors.Recharts.PieChart(
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Pie"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Pie(
                                                                                dataKey = ("Value1"),
                                                                                nameKey = ("Name"),
                                                                                data = (estate.DataSet1 |> Array.map box)
                                                                            )
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                    #endif
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                #if EGGSHELL_PLATFORM_IS_WEB
                                                                @"
                    <Recharts.ResponsiveContainer MinWidth='minChartWidth' MinHeight='minChartHeight'>
                        <Recharts.PieChart>
                            <Recharts.Pie Data='estate.DataSet1 |> Array.map box' NameKey='""Name""' DataKey='""Value1""'>
                                <Recharts.Cell rt-map='data := estate.DataSet1' Fill='data.Fill'/>
                            </Recharts.Pie>
                        </Recharts.PieChart>
                    </Recharts.ResponsiveContainer>
                "
                                                                |> makeTextNode2 __parentFQN
                                                                #endif
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    #if EGGSHELL_PLATFORM_IS_WEB
                                                    let __parentFQN = Some "ThirdParty.Recharts.Components.ResponsiveContainer"
                                                    ThirdParty.Recharts.Components.Constructors.Recharts.ResponsiveContainer(
                                                        minHeight = (minChartHeight),
                                                        minWidth = (minChartWidth),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "ThirdParty.Recharts.Components.PieChart"
                                                                ThirdParty.Recharts.Components.Constructors.Recharts.PieChart(
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Pie"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Pie(
                                                                                dataKey = ("Value1"),
                                                                                nameKey = ("Name"),
                                                                                data = (estate.DataSet1 |> Array.map box),
                                                                                children =
                                                                                    [|
                                                                                        (
                                                                                            (estate.DataSet1)
                                                                                            |> Seq.map
                                                                                                (fun data ->
                                                                                                    let __parentFQN = Some "ThirdParty.Recharts.Components.Cell"
                                                                                                    ThirdParty.Recharts.Components.Constructors.Recharts.Cell(
                                                                                                        fill = (data.Fill)
                                                                                                    )
                                                                                                )
                                                                                            |> Array.ofSeq |> castAsElement
                                                                                        )
                                                                                    |]
                                                                            )
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                    #endif
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                #if EGGSHELL_PLATFORM_IS_WEB
                                                                @"
                    <Recharts.ResponsiveContainer MinWidth='minChartWidth' MinHeight='minChartHeight'>
                        <Recharts.PieChart>
                            <Recharts.Pie Data='estate.DataSet1 |> Array.map box' NameKey='""Name""' DataKey='""Value1""' Cx='~Offset.Percentage 50.' Cy='~Offset.Percentage 50.' InnerRadius='~Radius.Number 30' OuterRadius='~Radius.Number 50'>
                                <Recharts.Cell rt-map='data := estate.DataSet1' Fill='data.Fill' Stroke='Color.White'/>
                            </Recharts.Pie>
                            <Recharts.Pie Data='estate.DataSet1 |> Array.map box' NameKey='""Name""' DataKey='""Value2""' Cx='~Offset.Percentage 50.' Cy='~Offset.Percentage 50.' InnerRadius='~Radius.Number 60' OuterRadius='~Radius.Number 80'>
                                <Recharts.Cell rt-map='data := estate.DataSet1' Fill='data.Fill' Stroke='Color.White'/>
                            </Recharts.Pie>
                            <Recharts.Tooltip IsAnimationActive='true' AnimationEasing='~EaseInOut'/>
                            <Recharts.Legend/>
                        </Recharts.PieChart>
                    </Recharts.ResponsiveContainer>
                "
                                                                |> makeTextNode2 __parentFQN
                                                                #endif
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    #if EGGSHELL_PLATFORM_IS_WEB
                                                    let __parentFQN = Some "ThirdParty.Recharts.Components.ResponsiveContainer"
                                                    ThirdParty.Recharts.Components.Constructors.Recharts.ResponsiveContainer(
                                                        minHeight = (minChartHeight),
                                                        minWidth = (minChartWidth),
                                                        children =
                                                            [|
                                                                let __parentFQN = Some "ThirdParty.Recharts.Components.PieChart"
                                                                ThirdParty.Recharts.Components.Constructors.Recharts.PieChart(
                                                                    children =
                                                                        [|
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Pie"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Pie(
                                                                                outerRadius = (ThirdParty.Recharts.Components.Pie.Radius.Number 50),
                                                                                innerRadius = (ThirdParty.Recharts.Components.Pie.Radius.Number 30),
                                                                                cy = (ThirdParty.Recharts.Components.Pie.Offset.Percentage 50.),
                                                                                cx = (ThirdParty.Recharts.Components.Pie.Offset.Percentage 50.),
                                                                                dataKey = ("Value1"),
                                                                                nameKey = ("Name"),
                                                                                data = (estate.DataSet1 |> Array.map box),
                                                                                children =
                                                                                    [|
                                                                                        (
                                                                                            (estate.DataSet1)
                                                                                            |> Seq.map
                                                                                                (fun data ->
                                                                                                    let __parentFQN = Some "ThirdParty.Recharts.Components.Cell"
                                                                                                    ThirdParty.Recharts.Components.Constructors.Recharts.Cell(
                                                                                                        stroke = (Color.White),
                                                                                                        fill = (data.Fill)
                                                                                                    )
                                                                                                )
                                                                                            |> Array.ofSeq |> castAsElement
                                                                                        )
                                                                                    |]
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Pie"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Pie(
                                                                                outerRadius = (ThirdParty.Recharts.Components.Pie.Radius.Number 80),
                                                                                innerRadius = (ThirdParty.Recharts.Components.Pie.Radius.Number 60),
                                                                                cy = (ThirdParty.Recharts.Components.Pie.Offset.Percentage 50.),
                                                                                cx = (ThirdParty.Recharts.Components.Pie.Offset.Percentage 50.),
                                                                                dataKey = ("Value2"),
                                                                                nameKey = ("Name"),
                                                                                data = (estate.DataSet1 |> Array.map box),
                                                                                children =
                                                                                    [|
                                                                                        (
                                                                                            (estate.DataSet1)
                                                                                            |> Seq.map
                                                                                                (fun data ->
                                                                                                    let __parentFQN = Some "ThirdParty.Recharts.Components.Cell"
                                                                                                    ThirdParty.Recharts.Components.Constructors.Recharts.Cell(
                                                                                                        stroke = (Color.White),
                                                                                                        fill = (data.Fill)
                                                                                                    )
                                                                                                )
                                                                                            |> Array.ofSeq |> castAsElement
                                                                                        )
                                                                                    |]
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Tooltip"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Tooltip(
                                                                                animationEasing = (ThirdParty.Recharts.Components.Tooltip.EaseInOut),
                                                                                isAnimationActive = (true)
                                                                            )
                                                                            let __parentFQN = Some "ThirdParty.Recharts.Components.Legend"
                                                                            ThirdParty.Recharts.Components.Constructors.Recharts.Legend()
                                                                        |]
                                                                )
                                                            |]
                                                    )
                                                    #endif
                                                |])
                                    )
                                |])
                    )
                |])
    )
