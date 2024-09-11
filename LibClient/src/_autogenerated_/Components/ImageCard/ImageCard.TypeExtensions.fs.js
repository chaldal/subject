import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { noElement } from "../../../EggShellReact.fs.js";
import { JsUndefined } from "../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../Components/ImageCard/ImageCard.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeImageCardProps_Static_1F2A354C(pSource, pchildren, pLabel, pOnPress, pkey, pLabelOption, pOnPressOption, pkeyOption) {
    const children = defaultArg(pchildren, noElement);
    return new Props(pSource, defaultArg(pLabelOption, defaultArg(map((arg0) => arg0, pLabel), void 0)), defaultArg(pOnPressOption, defaultArg(map((arg0_1) => arg0_1, pOnPress), void 0)), children, defaultArg(pkeyOption, defaultArg(map((arg0_2) => arg0_2, pkey), JsUndefined())));
}

