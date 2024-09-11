import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props$1 } from "../../../../Components/Input/ParsedText/ParsedText.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_ParsedTextProps_Static_Z4D906D6A(pParse, pValue, pValidity, pRequestFocusOnMount, pOnChange, pLabel, pPlaceholder, pPrefix, pSuffix, pOnKeyPress, pOnEnterKeyPress, pkey, pLabelOption, pPlaceholderOption, pPrefixOption, pSuffixOption, pOnKeyPressOption, pOnEnterKeyPressOption, pkeyOption) {
    return new Props$1(pParse, defaultArg(pLabelOption, defaultArg(map((arg0) => arg0, pLabel), void 0)), pValue, pValidity, defaultArg(pPlaceholderOption, defaultArg(map((arg0_1) => arg0_1, pPlaceholder), void 0)), defaultArg(pPrefixOption, defaultArg(map((arg0_2) => arg0_2, pPrefix), void 0)), defaultArg(pSuffixOption, defaultArg(map((arg0_3) => arg0_3, pSuffix), void 0)), pRequestFocusOnMount, pOnChange, defaultArg(pOnKeyPressOption, defaultArg(map((arg0_4) => arg0_4, pOnKeyPress), void 0)), defaultArg(pOnEnterKeyPressOption, defaultArg(map((arg0_5) => arg0_5, pOnEnterKeyPress), void 0)), defaultArg(pkeyOption, defaultArg(map((arg0_6) => arg0_6, pkey), JsUndefined())));
}

