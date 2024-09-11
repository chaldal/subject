import { curry, uncurry } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props } from "../../../../Components/Sidebar/Item/Item.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeSidebar_ItemProps_Static_195C0A54(pLabel, pState, pLeftIcon, pRight, pLeftIconOption, pRightOption) {
    return new Props(pLabel, uncurry(2, defaultArg(pLeftIconOption, defaultArg(map((arg0) => arg0, curry(2, pLeftIcon)), void 0))), defaultArg(pRightOption, defaultArg(map((arg0_1) => arg0_1, pRight), void 0)), pState);
}

