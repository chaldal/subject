import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { noElement } from "../../../EggShellReact.fs.js";
import { JsUndefined } from "../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../Components/Draggable/Draggable.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeDraggableProps_Static_Z2F4A4793(pBaseOffset, pchildren, pLeft, pRight, pUp, pDown, pOnChange, pkey, pref, pLeftOption, pRightOption, pUpOption, pDownOption, pOnChangeOption, pkeyOption, prefOption) {
    const BaseOffset = defaultArg(pBaseOffset, [0, 0]);
    const children = defaultArg(pchildren, noElement);
    return new Props(defaultArg(pLeftOption, defaultArg(map((arg0) => arg0, pLeft), void 0)), defaultArg(pRightOption, defaultArg(map((arg0_1) => arg0_1, pRight), void 0)), defaultArg(pUpOption, defaultArg(map((arg0_2) => arg0_2, pUp), void 0)), defaultArg(pDownOption, defaultArg(map((arg0_3) => arg0_3, pDown), void 0)), BaseOffset, children, defaultArg(pOnChangeOption, defaultArg(map((arg0_4) => arg0_4, pOnChange), void 0)), defaultArg(pkeyOption, defaultArg(map((arg0_5) => arg0_5, pkey), JsUndefined())), defaultArg(prefOption, defaultArg(map((arg0_6) => arg0_6, pref), JsUndefined())));
}

