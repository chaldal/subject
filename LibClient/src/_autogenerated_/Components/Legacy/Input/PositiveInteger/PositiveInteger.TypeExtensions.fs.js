import { map, defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../../Components/Legacy/Input/PositiveInteger/PositiveInteger.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_Input_PositiveIntegerProps_Static_Z767EA171(pOnChange, pInitialValue, pOnKeyPress, pref, pInitialValueOption, pOnKeyPressOption, prefOption) {
    return new Props(defaultArg(pInitialValueOption, defaultArg(map((arg0) => arg0, pInitialValue), void 0)), pOnChange, defaultArg(pOnKeyPressOption, defaultArg(map((arg0_1) => arg0_1, pOnKeyPress), JsUndefined())), defaultArg(prefOption, defaultArg(map((arg0_2) => arg0_2, pref), JsUndefined())));
}

