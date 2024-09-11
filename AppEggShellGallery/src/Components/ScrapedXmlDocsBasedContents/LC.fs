
// ************************************************************ //
//                                                              //
//  THIS FILE IS AUTO-GENERATED FROM XML DOCUMENTATION COMMENTS //
//                                                              //
// ************************************************************ //

[<AutoOpen>]
module AppEggShellGallery.Components.ScrapedXmlDocsBasedContents_LC

open Fable.React
open LibClient
open LibClient.Components
open ReactXP.Components
open ReactXP.Styles


module private Module_LibClient_Disposables_SerialDisposable =

    [<Component>]
    let content () : ReactElement =
        Ui.ComponentContent (
            displayName = "T:LibClient.Disposables.SerialDisposable",
            
            notes       = LC.Text """A disposable with an inner, replaceable disposable, with automatic disposal of any predecessor.""",
            samples     = element {
                LC.Text "No examples"
            }
        )


module private Module_LibClient_Disposables_CompositeDisposable =

    [<Component>]
    let content () : ReactElement =
        Ui.ComponentContent (
            displayName = "T:LibClient.Disposables.CompositeDisposable",
            
            notes       = LC.Text """Many disposables under the guise of one.""",
            samples     = element {
                LC.Text "No examples"
            }
        )


module private Module_Fragment =

    [<Component>]
    let content () : ReactElement =
        Ui.ComponentContent (
            displayName = "Fragment",
            
            props = AppEggShellGallery.Components.ComponentContent.PropsConfig.Manual (
                Ui.ComponentProps (data = {
                    Fields = (Choice2Of2 [

                        {
                            Name = "children"
                            Type = "array<ReactElement>"
                            Default = None
                            Description = None
                        }
                        {
                            Name = "key"
                            Type = "string"
                            Default = Some "None"
                            Description = None
                        }
                    ])
                    MaybeScrapeErrors = None
                })
            ),
            notes       = LC.Text """Wrap children in a fragment, optionally specifying a key""",
            samples     = element {
                LC.Text "No examples"
            }
        )


