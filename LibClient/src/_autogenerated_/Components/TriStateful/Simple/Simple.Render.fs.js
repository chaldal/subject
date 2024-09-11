import { defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult, makeTextNode, RenderResult_ToRawElements, nthChildClasses, RenderResult_Make_6E3A73D, RenderResult_Make_2B31D457, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../Components/TriStateful/Abstract/Abstract.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTriStateful_AbstractProps_Static_Z5186E7B4 } from "../Abstract/Abstract.TypeExtensions.fs.js";
import { Make as Make_1 } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, singleton as singleton_1, empty as empty_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_2 } from "../../../../ReactXP/Components/ActivityIndicator/ActivityIndicator.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B } from "../../../ReactXP/Components/ActivityIndicator/ActivityIndicator.TypeExtensions.fs.js";
import { format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { Make as Make_3 } from "../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../Text/Text.TypeExtensions.fs.js";
import { Actionable, Make as Make_4 } from "../../../../Components/Button/Button.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52 } from "../../Button/Button.TypeExtensions.fs.js";
import { ButtonHighLevelStateFactory_MakeLowLevel_3536472E } from "../../../../Input.fs.js";
import * as react from "react";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.TriStateful.Abstract";
        return Make((__currExplicitProps_5 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTriStateful_AbstractProps_Static_Z5186E7B4((tupledArg_1) => {
            let __list_5, __list_1, message, __list_4;
            const mode = tupledArg_1[0];
            const runAction = tupledArg_1[1];
            const reset = tupledArg_1[2];
            const children = (__list_5 = ofArray([RenderResult_Make_2B31D457(props.Content(runAction)), (mode.tag === 1) ? RenderResult_Make_6E3A73D((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list;
                const __sibI_1 = tupledArg_2[0] | 0;
                const __sibC_1 = tupledArg_2[1] | 0;
                const __pFQN_1 = tupledArg_2[2];
                const __parentFQN_2 = "ReactXP.Components.View";
                return Make_1((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = ("shield" + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                    let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1;
                    const __sibI_2 = tupledArg_3[0] | 0;
                    const __sibC_2 = tupledArg_3[1] | 0;
                    const __pFQN_2 = tupledArg_3[2];
                    const __parentFQN_3 = "ReactXP.Components.ActivityIndicator";
                    return Make_2((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B("#aaaaaa", "small"), (__currClass_1 = nthChildClasses(__sibI_2, __sibC_2), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ActivityIndicator", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))(empty_1());
                })), RenderResult_ToRawElements(__parentFQN_2, __list)));
            })), RenderResult_ToRawElements(__parentFQN_1, __list_1))) : ((mode.tag === 2) ? ((message = mode.fields[0], RenderResult_Make_6E3A73D((__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
                let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list_3;
                const __sibI_3 = tupledArg_4[0] | 0;
                const __sibC_3 = tupledArg_4[1] | 0;
                const __pFQN_3 = tupledArg_4[2];
                const __parentFQN_4 = "ReactXP.Components.View";
                return Make_1((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_2 = ("shield error" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list_3 = ofArray([RenderResult_Make_E25108F((tupledArg_5) => {
                    let text, __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                    const __sibI_4 = tupledArg_5[0] | 0;
                    const __sibC_4 = tupledArg_5[1] | 0;
                    const __pFQN_4 = tupledArg_5[2];
                    const __parentFQN_5 = "LibClient.Components.Text";
                    let arg10_3;
                    const __list_2 = singleton_1(RenderResult_Make_E25108F((text = format("{0}", message), (tupledArg_6) => makeTextNode(text, tupledArg_6[0], tupledArg_6[1], tupledArg_6[2]))));
                    arg10_3 = RenderResult_ToRawElements(__parentFQN_5, __list_2);
                    return Make_3((__currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_3 = ("message" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["__style", __currStyles_3]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))(arg10_3);
                }), RenderResult_Make_E25108F((tupledArg_7) => {
                    let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                    const __sibI_5 = tupledArg_7[0] | 0;
                    const __sibC_5 = tupledArg_7[1] | 0;
                    const __pFQN_5 = tupledArg_7[2];
                    const __parentFQN_6 = "LibClient.Components.Button";
                    return Make_4((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52("Ok", ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Actionable(reset))), (__currClass_4 = nthChildClasses(__sibI_5, __sibC_5), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))([]);
                })]), RenderResult_ToRawElements(__parentFQN_4, __list_3)));
            })), RenderResult_ToRawElements(__parentFQN_1, __list_4))))) : (new RenderResult(0)))]), RenderResult_ToRawElements(__parentFQN_1, __list_5));
            return react.createElement(react.Fragment, {}, ...children);
        }), (__currClass_5 = nthChildClasses(__sibI, __sibC), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))([]);
    })));
}

