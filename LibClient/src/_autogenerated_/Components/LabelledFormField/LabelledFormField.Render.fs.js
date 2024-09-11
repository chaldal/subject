import { defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_2B31D457, RenderResult_ToRawElements, makeTextNode, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../RenderResult.fs.js";
import { Make } from "../../../Components/With/ScreenSize/ScreenSize.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06 } from "../With/ScreenSize/ScreenSize.TypeExtensions.fs.js";
import { Make as Make_1 } from "../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { format } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { ScreenSize__get_Class } from "../../../Responsive.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, singleton as singleton_1, isEmpty } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_2 } from "../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../UiText/UiText.TypeExtensions.fs.js";
import * as react from "react";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, void 0];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.With.ScreenSize";
        return Make((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06((screenSize) => {
            let __list_4;
            const children = (__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
                let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_3;
                const __sibI_1 = tupledArg_1[0] | 0;
                const __sibC_1 = tupledArg_1[1] | 0;
                const __pFQN_1 = tupledArg_1[2];
                const __parentFQN_2 = "ReactXP.Components.View";
                return Make_1((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = (format("{0}{1}{2}", "view ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_3 = ofArray([RenderResult_Make_E25108F((tupledArg_2) => {
                    let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_1;
                    const __sibI_2 = tupledArg_2[0] | 0;
                    const __sibC_2 = tupledArg_2[1] | 0;
                    const __pFQN_2 = tupledArg_2[2];
                    const __parentFQN_3 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = (format("{0}{1}{2}", "label ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                        let text, __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
                        const __sibI_3 = tupledArg_3[0] | 0;
                        const __sibC_3 = tupledArg_3[1] | 0;
                        const __pFQN_3 = tupledArg_3[2];
                        const __parentFQN_4 = "LibClient.Components.UiText";
                        let arg10_1;
                        const __list = singleton_1(RenderResult_Make_E25108F((text = format("{0}", props.Label), (tupledArg_4) => makeTextNode(text, tupledArg_4[0], tupledArg_4[1], tupledArg_4[2]))));
                        arg10_1 = RenderResult_ToRawElements(__parentFQN_4, __list);
                        return Make_2((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_2 = nthChildClasses(__sibI_3, __sibC_3), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))(arg10_1);
                    })), RenderResult_ToRawElements(__parentFQN_3, __list_1)));
                }), RenderResult_Make_E25108F((tupledArg_5) => {
                    let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3, __list_2;
                    const __sibI_4 = tupledArg_5[0] | 0;
                    const __sibC_4 = tupledArg_5[1] | 0;
                    const __pFQN_4 = tupledArg_5[2];
                    const __parentFQN_5 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = (format("{0}{1}{2}", "field ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))((__list_2 = singleton_1(RenderResult_Make_2B31D457(props.children)), RenderResult_ToRawElements(__parentFQN_5, __list_2)));
                })]), RenderResult_ToRawElements(__parentFQN_2, __list_3)));
            })), RenderResult_ToRawElements(__parentFQN_1, __list_4));
            return react.createElement(react.Fragment, {}, ...children);
        }), (__currClass_4 = nthChildClasses(__sibI, __sibC), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))([]);
    })));
}

