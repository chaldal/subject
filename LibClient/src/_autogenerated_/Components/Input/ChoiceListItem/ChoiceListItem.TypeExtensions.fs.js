import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props$1, Label } from "../../../../Components/Input/ChoiceListItem/ChoiceListItem.typext.fs.js";
import { noElement } from "../../../../EggShellReact.fs.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_ChoiceListItemProps_Static_1B35FD7C(pValue, pGroup, pLabel, pchildren, pkey, pkeyOption) {
    return new Props$1(pValue, defaultArg(pLabel, new Label(1)), defaultArg(pchildren, noElement), pGroup, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

