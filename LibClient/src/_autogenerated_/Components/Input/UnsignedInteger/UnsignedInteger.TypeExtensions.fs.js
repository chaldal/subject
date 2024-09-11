import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../Components/Input/UnsignedInteger/UnsignedInteger.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_UnsignedIntegerProps_Static_313612A9(pValue, pValidity, pOnChange, pRequestFocusOnMount, pLabel, pPlaceholder, pPrefix, pSuffix, pOnKeyPress, pOnEnterKeyPress, pkey, pLabelOption, pPlaceholderOption, pPrefixOption, pSuffixOption, pOnKeyPressOption, pOnEnterKeyPressOption, pkeyOption) {
    const RequestFocusOnMount = defaultArg(pRequestFocusOnMount, false);
    return new Props(defaultArg(pLabelOption, defaultArg(map((arg0) => arg0, pLabel), void 0)), pValue, pValidity, defaultArg(pPlaceholderOption, defaultArg(map((arg0_1) => arg0_1, pPlaceholder), void 0)), defaultArg(pPrefixOption, defaultArg(map((arg0_2) => arg0_2, pPrefix), void 0)), defaultArg(pSuffixOption, defaultArg(map((arg0_3) => arg0_3, pSuffix), void 0)), RequestFocusOnMount, pOnChange, defaultArg(pOnKeyPressOption, defaultArg(map((arg0_4) => arg0_4, pOnKeyPress), void 0)), defaultArg(pOnEnterKeyPressOption, defaultArg(map((arg0_5) => arg0_5, pOnEnterKeyPress), void 0)), defaultArg(pkeyOption, defaultArg(map((arg0_6) => arg0_6, pkey), JsUndefined())));
}

