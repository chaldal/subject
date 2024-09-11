import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props$1 } from "../../../../Components/Input/Picker/Picker.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_PickerProps_Static_Z3F47FAF2(pItems, pItemView, pValue, pValidity, pLabel, pPlaceholder, pkey, pLabelOption, pPlaceholderOption, pkeyOption) {
    return new Props$1(defaultArg(pLabelOption, defaultArg(map((arg0) => arg0, pLabel), void 0)), defaultArg(pPlaceholderOption, defaultArg(map((arg0_1) => arg0_1, pPlaceholder), void 0)), pItems, pItemView, pValue, pValidity, defaultArg(pkeyOption, defaultArg(map((arg0_2) => arg0_2, pkey), JsUndefined())));
}

