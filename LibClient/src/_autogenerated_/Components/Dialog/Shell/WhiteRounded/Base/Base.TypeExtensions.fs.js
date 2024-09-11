import { map, defaultArg } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { noElement } from "../../../../../../EggShellReact.fs.js";
import { JsUndefined } from "../../../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../../../Components/Dialog/Shell/WhiteRounded/Base/Base.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_Shell_WhiteRounded_BaseProps_Static_Z5E99F378(pCanClose, pInProgress, pchildren, pkey, pkeyOption) {
    return new Props(pCanClose, defaultArg(pInProgress, false), defaultArg(pchildren, noElement), defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

