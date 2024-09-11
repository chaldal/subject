import { defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToRawElements, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../RenderResult.fs.js";
import { Make } from "../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { format } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, isEmpty } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_1 } from "../../../Components/Icon/Icon.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B } from "../Icon/Icon.TypeExtensions.fs.js";
import { Make as Make_2 } from "../../../Components/Badge/Badge.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeBadgeProps_Static_ZE5CF26C } from "../Badge/Badge.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, void 0];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = (format("{0}{1}{2}", "icon-with-count ", TopLevelBlockClass, "") + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list = ofArray([RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "LibClient.Components.Icon";
            return Make_1((__currExplicitProps_1 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(props.Icon), (__currClass_1 = ("icon" + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["__style", __currStyles_1]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))([]);
        }), RenderResult_Make_E25108F((tupledArg_2) => {
            let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
            const __sibI_2 = tupledArg_2[0] | 0;
            const __sibC_2 = tupledArg_2[1] | 0;
            const __pFQN_2 = tupledArg_2[2];
            const __parentFQN_3 = "LibClient.Components.Badge";
            return Make_2((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeBadgeProps_Static_ZE5CF26C(props.Badge), (__currClass_2 = ("badge" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))([]);
        })]), RenderResult_ToRawElements(__parentFQN_1, __list)));
    })));
}

