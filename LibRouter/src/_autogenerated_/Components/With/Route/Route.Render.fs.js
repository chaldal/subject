import { defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../../LibClient/src/ReactXP/ReactXPBindings.fs.js";
import { mergeComponentAndPropsStyles } from "../../../../../../LibClient/src/ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_2B31D457, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../../LibClient/src/RenderResult.fs.js";
import { decodeLocation } from "../../../../Components/With/Route/Route.typext.fs.js";
import { useLocation } from "../../../../Components/Router/Router.typext.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_2B31D457(props.With(decodeLocation(props.Spec, useLocation())))));
}

