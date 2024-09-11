import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { noElement } from "../../../EggShellReact.fs.js";
import { JsUndefined } from "../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../Components/ScrollView/ScrollView.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_Z7E0BACE1(pScroll, pRestoreScroll, pchildren, pOnLayout, pref, pkey, pOnLayoutOption, prefOption, pkeyOption) {
    const children = defaultArg(pchildren, noElement);
    return new Props(pScroll, pRestoreScroll, defaultArg(pOnLayoutOption, defaultArg(map((arg0) => arg0, pOnLayout), void 0)), children, defaultArg(prefOption, defaultArg(map((arg0_1) => arg0_1, pref), undefined)), defaultArg(pkeyOption, defaultArg(map((arg0_2) => arg0_2, pkey), JsUndefined())));
}

