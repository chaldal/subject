import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props } from "../../../../ReactXP/Components/Picker/Picker.typext.fs.js";

export function ReactXP_Components_TypeExtensions_TEs__TEs_MakePickerProps_Static_Z28D585EA(pitems, pselectedValue, ponValueChange, pmode, pmodeOption) {
    return new Props(pitems, pselectedValue, ponValueChange, defaultArg(pmodeOption, defaultArg(map((arg0) => arg0, pmode), undefined)));
}