module private Module_Column =

    [<Component>]
    let content () : ReactElement =
        Ui.ComponentContent (
            displayName = "Column",
            
            props = AppEggShellGallery.Components.ComponentContent.PropsConfig.Manual (
                Ui.ComponentProps (data = {
                    Fields = (Choice2Of2 [

                        {
                            Name = "children"
                            Type = "array<ReactElement>"
                            Default = None
                            Description = None
                        }
                        {
                            Name = "crossAxisAlignment"
                            Type = "CrossAxisAlignment"
                            Default = Some "CrossAxisAlignment.Stretch"
                            Description = Some "Alignment of children along the horizontal axis"
                        }
                        {
                            Name = "gap"
                            Type = "int"
                            Default = Some "0"
                            Description = Some "Vertical gap between children"
                        }
                        {
                            Name = "styles"
                            Type = "array<ViewStyles>"
                            Default = Some "[||]"
                            Description = None
                        }
                    ])
                    MaybeScrapeErrors = None
                })
            ),
            notes       = LC.Text """Lay out the children in a column, optionally configuring the gap beteween children, and the horizontal alignment""",
            samples     = element {
                
                Ui.ComponentSample (
                    heading = """Basics""",
                    visuals = (element {

     LC.Column [|
         LC.Text "Banana"
         LC.Text "Apple"
         LC.Text "Mango"
     |]
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Column [|
         LC.Text "Banana"
         LC.Text "Apple"
         LC.Text "Mango"
     |]
 """
                            |])

                        })
                )

                Ui.ComponentSample (
                    heading = """Gap""",
                    visuals = (element {

     LC.Column (
         gap = 30,
         children = [|
             LC.Text "Banana"
             LC.Text "Apple"
             LC.Text "Mango"
         |]
     )
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Column (
         gap = 30,
         children = [|
             LC.Text "Banana"
             LC.Text "Apple"
             LC.Text "Mango"
         |]
     )
 """
                            |])

                        })
                )

                Ui.ComponentSample (
                    heading = """Cross Axis Alignment""",
                    visuals = (element {

     LC.Column (
         crossAxisAlignment = LC.CrossAxisAlignment.Center,
         children = [|
             LC.Text "Banana"
             LC.Text "Green\nApple"
             LC.Text "Mango"
         |]
     )
     LC.Column (
         crossAxisAlignment = LC.CrossAxisAlignment.FlexEnd,
         children = [|
             LC.Text "Banana"
             LC.Text "Green\nApple"
             LC.Text "Mango"
         |]
     )
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Column (
         crossAxisAlignment = LC.CrossAxisAlignment.Center,
         children = [|
             LC.Text "Banana"
             LC.Text "Green\nApple"
             LC.Text "Mango"
         |]
     )
     LC.Column (
         crossAxisAlignment = LC.CrossAxisAlignment.FlexEnd,
         children = [|
             LC.Text "Banana"
             LC.Text "Green\nApple"
             LC.Text "Mango"
         |]
     )
 """
                            |])

                        })
                )

            }
        )


module private Module_Row =

    [<Component>]
    let content () : ReactElement =
        Ui.ComponentContent (
            displayName = "Row",
            
            props = AppEggShellGallery.Components.ComponentContent.PropsConfig.Manual (
                Ui.ComponentProps (data = {
                    Fields = (Choice2Of2 [

                        {
                            Name = "children"
                            Type = "array<ReactElement>"
                            Default = None
                            Description = None
                        }
                        {
                            Name = "crossAxisAlignment"
                            Type = "CrossAxisAlignment"
                            Default = Some "CrossAxisAlignment.Stretch"
                            Description = Some "Alignment of children along the vertical axis"
                        }
                        {
                            Name = "gap"
                            Type = "int"
                            Default = Some "0"
                            Description = Some "Vertical gap between children"
                        }
                        {
                            Name = "styles"
                            Type = "array<ViewStyles>"
                            Default = Some "[||]"
                            Description = None
                        }
                    ])
                    MaybeScrapeErrors = None
                })
            ),
            notes       = LC.Text """Lay out the children in a row, optionally configuring the gap between children, and the vertical alignment.""",
            samples     = element {
                
                Ui.ComponentSample (
                    heading = """Basics""",
                    visuals = (element {

     LC.Row [|
         LC.Text "Banana"
         LC.Text "Apple"
         LC.Text "Mango"
     |]
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Row [|
         LC.Text "Banana"
         LC.Text "Apple"
         LC.Text "Mango"
     |]
 """
                            |])

                        })
                )

                Ui.ComponentSample (
                    heading = """Gap""",
                    visuals = (element {

     LC.Row (
         gap = 10,
         children = [|
             LC.Text "Banana"
             LC.Text "Apple"
             LC.Text "Mango"
         |]
     )
     LC.Row (
         gap = 30,
         children = [|
             LC.Text "Banana"
             LC.Text "Apple"
             LC.Text "Mango"
         |]
     )
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Row (
         gap = 10,
         children = [|
             LC.Text "Banana"
             LC.Text "Apple"
             LC.Text "Mango"
         |]
     )
     LC.Row (
         gap = 30,
         children = [|
             LC.Text "Banana"
             LC.Text "Apple"
             LC.Text "Mango"
         |]
     )
 """
                            |])

                        })
                )

                Ui.ComponentSample (
                    heading = """Cross axis alignment""",
                    visuals = (element {

     LC.Row (
         crossAxisAlignment = LC.CrossAxisAlignment.Center,
         children = [|
             LC.Text "Banana"
             LC.Text "Green\nApple"
             LC.Text "Mango"
         |]
     )
     LC.Row (
         crossAxisAlignment = LC.CrossAxisAlignment.FlexStart,
         children = [|
             LC.Text "Banana"
             LC.Text "Green\nApple"
             LC.Text "Mango"
         |]
     )
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Row (
         crossAxisAlignment = LC.CrossAxisAlignment.Center,
         children = [|
             LC.Text "Banana"
             LC.Text "Green\nApple"
             LC.Text "Mango"
         |]
     )
     LC.Row (
         crossAxisAlignment = LC.CrossAxisAlignment.FlexStart,
         children = [|
             LC.Text "Banana"
             LC.Text "Green\nApple"
             LC.Text "Mango"
         |]
     )
 """
                            |])

                        })
                )

            }
        )


