import { defaultArg } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props, DialogPosition } from "../../../../../../Components/Dialog/Shell/WhiteRounded/Raw/Raw.typext.fs.js";
import { noElement } from "../../../../../../EggShellReact.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_Shell_WhiteRounded_RawProps_Static_Z79AF04F8(pCanClose, pPosition, pchildren, pInProgress) {
    return new Props(pCanClose, defaultArg(pPosition, new DialogPosition(1)), defaultArg(pchildren, noElement), defaultArg(pInProgress, false));
}

