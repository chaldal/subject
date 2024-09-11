import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props } from "../../../Components/DateSelector/DateSelector.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeDateSelectorProps_Static_5095506A(pOnChange, pMaybeSelected, pMinDate, pMinDateOption) {
    return new Props(pOnChange, pMaybeSelected, defaultArg(pMinDateOption, defaultArg(map((arg0) => arg0, pMinDate), void 0)));
}

