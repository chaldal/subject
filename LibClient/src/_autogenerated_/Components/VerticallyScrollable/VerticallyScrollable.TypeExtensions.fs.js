import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { JsUndefined } from "../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../Components/VerticallyScrollable/VerticallyScrollable.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeVerticallyScrollableProps_Static_Z36E2C840(pFixedTop, pFixedBottom, pScrollableMiddle, pkey, pFixedTopOption, pFixedBottomOption, pScrollableMiddleOption, pkeyOption) {
    return new Props(defaultArg(pFixedTopOption, defaultArg(map((arg0) => arg0, pFixedTop), void 0)), defaultArg(pFixedBottomOption, defaultArg(map((arg0_1) => arg0_1, pFixedBottom), void 0)), defaultArg(pScrollableMiddleOption, defaultArg(map((arg0_2) => arg0_2, pScrollableMiddle), void 0)), defaultArg(pkeyOption, defaultArg(map((arg0_3) => arg0_3, pkey), JsUndefined())));
}

