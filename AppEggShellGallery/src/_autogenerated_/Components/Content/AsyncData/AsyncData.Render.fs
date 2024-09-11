module AppEggShellGallery.Components.Content.AsyncDataRender

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

open AppEggShellGallery.Components.Content.AsyncData



let render(children: array<ReactElement>, props: AppEggShellGallery.Components.Content.AsyncData.Props, estate: AppEggShellGallery.Components.Content.AsyncData.Estate, pstate: AppEggShellGallery.Components.Content.AsyncData.Pstate, actions: AppEggShellGallery.Components.Content.AsyncData.Actions, __componentStyles: ReactXP.LegacyStyles.RuntimeStyles) : Fable.React.ReactElement =
    // sadly #nowarn has file scope, so we have to emulate it manually
    (children, props, estate, pstate, actions) |> ignore
    let __class = (ReactXP.Helpers.extractProp "ClassName" props) |> Option.defaultValue ""
    let __mergedStyles = ReactXP.LegacyStyles.Runtime.mergeComponentAndPropsStyles __componentStyles props
    let __parentFQN = None
    let __parentFQN = Some "AppEggShellGallery.Components.ComponentContent"
    AppEggShellGallery.Components.Constructors.Ui.ComponentContent(
        props = (AppEggShellGallery.Components.ComponentContent.ForFullyQualifiedName "LibClient.Components.AsyncData"),
        displayName = ("AsyncData"),
        notes =
                (castAsElementAckingKeysWarning [|
                    makeTextNode2 __parentFQN "An AsyncData component is typically used in conjunction with a component that provides data asynchronously, such\n        as With.Subject or QueryGrid. Such components will typically handle the async life cycle on your behalf, automatically\n        transitioning between different AsyncData<'T> values. For the sake of simplicity, the examples below provide\n        AsyncData<'T> values to the AsyncData component directly."
                |]),
        samples =
                (castAsElementAckingKeysWarning [|
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Basics"),
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
                    <LC.AsyncData rt-fs='true'
                     Data='Uninitialized'
                     rt-prop-children='WhenAvailable(name)'>
                        The name is {name}.
                    </LC.AsyncData>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                        data = (Uninitialized),
                                                        whenAvailable =
                                                            (fun (name) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <LC.AsyncData rt-fs='true'
                     Data='Unavailable'
                     rt-prop-children='WhenAvailable(name)'>
                        The name is {name}.
                    </LC.AsyncData>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                        data = (Unavailable),
                                                        whenAvailable =
                                                            (fun (name) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <LC.AsyncData rt-fs='true'
                     Data='AccessDenied'
                     rt-prop-children='WhenAvailable(name)'>
                        The name is {name}.
                    </LC.AsyncData>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                        data = (AccessDenied),
                                                        whenAvailable =
                                                            (fun (name) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <LC.AsyncData rt-fs='true'
                     Data='WillStartFetchingSoonHack'
                     rt-prop-children='WhenAvailable(name)'>
                        The name is {name}.
                    </LC.AsyncData>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                        data = (WillStartFetchingSoonHack),
                                                        whenAvailable =
                                                            (fun (name) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <LC.AsyncData rt-fs='true'
                     Data='Fetching None'
                     rt-prop-children='WhenAvailable(name)'>
                        The name is {name}.
                    </LC.AsyncData>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                        data = (Fetching None),
                                                        whenAvailable =
                                                            (fun (name) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <LC.AsyncData rt-fs='true'
                     Data='Fetching (Some ""Jekyll"")'
                     rt-prop-children='WhenAvailable(name)'>
                        The name is {name}.
                    </LC.AsyncData>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                        data = (Fetching (Some "Jekyll")),
                                                        whenAvailable =
                                                            (fun (name) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <LC.AsyncData rt-fs='true'
                     Data='Available ""Hyde""'
                     rt-prop-children='WhenAvailable(name)'>
                        The name is {name}.
                    </LC.AsyncData>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                        data = (Available "Hyde"),
                                                        whenAvailable =
                                                            (fun (name) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Customization"),
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
                    <LC.AsyncData rt-fs='true'
                     Data='Uninitialized'
                     rt-prop-children='WhenAvailable(name)'>
                        The name is {name}.
                        <rt-prop name='WhenUninitialized(_)'>
                            A custom uninitialized message.
                        </rt-prop>
                    </LC.AsyncData>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                        data = (Uninitialized),
                                                        whenAvailable =
                                                            (fun (name) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                    |])
                                                            ),
                                                        whenUninitialized =
                                                            (fun (_) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN "A custom uninitialized message."
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <LC.AsyncData rt-fs='true'
                     Data='Unavailable'
                     rt-prop-children='WhenAvailable(name)'>
                        The name is {name}.
                        <rt-prop name='WhenElse(_)'>
                            An alternative way to customize for all states other than Available.
                        </rt-prop>
                    </LC.AsyncData>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                        data = (Unavailable),
                                                        whenAvailable =
                                                            (fun (name) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                    |])
                                                            ),
                                                        whenElse =
                                                            (fun (_) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN "An alternative way to customize for all states other than Available."
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <LC.AsyncData rt-fs='true'
                     Data='Fetching (Some ""Jekyll"")'
                     rt-prop-children='WhenAvailable(name)'>
                        The name is {name}.

                        <rt-prop name='WhenFetching(maybePrevName)'>
                            Updating name (previously {maybePrevName |> Option.getOrElse ""unknown""}), please wait.
                        </rt-prop>
                    </LC.AsyncData>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                        data = (Fetching (Some "Jekyll")),
                                                        whenAvailable =
                                                            (fun (name) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                    |])
                                                            ),
                                                        whenFetching =
                                                            (fun (maybePrevName) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "Updating name (previously ", (maybePrevName |> Option.getOrElse "unknown"), "), please wait."))
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                |])
                    )
                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSampleGroup"
                    AppEggShellGallery.Components.Constructors.Ui.ComponentSampleGroup(
                        heading = ("Failures"),
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
                    <LC.ErrorBoundary rt-fs='true' rt-prop-children='Try'>
                        <LC.AsyncData rt-fs='true'
                         Data='Failed (UserReadableFailure ""someone sent us a bomb"")'
                         rt-prop-children='WhenAvailable(name)'>
                            The name is {name}.
                        </LC.AsyncData>

                        <rt-prop name='Catch(_, _)'>
                            Caught an error (catching here lest the error propagates all the way to the top of the app)
                        </rt-prop>
                    </LC.ErrorBoundary>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.ErrorBoundary"
                                                    LibClient.Components.Constructors.LC.ErrorBoundary(
                                                        catch =
                                                            (fun (_, _) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN "Caught an error (catching here lest the error propagates all the way to the top of the app)"
                                                                    |])
                                                            ),
                                                        ``try`` =
                                                                (castAsElementAckingKeysWarning [|
                                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                                        data = (Failed (UserReadableFailure "someone sent us a bomb")),
                                                                        whenAvailable =
                                                                            (fun (name) ->
                                                                                    (castAsElementAckingKeysWarning [|
                                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                                    |])
                                                                            )
                                                                    )
                                                                |])
                                                    )
                                                |])
                                    )
                                    let __parentFQN = Some "AppEggShellGallery.Components.ComponentSample"
                                    AppEggShellGallery.Components.Constructors.Ui.ComponentSample(
                                        code =
                                            (
                                                AppEggShellGallery.Components.ComponentSample.singleBlock AppEggShellGallery.Components.ComponentSample.Render
                                                    (
                                                            (castAsElementAckingKeysWarning [|
                                                                @"
                    <LC.AsyncData rt-fs='true'
                     Data='Failed (UserReadableFailure ""someone sent us a bomb"")'
                     rt-prop-children='WhenAvailable(name)'>
                        The name is {name}.

                        <rt-prop name='WhenFailed(_error)'>
                            Something went wrong - we couldn't retrieve the name.
                        </rt-prop>
                    </LC.AsyncData>
                "
                                                                |> makeTextNode2 __parentFQN
                                                            |])
                                                    )
                                            ),
                                        visuals =
                                                (castAsElementAckingKeysWarning [|
                                                    let __parentFQN = Some "LibClient.Components.AsyncData"
                                                    LibClient.Components.Constructors.LC.AsyncData(
                                                        data = (Failed (UserReadableFailure "someone sent us a bomb")),
                                                        whenAvailable =
                                                            (fun (name) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN (System.String.Format("{0}{1}{2}", "The name is ", (name), "."))
                                                                    |])
                                                            ),
                                                        whenFailed =
                                                            (fun (_error) ->
                                                                    (castAsElementAckingKeysWarning [|
                                                                        makeTextNode2 __parentFQN "Something went wrong - we couldn't retrieve the name."
                                                                    |])
                                                            )
                                                    )
                                                |])
                                    )
                                |])
                    )
                |])
    )
