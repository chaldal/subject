import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { noElement } from "../../../../EggShellReact.fs.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../Components/Section/Padded/Padded.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeSection_PaddedProps_Static_Z7C92D749(pchildren, pkey, pkeyOption) {
    return new Props(defaultArg(pchildren, noElement), defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

