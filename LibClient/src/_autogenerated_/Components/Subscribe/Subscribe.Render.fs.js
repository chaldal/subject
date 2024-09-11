import { defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../ReactXP/Styles/Runtime.fs.js";
import { nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElements, RenderResult_Make_2B31D457, RenderResult_Make_6E3A73D, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../RenderResult.fs.js";
import { isEmpty, singleton } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeAsyncDataProps_Static_379524B4 } from "../AsyncData/AsyncData.TypeExtensions.fs.js";
import { empty, singleton as singleton_1, append, delay, toList } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { Make } from "../../../Components/AsyncData/AsyncData.typext.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    let matchValue, render_2, __list_1, render_1, __list;
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, (matchValue = props.With, (matchValue.tag === 0) ? ((render_2 = matchValue.fields[0], RenderResult_Make_6E3A73D((__list_1 = singleton(RenderResult_Make_2B31D457(render_2(estate.Value))), RenderResult_ToRawElements(__parentFQN, __list_1))))) : ((render_1 = matchValue.fields[0], RenderResult_Make_6E3A73D((__list = singleton(RenderResult_Make_E25108F((tupledArg) => {
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.AsyncData";
        let arg00;
        const __currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeAsyncDataProps_Static_379524B4(estate.Value, render_1);
        const __currClass = nthChildClasses(__sibI, __sibC);
        const __currStyles = findApplicableStyles(__mergedStyles, __currClass);
        const __implicitProps = toList(delay(() => append((__currClass !== "") ? singleton_1(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton_1(["__style", __currStyles]) : empty())))));
        arg00 = injectImplicitProps(__implicitProps, __currExplicitProps);
        return Make()(arg00)([]);
    })), RenderResult_ToRawElements(__parentFQN, __list))))))));
}

