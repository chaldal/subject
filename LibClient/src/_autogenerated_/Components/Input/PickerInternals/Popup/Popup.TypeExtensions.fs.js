import { map, defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../../AdditionalAutoOpens.fs.js";
import { Props$1 } from "../../../../../Components/Input/PickerInternals/Popup/Popup.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_PickerInternals_PopupProps_Static_34554862(pModel, pItemView, pkey, pkeyOption) {
    return new Props$1(pModel, pItemView, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