module private Module_Sized =

    module private Styles =
        let greyExpandingBox = makeViewStyles {
            backgroundColor Color.DevLightGrey
            flex 1
        }

    [<Component>]
    let content () : ReactElement =
        Ui.ComponentContent (
            displayName = "Sized",
            
            props = AppEggShellGallery.Components.ComponentContent.PropsConfig.Manual (
                Ui.ComponentProps (data = {
                    Fields = (Choice2Of2 [

                        {
                            Name = "child"
                            Type = "ReactElement"
                            Default = None
                            Description = None
                        }
                        {
                            Name = "width"
                            Type = "int"
                            Default = Some "None"
                            Description = None
                        }
                        {
                            Name = "height"
                            Type = "int"
                            Default = Some "None"
                            Description = None
                        }
                        {
                            Name = "styles"
                            Type = "array<ViewStyles>"
                            Default = Some "[||]"
                            Description = None
                        }
                    ])
                    MaybeScrapeErrors = None
                })
            ),
            notes       = LC.Text """Wrap the child in a fixed size box""",
            samples     = element {
                
                Ui.ComponentSample (
                    heading = """Basics""",
                    visuals = (element {

     LC.Sized (
         width = 100,
         height = 100,
         child = RX.View (
             styles = [|Styles.greyExpandingBox|],
             children = [|LC.Text "the box"|]
         )
     )
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Sized (
         width = 100,
         height = 100,
         child = RX.View (
             styles = [|Styles.greyExpandingBox|],
             children = [|LC.Text "the box"|]
         )
     )
 """
                            |])

                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, heading = "Setup Code", children = [| LC.Text """

         module private Styles =
             let greyExpandingBox = makeViewStyles {
                 backgroundColor Color.DevLightGrey
                 flex 1
             }
     """
                            |])
                        })
                )

                Ui.ComponentSample (
                    heading = """Only width""",
                    visuals = (element {

     LC.Sized (
         width = 100,
         child = RX.View (
             styles = [|Styles.greyExpandingBox|],
             children = [|LC.Text "the box"|]
         )
     )
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Sized (
         width = 100,
         child = RX.View (
             styles = [|Styles.greyExpandingBox|],
             children = [|LC.Text "the box"|]
         )
     )
 """
                            |])

                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, heading = "Setup Code", children = [| LC.Text """

         module private Styles =
             let greyExpandingBox = makeViewStyles {
                 backgroundColor Color.DevLightGrey
                 flex 1
             }
     """
                            |])
                        })
                )

                Ui.ComponentSample (
                    heading = """Only height""",
                    visuals = (element {

     LC.Sized (
         height = 100,
         child = RX.View (
             styles = [|Styles.greyExpandingBox|],
             children = [|LC.Text "the box"|]
         )
     )
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Sized (
         height = 100,
         child = RX.View (
             styles = [|Styles.greyExpandingBox|],
             children = [|LC.Text "the box"|]
         )
     )
 """
                            |])

                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, heading = "Setup Code", children = [| LC.Text """

         module private Styles =
             let greyExpandingBox = makeViewStyles {
                 backgroundColor Color.DevLightGrey
                 flex 1
             }
     """
                            |])
                        })
                )

            }
        )


module private Module_Constrained =

    module private Styles =
        let greyExpandingBox = makeViewStyles {
            backgroundColor Color.DevLightGrey
            flex 1
        }

    let private lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In hendrerit vehicula sollicitudin. Sed lacinia, libero ultrices mattis dignissim, libero purus interdum ante, eu pharetra tortor massa vel lorem. Donec dignissim felis quis nisl sodales, id lacinia felis congue. Sed porta ipsum sem, et interdum arcu fringilla ac. Maecenas tincidunt, leo non ultricies molestie, lectus odio sagittis tellus, a convallis neque sapien vel sem. Nullam a justo blandit, condimentum leo sit amet, egestas lectus. Nunc eu eros eget lorem condimentum facilisis eget ac leo. Nulla bibendum ex eu dui blandit, sit amet feugiat quam interdum. Maecenas iaculis pharetra ex a gravida. Integer faucibus venenatis commodo."

    [<Component>]
    let content () : ReactElement =
        Ui.ComponentContent (
            displayName = "Constrained",
            
            props = AppEggShellGallery.Components.ComponentContent.PropsConfig.Manual (
                Ui.ComponentProps (data = {
                    Fields = (Choice2Of2 [

                        {
                            Name = "child"
                            Type = "ReactElement"
                            Default = None
                            Description = None
                        }
                        {
                            Name = "maxWidth"
                            Type = "int"
                            Default = Some "None"
                            Description = None
                        }
                        {
                            Name = "minWidth"
                            Type = "int"
                            Default = Some "None"
                            Description = None
                        }
                        {
                            Name = "maxHeight"
                            Type = "int"
                            Default = Some "None"
                            Description = None
                        }
                        {
                            Name = "minHeight"
                            Type = "int"
                            Default = Some "None"
                            Description = None
                        }
                    ])
                    MaybeScrapeErrors = None
                })
            ),
            notes       = LC.Text """Wrap the child in a constrained box""",
            samples     = element {
                
                Ui.ComponentSample (
                    heading = """maxWidth""",
                    visuals = (element {

     LC.Constrained (
         maxWidth = 200,
         child = RX.View (
             styles = [|Styles.greyExpandingBox|],
             children = [|LC.Text lipsum|]
         )
     )
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Constrained (
         maxWidth = 200,
         child = RX.View (
             styles = [|Styles.greyExpandingBox|],
             children = [|LC.Text lipsum|]
         )
     )
 """
                            |])

                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, heading = "Setup Code", children = [| LC.Text """

         module private Styles =
             let greyExpandingBox = makeViewStyles {
                 backgroundColor Color.DevLightGrey
                 flex 1
             }

         let private lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In hendrerit vehicula sollicitudin. Sed lacinia, libero ultrices mattis dignissim, libero purus interdum ante, eu pharetra tortor massa vel lorem. Donec dignissim felis quis nisl sodales, id lacinia felis congue. Sed porta ipsum sem, et interdum arcu fringilla ac. Maecenas tincidunt, leo non ultricies molestie, lectus odio sagittis tellus, a convallis neque sapien vel sem. Nullam a justo blandit, condimentum leo sit amet, egestas lectus. Nunc eu eros eget lorem condimentum facilisis eget ac leo. Nulla bibendum ex eu dui blandit, sit amet feugiat quam interdum. Maecenas iaculis pharetra ex a gravida. Integer faucibus venenatis commodo."
     """
                            |])
                        })
                )

                Ui.ComponentSample (
                    heading = """maxHeight""",
                    visuals = (element {

     LC.Constrained (
         maxHeight = 100,
         child = RX.View (
             styles = [|Styles.greyExpandingBox|],
             children = [|LC.Text lipsum|]
         )
     )
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Constrained (
         maxHeight = 100,
         child = RX.View (
             styles = [|Styles.greyExpandingBox|],
             children = [|LC.Text lipsum|]
         )
     )
 """
                            |])

                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, heading = "Setup Code", children = [| LC.Text """

         module private Styles =
             let greyExpandingBox = makeViewStyles {
                 backgroundColor Color.DevLightGrey
                 flex 1
             }

         let private lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In hendrerit vehicula sollicitudin. Sed lacinia, libero ultrices mattis dignissim, libero purus interdum ante, eu pharetra tortor massa vel lorem. Donec dignissim felis quis nisl sodales, id lacinia felis congue. Sed porta ipsum sem, et interdum arcu fringilla ac. Maecenas tincidunt, leo non ultricies molestie, lectus odio sagittis tellus, a convallis neque sapien vel sem. Nullam a justo blandit, condimentum leo sit amet, egestas lectus. Nunc eu eros eget lorem condimentum facilisis eget ac leo. Nulla bibendum ex eu dui blandit, sit amet feugiat quam interdum. Maecenas iaculis pharetra ex a gravida. Integer faucibus venenatis commodo."
     """
                            |])
                        })
                )

                Ui.ComponentSample (
                    heading = """minWidth""",
                    visuals = (element {

     // hard to demonstrate minWidth without LC.Shrink wrapping the whole thing
     LC.Shrink (
         LC.Constrained (
             minWidth = 150,
             child = RX.View (
                 styles = [|Styles.greyExpandingBox|],
                 children = [|LC.Text "Little text"|]
             )
         )
     )
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     // hard to demonstrate minWidth without LC.Shrink wrapping the whole thing
     LC.Shrink (
         LC.Constrained (
             minWidth = 150,
             child = RX.View (
                 styles = [|Styles.greyExpandingBox|],
                 children = [|LC.Text "Little text"|]
             )
         )
     )
 """
                            |])

                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, heading = "Setup Code", children = [| LC.Text """

         module private Styles =
             let greyExpandingBox = makeViewStyles {
                 backgroundColor Color.DevLightGrey
                 flex 1
             }

         let private lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In hendrerit vehicula sollicitudin. Sed lacinia, libero ultrices mattis dignissim, libero purus interdum ante, eu pharetra tortor massa vel lorem. Donec dignissim felis quis nisl sodales, id lacinia felis congue. Sed porta ipsum sem, et interdum arcu fringilla ac. Maecenas tincidunt, leo non ultricies molestie, lectus odio sagittis tellus, a convallis neque sapien vel sem. Nullam a justo blandit, condimentum leo sit amet, egestas lectus. Nunc eu eros eget lorem condimentum facilisis eget ac leo. Nulla bibendum ex eu dui blandit, sit amet feugiat quam interdum. Maecenas iaculis pharetra ex a gravida. Integer faucibus venenatis commodo."
     """
                            |])
                        })
                )

                Ui.ComponentSample (
                    heading = """minHeight""",
                    visuals = (element {

     // hard to demonstrate minHeight without LC.Shrink wrapping the whole thing
     LC.Shrink (
         LC.Constrained (
             minHeight = 150,
             child = RX.View (
                 styles = [|Styles.greyExpandingBox|],
                 children = [|LC.Text "Little text"|]
             )
         )
     )
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     // hard to demonstrate minHeight without LC.Shrink wrapping the whole thing
     LC.Shrink (
         LC.Constrained (
             minHeight = 150,
             child = RX.View (
                 styles = [|Styles.greyExpandingBox|],
                 children = [|LC.Text "Little text"|]
             )
         )
     )
 """
                            |])

                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, heading = "Setup Code", children = [| LC.Text """

         module private Styles =
             let greyExpandingBox = makeViewStyles {
                 backgroundColor Color.DevLightGrey
                 flex 1
             }

         let private lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In hendrerit vehicula sollicitudin. Sed lacinia, libero ultrices mattis dignissim, libero purus interdum ante, eu pharetra tortor massa vel lorem. Donec dignissim felis quis nisl sodales, id lacinia felis congue. Sed porta ipsum sem, et interdum arcu fringilla ac. Maecenas tincidunt, leo non ultricies molestie, lectus odio sagittis tellus, a convallis neque sapien vel sem. Nullam a justo blandit, condimentum leo sit amet, egestas lectus. Nunc eu eros eget lorem condimentum facilisis eget ac leo. Nulla bibendum ex eu dui blandit, sit amet feugiat quam interdum. Maecenas iaculis pharetra ex a gravida. Integer faucibus venenatis commodo."
     """
                            |])
                        })
                )

            }
        )


module private Module_InProgress =

    [<Component>]
    let content () : ReactElement =
        Ui.ComponentContent (
            displayName = "InProgress",
            
            props = AppEggShellGallery.Components.ComponentContent.PropsConfig.Manual (
                Ui.ComponentProps (data = {
                    Fields = (Choice2Of2 [

                        {
                            Name = "isInProgress"
                            Type = "bool"
                            Default = None
                            Description = None
                        }
                        {
                            Name = "children"
                            Type = "array<ReactElement>"
                            Default = None
                            Description = None
                        }
                        {
                            Name = "styles"
                            Type = "array<ViewStyles>"
                            Default = Some "[||]"
                            Description = None
                        }
                    ])
                    MaybeScrapeErrors = None
                })
            ),
            notes       = LC.Text """Superimpose a spinner and a scrim on top of children when in progress. TODO: theme for scrim colour, spinner colour and size.""",
            samples     = element {
                
                Ui.ComponentSample (
                    heading = """Basics""",
                    visuals = (element {

     LC.Column (gap = 20, children = [|
         LC.InProgress (false, [|
             LC.InfoMessage "Some content here"
         |])
         LC.InProgress (true, [|
             LC.InfoMessage "Some content here"
         |])
     |])
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Column (gap = 20, children = [|
         LC.InProgress (false, [|
             LC.InfoMessage "Some content here"
         |])
         LC.InProgress (true, [|
             LC.InfoMessage "Some content here"
         |])
     |])
 """
                            |])

                        })
                )

            }
        )


