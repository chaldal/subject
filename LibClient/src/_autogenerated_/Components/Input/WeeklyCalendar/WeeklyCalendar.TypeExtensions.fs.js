import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../Components/Input/WeeklyCalendar/WeeklyCalendar.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_WeeklyCalendarProps_Static_FBD89F6(pValue, pOnChange, pValidity, pLabel, pStartDate, pkey, pLabelOption, pStartDateOption, pkeyOption) {
    return new Props(defaultArg(pLabelOption, defaultArg(map((arg0) => arg0, pLabel), void 0)), pValue, pOnChange, pValidity, defaultArg(pStartDateOption, defaultArg(map((arg0_1) => arg0_1, pStartDate), void 0)), defaultArg(pkeyOption, defaultArg(map((arg0_2) => arg0_2, pkey), JsUndefined())));
}

