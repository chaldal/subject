import { defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToLeaves_410EF94B, RenderResult_Make_2B31D457, RenderResult_Make_Z1C694E5A, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Actions__Reset_411CDB90, Actions__RunAction_6DE5E21E } from "../../../../Components/TriStateful/Abstract/Abstract.typext.fs.js";
import { singleton } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    let __list;
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_Z1C694E5A((__list = singleton(RenderResult_Make_2B31D457(props.Content([estate.Mode, (arg00) => {
        Actions__RunAction_6DE5E21E(actions, arg00);
    }, (arg00_1) => {
        Actions__Reset_411CDB90(actions, arg00_1);
    }]))), RenderResult_ToLeaves_410EF94B(__list)))));
}

