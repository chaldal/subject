import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props, ReplacedExisting } from "../../../../Components/Input/Image/Image.typext.fs.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_ImageProps_Static_4CEE3426(pValue, pValidity, pOnChange, pShowPreview, pSelectionMode, pMaxFileCount, pMaxFileSize, pkey, pMaxFileCountOption, pMaxFileSizeOption, pkeyOption) {
    const ShowPreview = defaultArg(pShowPreview, true);
    const SelectionMode = defaultArg(pSelectionMode, ReplacedExisting);
    return new Props(pValue, pValidity, pOnChange, defaultArg(pMaxFileCountOption, defaultArg(map((arg0) => arg0, pMaxFileCount), void 0)), defaultArg(pMaxFileSizeOption, defaultArg(map((arg0_1) => arg0_1, pMaxFileSize), void 0)), ShowPreview, SelectionMode, defaultArg(pkeyOption, defaultArg(map((arg0_2) => arg0_2, pkey), JsUndefined())));
}

