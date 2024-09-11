import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../../LibClient/src/AdditionalAutoOpens.fs.js";
import { Props } from "../../../Components/LogRouteTransitions/LogRouteTransitions.typext.fs.js";

export function LibRouter_Components_TypeExtensions_TEs__TEs_MakeLogRouteTransitionsProps_Static_Z6CCA534B(pkey, pkeyOption) {
    return new Props(defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

