import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_nothing } from "../../../../../AppSample2/src/fable_modules/Fable.React.5.3.6/Fable.React.Helpers.fs.js";
import { Props } from "../../../Components/Link/Link.typext.fs.js";

export function LibRouter_Components_TypeExtensions_TEs__TEs_MakeLinkProps_Static_Z560BCBD4(pTo, pchildren, pkey, pkeyOption) {
    return new Props(pTo, defaultArg(pchildren, Helpers_nothing), defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), undefined)));
}

