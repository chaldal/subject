import { map, defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../../AdditionalAutoOpens.fs.js";
import { Props$1 } from "../../../../../Components/Legacy/Input/Picker/Picker.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_Input_PickerProps_Static_3FD7D463(pItems, pValue, pOnChange, pValidity, pLabel, pkey, pLabelOption, pkeyOption) {
    return new Props$1(pItems, pValue, pOnChange, defaultArg(pLabelOption, defaultArg(map((arg0) => arg0, pLabel), void 0)), pValidity, defaultArg(pkeyOption, defaultArg(map((arg0_1) => arg0_1, pkey), JsUndefined())));
}

