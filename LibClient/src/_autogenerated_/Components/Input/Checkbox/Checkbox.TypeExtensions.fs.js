import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props, Label } from "../../../../Components/Input/Checkbox/Checkbox.typext.fs.js";
import { noElement } from "../../../../EggShellReact.fs.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_CheckboxProps_Static_177C9590(pOnChange, pValue, pValidity, pLabel, pchildren, pkey, pkeyOption) {
    return new Props(pOnChange, pValue, defaultArg(pLabel, new Label(1)), defaultArg(pchildren, noElement), pValidity, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

