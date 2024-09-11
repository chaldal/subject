import { defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../LibClient/src/ReactXP/ReactXPBindings.fs.js";
import { mergeComponentAndPropsStyles } from "../../../../../LibClient/src/ReactXP/Styles/Runtime.fs.js";
import { makeTextNode, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../LibClient/src/RenderResult.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => makeTextNode("not used, providing a render function directly", tupledArg[0], tupledArg[1], tupledArg[2]))));
}

