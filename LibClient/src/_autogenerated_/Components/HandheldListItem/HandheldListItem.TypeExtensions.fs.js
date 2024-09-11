import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { noElement } from "../../../EggShellReact.fs.js";
import { Props } from "../../../Components/HandheldListItem/HandheldListItem.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeHandheldListItemProps_Static_70D85944(pLabel, pState, pLeftIcon, pchildren, pRight, pRightOption) {
    const LeftIcon = defaultArg(pLeftIcon, void 0);
    const children = defaultArg(pchildren, noElement);
    return new Props(pLabel, pState, LeftIcon, defaultArg(pRightOption, defaultArg(map((arg0) => arg0, pRight), void 0)), children);
}

