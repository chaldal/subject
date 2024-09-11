import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../Components/Input/DayOfTheWeek/DayOfTheWeek.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_DayOfTheWeekProps_Static_ZEBCB235(pMode, pValidity, pLabel, pkey, pLabelOption, pkeyOption) {
    return new Props(defaultArg(pLabelOption, defaultArg(map((arg0) => arg0, pLabel), void 0)), pMode, pValidity, defaultArg(pkeyOption, defaultArg(map((arg0_1) => arg0_1, pkey), JsUndefined())));
}

