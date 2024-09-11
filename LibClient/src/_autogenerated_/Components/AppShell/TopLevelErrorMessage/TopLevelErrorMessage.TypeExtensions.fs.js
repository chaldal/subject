import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../Components/AppShell/TopLevelErrorMessage/TopLevelErrorMessage.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeAppShell_TopLevelErrorMessageProps_Static_7D2FF8BA(pError, pRetry, pkey, pkeyOption) {
    return new Props(pError, pRetry, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

