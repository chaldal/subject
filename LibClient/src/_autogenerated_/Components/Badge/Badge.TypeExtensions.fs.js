import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../Components/Badge/Badge.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeBadgeProps_Static_ZE5CF26C(pBadge, pkey, pkeyOption) {
    return new Props(pBadge, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

