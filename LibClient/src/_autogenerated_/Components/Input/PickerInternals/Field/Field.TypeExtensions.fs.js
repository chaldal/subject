import { map, defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../../AdditionalAutoOpens.fs.js";
import { Props$1 } from "../../../../../Components/Input/PickerInternals/Field/Field.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_PickerInternals_FieldProps_Static_Z52658B7F(pModel, pValue, pValidity, pItemView, pLabel, pPlaceholder, pkey, pLabelOption, pPlaceholderOption, pkeyOption) {
    return new Props$1(pModel, defaultArg(pLabelOption, defaultArg(map((arg0) => arg0, pLabel), void 0)), defaultArg(pPlaceholderOption, defaultArg(map((arg0_1) => arg0_1, pPlaceholder), void 0)), pValue, pValidity, pItemView, defaultArg(pkeyOption, defaultArg(map((arg0_2) => arg0_2, pkey), JsUndefined())));
}

