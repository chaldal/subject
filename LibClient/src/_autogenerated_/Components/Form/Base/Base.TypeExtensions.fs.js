import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props$3 } from "../../../../Components/Form/Base/Base.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeForm_BaseProps_Static_Z556E86D6(pAccumulator, pSubmit, pContent, pExecutor, pkey, pExecutorOption, pkeyOption) {
    return new Props$3(pAccumulator, pSubmit, pContent, defaultArg(pExecutorOption, defaultArg(map((arg0) => arg0, pExecutor), void 0)), defaultArg(pkeyOption, defaultArg(map((arg0_1) => arg0_1, pkey), JsUndefined())));
}

