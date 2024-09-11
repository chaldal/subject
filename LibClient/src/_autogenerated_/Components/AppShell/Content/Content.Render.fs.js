import { some, map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_6E3A73D, RenderResult, RenderResult_ToRawElements, RenderResult_Make_2B31D457, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../Components/ErrorBoundary/ErrorBoundary.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeErrorBoundaryProps_Static_Z5775E1D } from "../../ErrorBoundary/ErrorBoundary.TypeExtensions.fs.js";
import { Make as Make_1 } from "../../../../Components/Responsive/Responsive.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeResponsiveProps_Static_502D6660 } from "../../Responsive/Responsive.TypeExtensions.fs.js";
import { Make as Make_2 } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { append as append_1, ofArray, empty as empty_1, singleton as singleton_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { equals } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { noElement } from "../../../../EggShellReact.fs.js";
import { Make as Make_3 } from "../../../../Components/Popup/Popup.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakePopupProps_Static_Z200491B8 } from "../../Popup/Popup.TypeExtensions.fs.js";
import * as react from "react";
import { Make as Make_4 } from "../../../../Components/With/Layout/Layout.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_LayoutProps_Static_Z6CC90BB3 } from "../../With/Layout/Layout.TypeExtensions.fs.js";
import { Make as Make_5 } from "../../../../Components/Scrim/Scrim.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeScrimProps_Static_21752E0A } from "../../Scrim/Scrim.TypeExtensions.fs.js";
import { Actions__get_Bound, Actions__OnSidebarDraggableChange_Z26024D6E, setSidebarVisibility } from "../../../../Components/AppShell/Content/Content.typext.fs.js";
import { Option_getOrElse } from "../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { Make as Make_6 } from "../../../../Components/Draggable/Draggable.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeDraggableProps_Static_Z2F4A4793 } from "../../Draggable/Draggable.TypeExtensions.fs.js";
import { op_UnaryNegation_Int32 } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Int32.js";
import { RulesBasic_width } from "../../../../ReactXP/Styles/RulesBasic.fs.js";
import { processDynamicStyles } from "../../../../ReactXP/Styles/Designtime.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps_22, children_4, __list_21, __currClass_22, __currStyles_23, __implicitProps_22;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.ErrorBoundary";
        return Make((__currExplicitProps_22 = LibClient_Components_TypeExtensions_TEs__TEs_MakeErrorBoundaryProps_Static_Z5775E1D((children_4 = ((__list_21 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_21, __currClass_21, __currStyles_22, __implicitProps_21;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "LibClient.Components.Responsive";
            return Make_1((__currExplicitProps_21 = LibClient_Components_TypeExtensions_TEs__TEs_MakeResponsiveProps_Static_502D6660((_arg2) => {
                let __list_12;
                const children_1 = (__list_12 = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                    let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_11;
                    const __sibI_2 = tupledArg_2[0] | 0;
                    const __sibC_2 = tupledArg_2[1] | 0;
                    const __pFQN_2 = tupledArg_2[2];
                    const __parentFQN_3 = "ReactXP.Components.View";
                    return Make_2((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, true), (__currClass = ("safe-insets-view" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_11 = ofArray([RenderResult_Make_E25108F((tupledArg_3) => {
                        let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_10, __list_9, __list_4;
                        const __sibI_3 = tupledArg_3[0] | 0;
                        const __sibC_3 = tupledArg_3[1] | 0;
                        const __pFQN_3 = tupledArg_3[2];
                        const __parentFQN_4 = "ReactXP.Components.View";
                        return Make_2((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = ("view" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_10 = ofArray([(!equals(props.TopNav, noElement)) ? RenderResult_Make_E25108F((tupledArg_4) => {
                            let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list;
                            const __sibI_4 = tupledArg_4[0] | 0;
                            const __sibC_4 = tupledArg_4[1] | 0;
                            const __pFQN_4 = tupledArg_4[2];
                            const __parentFQN_5 = "ReactXP.Components.View";
                            return Make_2((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_2 = ("top-nav-block" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list = singleton_1(RenderResult_Make_2B31D457(props.TopNav)), RenderResult_ToRawElements(__parentFQN_5, __list)));
                        }) : (new RenderResult(0)), (props.DesktopSidebarStyle.tag === 1) ? RenderResult_Make_6E3A73D((__list_9 = ofArray([RenderResult_Make_E25108F((tupledArg_9) => {
                            let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7, __list_6;
                            const __sibI_9 = tupledArg_9[0] | 0;
                            const __sibC_9 = tupledArg_9[1] | 0;
                            const __pFQN_9 = tupledArg_9[2];
                            const __parentFQN_10 = "ReactXP.Components.View";
                            return Make_2((__currExplicitProps_7 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_7 = ("sidebar-and-content-block" + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_7)]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))((__list_6 = ofArray([RenderResult_Make_E25108F((tupledArg_10) => {
                                let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8, __list_5;
                                const __sibI_10 = tupledArg_10[0] | 0;
                                const __sibC_10 = tupledArg_10[1] | 0;
                                const __pFQN_10 = tupledArg_10[2];
                                const __parentFQN_11 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_8 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_8 = ("content-block" + nthChildClasses(__sibI_10, __sibC_10)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))((__list_5 = singleton_1(RenderResult_Make_2B31D457(props.Content)), RenderResult_ToRawElements(__parentFQN_11, __list_5)));
                            }), RenderResult_Make_E25108F((tupledArg_11) => {
                                let __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9;
                                const __sibI_11 = tupledArg_11[0] | 0;
                                const __sibC_11 = tupledArg_11[1] | 0;
                                const __pFQN_11 = tupledArg_11[2];
                                const __parentFQN_12 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_9 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_9 = ("top-nav-shadow" + nthChildClasses(__sibI_11, __sibC_11)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_9)]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))(empty_1());
                            })]), RenderResult_ToRawElements(__parentFQN_10, __list_6)));
                        }), RenderResult_Make_E25108F((tupledArg_12) => {
                            let __currExplicitProps_11, __currClass_11, __currStyles_11, __implicitProps_11;
                            const __sibI_12 = tupledArg_12[0] | 0;
                            const __sibC_12 = tupledArg_12[1] | 0;
                            const __pFQN_12 = tupledArg_12[2];
                            const __parentFQN_13 = "LibClient.Components.Popup";
                            return Make_3((__currExplicitProps_11 = LibClient_Components_TypeExtensions_TEs__TEs_MakePopupProps_Static_Z200491B8(() => {
                                let __list_8;
                                const children = (__list_8 = singleton_1(RenderResult_Make_E25108F((tupledArg_13) => {
                                    let __currExplicitProps_10, __currClass_10, __currStyles_10, __implicitProps_10, __list_7;
                                    const __sibI_13 = tupledArg_13[0] | 0;
                                    const __sibC_13 = tupledArg_13[1] | 0;
                                    const __pFQN_13 = tupledArg_13[2];
                                    const __parentFQN_14 = "ReactXP.Components.View";
                                    return Make_2((__currExplicitProps_10 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_10 = ("sidebar-popup-wrapper" + nthChildClasses(__sibI_13, __sibC_13)), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append((__currClass_10 !== "") ? singleton(["ClassName", __currClass_10]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_10)]) : empty()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))((__list_7 = singleton_1(RenderResult_Make_2B31D457(props.Sidebar)), RenderResult_ToRawElements(__parentFQN_14, __list_7)));
                                })), RenderResult_ToRawElements(__parentFQN_13, __list_8));
                                return react.createElement(react.Fragment, {}, ...children);
                            }, estate.SidebarPopupConnector), (__currClass_11 = nthChildClasses(__sibI_12, __sibC_12), (__currStyles_11 = findApplicableStyles(__mergedStyles, __currClass_11), (__implicitProps_11 = toList(delay(() => append((__currClass_11 !== "") ? singleton(["ClassName", __currClass_11]) : empty(), delay(() => ((!isEmpty(__currStyles_11)) ? singleton(["__style", __currStyles_11]) : empty()))))), injectImplicitProps(__implicitProps_11, __currExplicitProps_11))))))([]);
                        })]), RenderResult_ToRawElements(__parentFQN_4, __list_9))) : RenderResult_Make_6E3A73D((__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_5) => {
                            let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3, __list_3;
                            const __sibI_5 = tupledArg_5[0] | 0;
                            const __sibC_5 = tupledArg_5[1] | 0;
                            const __pFQN_5 = tupledArg_5[2];
                            const __parentFQN_6 = "ReactXP.Components.View";
                            return Make_2((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("sidebar-and-content-block" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))((__list_3 = ofArray([RenderResult_Make_E25108F((tupledArg_6) => {
                                let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4, __list_1;
                                const __sibI_6 = tupledArg_6[0] | 0;
                                const __sibC_6 = tupledArg_6[1] | 0;
                                const __pFQN_6 = tupledArg_6[2];
                                const __parentFQN_7 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_4 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_4 = ("sidebar-block desktop" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_4)]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))((__list_1 = singleton_1(RenderResult_Make_2B31D457(props.Sidebar)), RenderResult_ToRawElements(__parentFQN_7, __list_1)));
                            }), RenderResult_Make_E25108F((tupledArg_7) => {
                                let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5, __list_2;
                                const __sibI_7 = tupledArg_7[0] | 0;
                                const __sibC_7 = tupledArg_7[1] | 0;
                                const __pFQN_7 = tupledArg_7[2];
                                const __parentFQN_8 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_5 = ("content-block" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_5)]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))((__list_2 = singleton_1(RenderResult_Make_2B31D457(props.Content)), RenderResult_ToRawElements(__parentFQN_8, __list_2)));
                            }), RenderResult_Make_E25108F((tupledArg_8) => {
                                let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                                const __sibI_8 = tupledArg_8[0] | 0;
                                const __sibC_8 = tupledArg_8[1] | 0;
                                const __pFQN_8 = tupledArg_8[2];
                                const __parentFQN_9 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_6 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_6 = ("top-nav-shadow" + nthChildClasses(__sibI_8, __sibC_8)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_6)]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))(empty_1());
                            })]), RenderResult_ToRawElements(__parentFQN_6, __list_3)));
                        })), RenderResult_ToRawElements(__parentFQN_4, __list_4)))]), RenderResult_ToRawElements(__parentFQN_4, __list_10)));
                    }), RenderResult_Make_2B31D457(props.Dialogs)]), RenderResult_ToRawElements(__parentFQN_3, __list_11)));
                })), RenderResult_ToRawElements(__parentFQN_2, __list_12));
                return react.createElement(react.Fragment, {}, ...children_1);
            }, (_arg3) => {
                let __list_20;
                const children_3 = (__list_20 = singleton_1(RenderResult_Make_E25108F((tupledArg_14) => {
                    let __currExplicitProps_12, __currClass_12, __currStyles_12, __implicitProps_12, __list_19;
                    const __sibI_14 = tupledArg_14[0] | 0;
                    const __sibC_14 = tupledArg_14[1] | 0;
                    const __pFQN_14 = tupledArg_14[2];
                    const __parentFQN_15 = "ReactXP.Components.View";
                    return Make_2((__currExplicitProps_12 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, true), (__currClass_12 = ("safe-insets-view" + nthChildClasses(__sibI_14, __sibC_14)), (__currStyles_12 = findApplicableStyles(__mergedStyles, __currClass_12), (__implicitProps_12 = toList(delay(() => append((__currClass_12 !== "") ? singleton(["ClassName", __currClass_12]) : empty(), delay(() => ((!isEmpty(__currStyles_12)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_12)]) : empty()))))), injectImplicitProps(__implicitProps_12, __currExplicitProps_12))))))((__list_19 = singleton_1(RenderResult_Make_E25108F((tupledArg_15) => {
                        let __currExplicitProps_13, __currClass_13, __currStyles_13, __implicitProps_13, __list_18;
                        const __sibI_15 = tupledArg_15[0] | 0;
                        const __sibC_15 = tupledArg_15[1] | 0;
                        const __pFQN_15 = tupledArg_15[2];
                        const __parentFQN_16 = "ReactXP.Components.View";
                        return Make_2((__currExplicitProps_13 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_13 = ("view" + nthChildClasses(__sibI_15, __sibC_15)), (__currStyles_13 = findApplicableStyles(__mergedStyles, __currClass_13), (__implicitProps_13 = toList(delay(() => append((__currClass_13 !== "") ? singleton(["ClassName", __currClass_13]) : empty(), delay(() => ((!isEmpty(__currStyles_13)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_13)]) : empty()))))), injectImplicitProps(__implicitProps_13, __currExplicitProps_13))))))((__list_18 = ofArray([(!equals(props.TopNav, noElement)) ? RenderResult_Make_E25108F((tupledArg_16) => {
                            let __currExplicitProps_14, __currClass_14, __currStyles_14, __implicitProps_14, __list_13;
                            const __sibI_16 = tupledArg_16[0] | 0;
                            const __sibC_16 = tupledArg_16[1] | 0;
                            const __pFQN_16 = tupledArg_16[2];
                            const __parentFQN_17 = "ReactXP.Components.View";
                            return Make_2((__currExplicitProps_14 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_14 = ("top-nav-block" + nthChildClasses(__sibI_16, __sibC_16)), (__currStyles_14 = findApplicableStyles(__mergedStyles, __currClass_14), (__implicitProps_14 = toList(delay(() => append((__currClass_14 !== "") ? singleton(["ClassName", __currClass_14]) : empty(), delay(() => ((!isEmpty(__currStyles_14)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_14)]) : empty()))))), injectImplicitProps(__implicitProps_14, __currExplicitProps_14))))))((__list_13 = singleton_1(RenderResult_Make_2B31D457(props.TopNav)), RenderResult_ToRawElements(__parentFQN_17, __list_13)));
                        }) : (new RenderResult(0)), RenderResult_Make_E25108F((tupledArg_17) => {
                            let __currExplicitProps_15, __currClass_15, __currStyles_15, __implicitProps_15, __list_14;
                            const __sibI_17 = tupledArg_17[0] | 0;
                            const __sibC_17 = tupledArg_17[1] | 0;
                            const __pFQN_17 = tupledArg_17[2];
                            const __parentFQN_18 = "ReactXP.Components.View";
                            return Make_2((__currExplicitProps_15 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_15 = ("content-block" + nthChildClasses(__sibI_17, __sibC_17)), (__currStyles_15 = findApplicableStyles(__mergedStyles, __currClass_15), (__implicitProps_15 = toList(delay(() => append((__currClass_15 !== "") ? singleton(["ClassName", __currClass_15]) : empty(), delay(() => ((!isEmpty(__currStyles_15)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_15)]) : empty()))))), injectImplicitProps(__implicitProps_15, __currExplicitProps_15))))))((__list_14 = ofArray([RenderResult_Make_2B31D457(props.Content), RenderResult_Make_E25108F((tupledArg_18) => {
                                let __currExplicitProps_16, __currClass_16, __currStyles_16, __implicitProps_16;
                                const __sibI_18 = tupledArg_18[0] | 0;
                                const __sibC_18 = tupledArg_18[1] | 0;
                                const __pFQN_18 = tupledArg_18[2];
                                const __parentFQN_19 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_16 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_16 = ("top-nav-shadow" + nthChildClasses(__sibI_18, __sibC_18)), (__currStyles_16 = findApplicableStyles(__mergedStyles, __currClass_16), (__implicitProps_16 = toList(delay(() => append((__currClass_16 !== "") ? singleton(["ClassName", __currClass_16]) : empty(), delay(() => ((!isEmpty(__currStyles_16)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_16)]) : empty()))))), injectImplicitProps(__implicitProps_16, __currExplicitProps_16))))))(empty_1());
                            })]), RenderResult_ToRawElements(__parentFQN_18, __list_14)));
                        }), RenderResult_Make_E25108F((tupledArg_19) => {
                            let __currExplicitProps_20, __currClass_20, __currStyles_21, __implicitProps_20;
                            const __sibI_19 = tupledArg_19[0] | 0;
                            const __sibC_19 = tupledArg_19[1] | 0;
                            const __pFQN_19 = tupledArg_19[2];
                            const __parentFQN_20 = "LibClient.Components.With.Layout";
                            return Make_4((__currExplicitProps_20 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_LayoutProps_Static_Z6CC90BB3((tupledArg_20) => {
                                let __list_17, width;
                                const onLayoutOption = tupledArg_20[0];
                                const maybeLayout = tupledArg_20[1];
                                const children_2 = (__list_17 = ofArray([RenderResult_Make_E25108F((tupledArg_21) => {
                                    let __currExplicitProps_17, __currClass_17, __currStyles_17, __implicitProps_17;
                                    const __sibI_20 = tupledArg_21[0] | 0;
                                    const __sibC_20 = tupledArg_21[1] | 0;
                                    const __pFQN_20 = tupledArg_21[2];
                                    const __parentFQN_21 = "LibClient.Components.Scrim";
                                    return Make_5((__currExplicitProps_17 = LibClient_Components_TypeExtensions_TEs__TEs_MakeScrimProps_Static_21752E0A(estate.IsSidebarScrimVisible, (e) => {
                                        setSidebarVisibility(false, e);
                                    }, void 0, void 0, void 0, void 0, void 0, some(map((draggable) => ((arg00_45) => {
                                        draggable.OnPanHorizontal(arg00_45);
                                    }), estate.MaybeSidebarDraggable))), (__currClass_17 = ("scrim" + nthChildClasses(__sibI_20, __sibC_20)), (__currStyles_17 = findApplicableStyles(__mergedStyles, __currClass_17), (__implicitProps_17 = toList(delay(() => append((__currClass_17 !== "") ? singleton(["ClassName", __currClass_17]) : empty(), delay(() => ((!isEmpty(__currStyles_17)) ? singleton(["__style", __currStyles_17]) : empty()))))), injectImplicitProps(__implicitProps_17, __currExplicitProps_17))))))([]);
                                }), (width = (Option_getOrElse(300, map((l) => l.Width, maybeLayout)) | 0), RenderResult_Make_E25108F((tupledArg_22) => {
                                    let __currExplicitProps_18, __currClass_18, __currStyles_18, __dynamicStyles, __currStyles_19, __implicitProps_18;
                                    const __sibI_21 = tupledArg_22[0] | 0;
                                    const __sibC_21 = tupledArg_22[1] | 0;
                                    const __pFQN_21 = tupledArg_22[2];
                                    const __parentFQN_22 = "LibClient.Components.Draggable";
                                    let arg10_20;
                                    const __list_16 = singleton_1(RenderResult_Make_E25108F((tupledArg_23) => {
                                        let __currExplicitProps_19, __currClass_19, __currStyles_20, __implicitProps_19, __list_15;
                                        const __sibI_22 = tupledArg_23[0] | 0;
                                        const __sibC_22 = tupledArg_23[1] | 0;
                                        const __pFQN_22 = tupledArg_23[2];
                                        const __parentFQN_23 = "ReactXP.Components.View";
                                        return Make_2((__currExplicitProps_19 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, some(onLayoutOption)), (__currClass_19 = ("sidebar-wrapper" + nthChildClasses(__sibI_22, __sibC_22)), (__currStyles_20 = findApplicableStyles(__mergedStyles, __currClass_19), (__implicitProps_19 = toList(delay(() => append((__currClass_19 !== "") ? singleton(["ClassName", __currClass_19]) : empty(), delay(() => ((!isEmpty(__currStyles_20)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_20)]) : empty()))))), injectImplicitProps(__implicitProps_19, __currExplicitProps_19))))))((__list_15 = singleton_1(RenderResult_Make_2B31D457(props.Sidebar)), RenderResult_ToRawElements(__parentFQN_23, __list_15)));
                                    }));
                                    arg10_20 = RenderResult_ToRawElements(__parentFQN_22, __list_16);
                                    return Make_6((__currExplicitProps_18 = LibClient_Components_TypeExtensions_TEs__TEs_MakeDraggableProps_Static_Z2F4A4793([op_UnaryNegation_Int32(width) + 10, 0], void 0, void 0, {
                                        BackwardThreshold: 50,
                                        ForwardThreshold: 30,
                                        Offset: width - 10,
                                    }, void 0, void 0, (arg00_48) => {
                                        Actions__OnSidebarDraggableChange_Z26024D6E(actions, arg00_48);
                                    }, void 0, Actions__get_Bound(actions).RefSidebarDraggable), (__currClass_18 = ("sidebar-draggable" + nthChildClasses(__sibI_21, __sibC_21)), (__currStyles_18 = findApplicableStyles(__mergedStyles, __currClass_18), (__dynamicStyles = singleton_1(RulesBasic_width(width)), (__currStyles_19 = append_1(__currStyles_18, processDynamicStyles(__dynamicStyles)), (__implicitProps_18 = toList(delay(() => append((__currClass_18 !== "") ? singleton(["ClassName", __currClass_18]) : empty(), delay(() => ((!isEmpty(__currStyles_19)) ? singleton(["__style", __currStyles_19]) : empty()))))), injectImplicitProps(__implicitProps_18, __currExplicitProps_18))))))))(arg10_20);
                                }))]), RenderResult_ToRawElements(__parentFQN_20, __list_17));
                                return react.createElement(react.Fragment, {}, ...children_2);
                            }), (__currClass_20 = nthChildClasses(__sibI_19, __sibC_19), (__currStyles_21 = findApplicableStyles(__mergedStyles, __currClass_20), (__implicitProps_20 = toList(delay(() => append((__currClass_20 !== "") ? singleton(["ClassName", __currClass_20]) : empty(), delay(() => ((!isEmpty(__currStyles_21)) ? singleton(["__style", __currStyles_21]) : empty()))))), injectImplicitProps(__implicitProps_20, __currExplicitProps_20))))))([]);
                        }), RenderResult_Make_2B31D457(props.Dialogs)]), RenderResult_ToRawElements(__parentFQN_16, __list_18)));
                    })), RenderResult_ToRawElements(__parentFQN_15, __list_19)));
                })), RenderResult_ToRawElements(__parentFQN_2, __list_20));
                return react.createElement(react.Fragment, {}, ...children_3);
            }), (__currClass_21 = nthChildClasses(__sibI_1, __sibC_1), (__currStyles_22 = findApplicableStyles(__mergedStyles, __currClass_21), (__implicitProps_21 = toList(delay(() => append((__currClass_21 !== "") ? singleton(["ClassName", __currClass_21]) : empty(), delay(() => ((!isEmpty(__currStyles_22)) ? singleton(["__style", __currStyles_22]) : empty()))))), injectImplicitProps(__implicitProps_21, __currExplicitProps_21))))))([]);
        })), RenderResult_ToRawElements(__parentFQN_1, __list_21))), react.createElement(react.Fragment, {}, ...children_4)), props.OnError), (__currClass_22 = nthChildClasses(__sibI, __sibC), (__currStyles_23 = findApplicableStyles(__mergedStyles, __currClass_22), (__implicitProps_22 = toList(delay(() => append((__currClass_22 !== "") ? singleton(["ClassName", __currClass_22]) : empty(), delay(() => ((!isEmpty(__currStyles_23)) ? singleton(["__style", __currStyles_23]) : empty()))))), injectImplicitProps(__implicitProps_22, __currExplicitProps_22))))))([]);
    })));
}