module private Module_With_Executor =

    let private action () : UDActionResult = async {
        do! Async.Sleep (System.TimeSpan.FromSeconds 1)
        return Ok ()
    }

    [<Component>]
    let content () : ReactElement =
        Ui.ComponentContent (
            displayName = "With.Executor",
            
            props = AppEggShellGallery.Components.ComponentContent.PropsConfig.Manual (
                Ui.ComponentProps (data = {
                    Fields = (Choice2Of2 [

                        {
                            Name = "executor"
                            Type = "LibClient.UniDirectionalDataFlow.Executor"
                            Default = None
                            Description = None
                        }
                        {
                            Name = "content"
                            Type = "LibClient.UniDirectionalDataFlow.Executor -> array<ReactElement>"
                            Default = None
                            Description = None
                        }
                        {
                            Name = "styles"
                            Type = "array<ViewStyles>"
                            Default = Some "[||]"
                            Description = None
                        }
                    ])
                    MaybeScrapeErrors = None
                })
            ),
            notes       = LC.Text """An ad hoc block with manual executor usage that handles the in-progress state using LC.InProgress.
 TODO: plumb through the LC.InProgress theme when implemented""",
            samples     = element {
                
                Ui.ComponentSample (
                    heading = """Basics""",
                    visuals = (element {

     LC.Executor.AlertErrors (fun makeExecutor -> element {
         LC.With.Executor (makeExecutor "test", fun executor -> [|
             RX.View (
                 onPress = (fun _ -> executor.MaybeExecute action),
                 children = [|
                     LC.InfoMessage "Press Here"
                 |]
             )
         |])
     })
 
                    }),
                    code =
                        ComponentSample.Children (element {
                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, children = [| LC.Text """

     LC.Executor.AlertErrors (fun makeExecutor -> element {
         LC.With.Executor (makeExecutor "test", fun executor -> [|
             RX.View (
                 onPress = (fun _ -> executor.MaybeExecute action),
                 children = [|
                     LC.InfoMessage "Press Here"
                 |]
             )
         |])
     })
 """
                            |])

                            Ui.Code (language = AppEggShellGallery.Components.Code.Fsharp, heading = "Setup Code", children = [| LC.Text """

         let private action () : UDActionResult = async {
             do! Async.Sleep (System.TimeSpan.FromSeconds 1)
             return Ok ()
         }
     """
                            |])
                        })
                )

            }
        )


type Ui.XmlDocsContent.LC with
    
    static member LibClient_Disposables_SerialDisposable () : ReactElement =
        Module_LibClient_Disposables_SerialDisposable.content ()

    static member LibClient_Disposables_CompositeDisposable () : ReactElement =
        Module_LibClient_Disposables_CompositeDisposable.content ()

    static member Fragment () : ReactElement =
        Module_Fragment.content ()

    static member Column () : ReactElement =
        Module_Column.content ()

    static member Row () : ReactElement =
        Module_Row.content ()

    static member Sized () : ReactElement =
        Module_Sized.content ()

    static member Constrained () : ReactElement =
        Module_Constrained.content ()

    static member InProgress () : ReactElement =
        Module_InProgress.content ()

    static member With_Executor () : ReactElement =
        Module_With_Executor.content ()
    