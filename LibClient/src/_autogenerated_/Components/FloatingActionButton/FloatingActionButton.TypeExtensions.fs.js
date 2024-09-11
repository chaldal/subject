import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props } from "../../../Components/FloatingActionButton/FloatingActionButton.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeFloatingActionButtonProps_Static_556CAD94(pIcon, pState, pLabel, pLabelOption) {
    return new Props(pIcon, defaultArg(pLabelOption, defaultArg(map((arg0) => arg0, pLabel), void 0)), pState);
}

