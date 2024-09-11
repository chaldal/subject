import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../../../LibClient/src/AdditionalAutoOpens.fs.js";
import { Props$3 } from "../../../../Components/With/Navigation/Navigation.typext.fs.js";

export function LibRouter_Components_TypeExtensions_TEs__TEs_MakeWith_NavigationProps_Static_Z5DB2E628(pSpec, pDialogsState, pWith, pkey, pkeyOption) {
    return new Props$3(pSpec, pDialogsState, pWith, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

