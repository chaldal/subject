import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props } from "../../../../ReactXP/Components/ActivityIndicator/ActivityIndicator.typext.fs.js";

export function ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B(pcolor, psize, pdeferTime, psizeOption, pdeferTimeOption) {
    return new Props(pcolor, defaultArg(psizeOption, defaultArg(map((arg0) => arg0, psize), undefined)), defaultArg(pdeferTimeOption, defaultArg(map((arg0_1) => arg0_1, pdeferTime), undefined)));
}

