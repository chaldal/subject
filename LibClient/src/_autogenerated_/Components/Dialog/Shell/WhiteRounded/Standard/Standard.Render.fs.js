import { map, defaultArg } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_2B31D457, RenderResult_ToRawElements, makeTextNode, RenderResult_Make_410EF94B, RenderResult, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../../RenderResult.fs.js";
import { Make } from "../../../../../../Components/With/ScreenSize/ScreenSize.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06 } from "../../../../With/ScreenSize/ScreenSize.TypeExtensions.fs.js";
import { Make as Make_1 } from "../../../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { empty as empty_1, ofArray, singleton as singleton_1, isEmpty } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Option_getOrElse } from "../../../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { format } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { ScreenSize__get_Class } from "../../../../../../Responsive.fs.js";
import { Make as Make_2 } from "../../../../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../../../../UiText/UiText.TypeExtensions.fs.js";
import { Make as Make_3 } from "../../../../../../ReactXP/Components/ScrollView/ScrollView.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5 } from "../../../../../ReactXP/Components/ScrollView/ScrollView.TypeExtensions.fs.js";
import { equals } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { noElement } from "../../../../../../EggShellReact.fs.js";
import { Mode } from "../../../../../../Components/Dialog/Shell/WhiteRounded/Standard/Standard.typext.fs.js";
import { Make as Make_4 } from "../../../../../../Components/Dialog/Shell/WhiteRounded/Raw/Raw.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_Shell_WhiteRounded_RawProps_Static_Z79AF04F8 } from "../Raw/Raw.TypeExtensions.fs.js";
import * as react from "react";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, void 0];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps_10, __currClass_10, __currStyles_10, __implicitProps_10;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.With.ScreenSize";
        return Make((__currExplicitProps_10 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06((screenSize) => {
            let __list_9;
            const children = (__list_9 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
                let __currExplicitProps, __currClass, __currStyles, __implicitProps;
                const __sibI_1 = tupledArg_1[0] | 0;
                const __sibC_1 = tupledArg_1[1] | 0;
                const __pFQN_1 = tupledArg_1[2];
                const __parentFQN_2 = "LibClient.Components.Dialog.Shell.WhiteRounded.Raw";
                let arg10_1;
                const __list_8 = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                    let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_7;
                    const __sibI_2 = tupledArg_2[0] | 0;
                    const __sibC_2 = tupledArg_2[1] | 0;
                    const __pFQN_2 = tupledArg_2[2];
                    const __parentFQN_3 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (e) => {
                        e.stopPropagation();
                    }), (__currClass_1 = ("dialog-contents" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_7 = ofArray([Option_getOrElse(new RenderResult(0), map((heading) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                        let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list_1;
                        const __sibI_3 = tupledArg_3[0] | 0;
                        const __sibC_3 = tupledArg_3[1] | 0;
                        const __pFQN_3 = tupledArg_3[2];
                        const __parentFQN_4 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_2 = (format("{0}{1}{2}", "heading ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
                            let text, __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                            const __sibI_4 = tupledArg_4[0] | 0;
                            const __sibC_4 = tupledArg_4[1] | 0;
                            const __pFQN_4 = tupledArg_4[2];
                            const __parentFQN_5 = "LibClient.Components.UiText";
                            let arg10_2;
                            const __list = singleton_1(RenderResult_Make_E25108F((text = format("{0}", heading), (tupledArg_5) => makeTextNode(text, tupledArg_5[0], tupledArg_5[1], tupledArg_5[2]))));
                            arg10_2 = RenderResult_ToRawElements(__parentFQN_5, __list);
                            return Make_2((__currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_3 = nthChildClasses(__sibI_4, __sibC_4), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["__style", __currStyles_3]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))(arg10_2);
                        })), RenderResult_ToRawElements(__parentFQN_4, __list_1)));
                    }))), props.Heading)), RenderResult_Make_E25108F((tupledArg_6) => {
                        let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4, __list_6, matchValue, message;
                        const __sibI_5 = tupledArg_6[0] | 0;
                        const __sibC_5 = tupledArg_6[1] | 0;
                        const __pFQN_5 = tupledArg_6[2];
                        const __parentFQN_6 = "ReactXP.Components.ScrollView";
                        return Make_3((__currExplicitProps_4 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5(void 0, true), (__currClass_4 = ("scroll-view" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ScrollView", __currStyles_4)]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))((__list_6 = ofArray([RenderResult_Make_E25108F((tupledArg_7) => {
                            let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5, __list_2;
                            const __sibI_6 = tupledArg_7[0] | 0;
                            const __sibC_6 = tupledArg_7[1] | 0;
                            const __pFQN_6 = tupledArg_7[2];
                            const __parentFQN_7 = "ReactXP.Components.View";
                            return Make_1((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_5 = ("body" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_5)]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))((__list_2 = singleton_1(RenderResult_Make_2B31D457(props.Body)), RenderResult_ToRawElements(__parentFQN_7, __list_2)));
                        }), Option_getOrElse(new RenderResult(0), map((message_1) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_8) => {
                            let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6, __list_4;
                            const __sibI_7 = tupledArg_8[0] | 0;
                            const __sibC_7 = tupledArg_8[1] | 0;
                            const __pFQN_7 = tupledArg_8[2];
                            const __parentFQN_8 = "ReactXP.Components.View";
                            return Make_1((__currExplicitProps_6 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_6 = ("error" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_6)]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))((__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_9) => {
                                let text_1, __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
                                const __sibI_8 = tupledArg_9[0] | 0;
                                const __sibC_8 = tupledArg_9[1] | 0;
                                const __pFQN_8 = tupledArg_9[2];
                                const __parentFQN_9 = "LibClient.Components.UiText";
                                let arg10_6;
                                const __list_3 = singleton_1(RenderResult_Make_E25108F((text_1 = format("{0}", message_1), (tupledArg_10) => makeTextNode(text_1, tupledArg_10[0], tupledArg_10[1], tupledArg_10[2]))));
                                arg10_6 = RenderResult_ToRawElements(__parentFQN_9, __list_3);
                                return Make_2((__currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_7 = nthChildClasses(__sibI_8, __sibC_8), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))(arg10_6);
                            })), RenderResult_ToRawElements(__parentFQN_8, __list_4)));
                        }))), (matchValue = props.Mode, (matchValue.tag === 2) ? ((message = matchValue.fields[0], message)) : (void 0)))), (!equals(props.Buttons, noElement)) ? RenderResult_Make_E25108F((tupledArg_11) => {
                            let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8, __list_5;
                            const __sibI_9 = tupledArg_11[0] | 0;
                            const __sibC_9 = tupledArg_11[1] | 0;
                            const __pFQN_9 = tupledArg_11[2];
                            const __parentFQN_10 = "ReactXP.Components.View";
                            return Make_1((__currExplicitProps_8 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_8 = ("buttons" + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))((__list_5 = singleton_1(RenderResult_Make_2B31D457(props.Buttons)), RenderResult_ToRawElements(__parentFQN_10, __list_5)));
                        }) : (new RenderResult(0))]), RenderResult_ToRawElements(__parentFQN_6, __list_6)));
                    }), equals(props.Mode, new Mode(1)) ? RenderResult_Make_E25108F((tupledArg_12) => {
                        let __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9;
                        const __sibI_10 = tupledArg_12[0] | 0;
                        const __sibC_10 = tupledArg_12[1] | 0;
                        const __pFQN_10 = tupledArg_12[2];
                        const __parentFQN_11 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_9 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_9 = ("in-progress" + nthChildClasses(__sibI_10, __sibC_10)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_9)]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))(empty_1());
                    }) : (new RenderResult(0))]), RenderResult_ToRawElements(__parentFQN_3, __list_7)));
                }));
                arg10_1 = RenderResult_ToRawElements(__parentFQN_2, __list_8);
                return Make_4((__currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_Shell_WhiteRounded_RawProps_Static_Z79AF04F8(props.CanClose), (__currClass = nthChildClasses(__sibI_1, __sibC_1), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))(arg10_1);
            })), RenderResult_ToRawElements(__parentFQN_1, __list_9));
            return react.createElement(react.Fragment, {}, ...children);
        }), (__currClass_10 = nthChildClasses(__sibI, __sibC), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append((__currClass_10 !== "") ? singleton(["ClassName", __currClass_10]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["__style", __currStyles_10]) : empty()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))([]);
    })));
}

