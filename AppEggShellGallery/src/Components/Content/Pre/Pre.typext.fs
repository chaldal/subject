module AppEggShellGallery.Components.Content.Pre

open LibClient

let text =
    """
let x := y

(*
     __//
cd  /.__.\
    \ \/ /
 '__/    \
  \-      )
   \_____/
_____|_|____
 " "
*)
    """

type Props = (* GenerateMakeFunction *) {
    key: string option // defaultWithAutoWrap JsUndefined
}

type Pre(_initialProps) =
    inherit PureStatelessComponent<Props, Actions, Pre>("AppEggShellGallery.Components.Content.Pre", _initialProps, Actions, hasStyles = true)

and Actions(_this: Pre) =
    class end

let Make = makeConstructor<Pre, _, _>

// Unfortunately necessary boilerplate
type Estate = NoEstate
type Pstate = NoPstate
