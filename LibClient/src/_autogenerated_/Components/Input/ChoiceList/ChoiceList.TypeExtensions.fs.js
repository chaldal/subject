import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props$1 } from "../../../../Components/Input/ChoiceList/ChoiceList.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_ChoiceListProps_Static_5595B43E(pItems, pValue, pValidity, pkey, pkeyOption) {
    return new Props$1(pItems, pValue, pValidity, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

