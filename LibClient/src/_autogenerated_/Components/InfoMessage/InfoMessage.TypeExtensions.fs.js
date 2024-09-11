import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props, Level } from "../../../Components/InfoMessage/InfoMessage.typext.fs.js";
import { JsUndefined } from "../../../AdditionalAutoOpens.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInfoMessageProps_Static_6A7D6521(pMessage, pLevel, pkey, pkeyOption) {
    return new Props(defaultArg(pLevel, new Level(0)), pMessage, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

