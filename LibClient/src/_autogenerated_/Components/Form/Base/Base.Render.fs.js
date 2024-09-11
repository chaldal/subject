import { defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_2B31D457, RenderResult_ToRawElements, nthChildClasses, RenderResult_Make_E25108F, RenderResult_Make_6E3A73D, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../Components/Executor/AlertErrors/AlertErrors.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeExecutor_AlertErrorsProps_Static_3E935B28 } from "../../Executor/AlertErrors/AlertErrors.TypeExtensions.fs.js";
import { makeFormHandle } from "../../../../Components/Form/Base/Base.typext.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { singleton as singleton_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    let matchValue, __list_1, executor, __list;
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, (matchValue = props.Executor, (matchValue == null) ? RenderResult_Make_6E3A73D((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.Executor.AlertErrors";
        return Make((__currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeExecutor_AlertErrorsProps_Static_3E935B28((executor_1) => props.Content(makeFormHandle(executor_1, estate, actions))), (__currClass = nthChildClasses(__sibI, __sibC), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))([]);
    })), RenderResult_ToRawElements(__parentFQN, __list_1))) : ((executor = matchValue, RenderResult_Make_6E3A73D((__list = singleton_1(RenderResult_Make_2B31D457(props.Content(makeFormHandle(executor, estate, actions)))), RenderResult_ToRawElements(__parentFQN, __list))))))));
}

