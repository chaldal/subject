import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../../../LibClient/src/AdditionalAutoOpens.fs.js";
import { Props } from "../../../../Components/With/Location/Location.typext.fs.js";

export function LibRouter_Components_TypeExtensions_TEs__TEs_MakeWith_LocationProps_Static_Z7FC62227(pWith, pkey, pkeyOption) {
    return new Props(pWith, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

