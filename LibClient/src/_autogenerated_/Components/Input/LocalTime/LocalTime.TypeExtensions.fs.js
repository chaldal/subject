import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../Components/Input/LocalTime/LocalTime.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_LocalTimeProps_Static_Z752BE297(pValue, pValidity, pOnChange, pRequestFocusOnMount, pLabel, pOnEnterKeyPress, pkey, pLabelOption, pOnEnterKeyPressOption, pkeyOption) {
    const RequestFocusOnMount = defaultArg(pRequestFocusOnMount, false);
    return new Props(defaultArg(pLabelOption, defaultArg(map((arg0) => arg0, pLabel), void 0)), pValue, pValidity, pOnChange, defaultArg(pOnEnterKeyPressOption, defaultArg(map((arg0_1) => arg0_1, pOnEnterKeyPress), void 0)), RequestFocusOnMount, defaultArg(pkeyOption, defaultArg(map((arg0_2) => arg0_2, pkey), JsUndefined())));
}

