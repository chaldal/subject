import { map, defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../../Components/Nav/Top/Base/Base.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeNav_Top_BaseProps_Static_Z37BA32B(pHandheld, pDesktop, pkey, pkeyOption) {
    return new Props(pHandheld, pDesktop, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

