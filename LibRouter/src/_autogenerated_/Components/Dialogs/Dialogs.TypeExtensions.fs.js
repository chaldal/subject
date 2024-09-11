import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../../LibClient/src/AdditionalAutoOpens.fs.js";
import { Props$3 } from "../../../Components/Dialogs/Dialogs.typext.fs.js";

export function LibRouter_Components_TypeExtensions_TEs__TEs_MakeDialogsProps_Static_Z3EEC96A5(pNav, pDialogs, pDialogsState, pMakeResultless, pMakeResultful, pkey, pkeyOption) {
    return new Props$3(pNav, pDialogs, pDialogsState, pMakeResultless, pMakeResultful, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

