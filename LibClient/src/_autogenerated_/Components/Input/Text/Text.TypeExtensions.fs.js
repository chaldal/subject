import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../Components/Input/Text/Text.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_TextProps_Static_CF9FCE9(pValue, pOnChange, pValidity, pMultiline, pRequestFocusOnMount, pSecureTextEntry, pLabel, pOnKeyPress, pOnEnterKeyPress, pOnFocus, pOnBlur, pPlaceholder, pPrefix, pSuffix, pMaxLength, pkey, pref, pLabelOption, pOnKeyPressOption, pOnEnterKeyPressOption, pOnFocusOption, pOnBlurOption, pPlaceholderOption, pPrefixOption, pSuffixOption, pMaxLengthOption, pkeyOption, prefOption) {
    const Multiline = defaultArg(pMultiline, false);
    const RequestFocusOnMount = defaultArg(pRequestFocusOnMount, false);
    const SecureTextEntry = defaultArg(pSecureTextEntry, false);
    return new Props(pValue, pOnChange, defaultArg(pLabelOption, defaultArg(map((arg0) => arg0, pLabel), void 0)), defaultArg(pOnKeyPressOption, defaultArg(map((arg0_1) => arg0_1, pOnKeyPress), void 0)), defaultArg(pOnEnterKeyPressOption, defaultArg(map((arg0_2) => arg0_2, pOnEnterKeyPress), void 0)), defaultArg(pOnFocusOption, defaultArg(map((arg0_3) => arg0_3, pOnFocus), void 0)), defaultArg(pOnBlurOption, defaultArg(map((arg0_4) => arg0_4, pOnBlur), void 0)), defaultArg(pPlaceholderOption, defaultArg(map((arg0_5) => arg0_5, pPlaceholder), void 0)), defaultArg(pPrefixOption, defaultArg(map((arg0_6) => arg0_6, pPrefix), void 0)), defaultArg(pSuffixOption, defaultArg(map((arg0_7) => arg0_7, pSuffix), void 0)), defaultArg(pMaxLengthOption, defaultArg(map((arg0_8) => arg0_8, pMaxLength), void 0)), Multiline, RequestFocusOnMount, SecureTextEntry, pValidity, defaultArg(pkeyOption, defaultArg(map((arg0_9) => arg0_9, pkey), JsUndefined())), defaultArg(prefOption, defaultArg(map((arg0_10) => arg0_10, pref), JsUndefined())));
}

