import { map, defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToLeaves_410EF94B, RenderResult_Make_2B31D457, RenderResult, RenderResult_Make_Z1C694E5A, RenderResult_Make_410EF94B, RenderResult_ToRawElements, makeTextNode, RenderResult_Make_6E3A73D, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../RenderResult.fs.js";
import { Make } from "../../../../../ReactXP/Components/ScrollView/ScrollView.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5 } from "../../../../ReactXP/Components/ScrollView/ScrollView.TypeExtensions.fs.js";
import { Option_getOrElse } from "../../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { ofArray, ofSeq, isEmpty, append, singleton, empty } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { RulesBasic_width } from "../../../../../ReactXP/Styles/RulesBasic.fs.js";
import { processDynamicStyles } from "../../../../../ReactXP/Styles/Designtime.fs.js";
import { mapIndexed, empty as empty_1, singleton as singleton_1, append as append_1, delay, toList } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { Make as Make_1 } from "../../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { Make as Make_2 } from "../../../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../../../UiText/UiText.TypeExtensions.fs.js";
import { uncurry, equals } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Actions$1__Select } from "../../../../../Components/Input/PickerInternals/Popup/Popup.typext.fs.js";
import { format } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { SelectableValue$1__IsSelected_2B595 } from "../../../../../Input.fs.js";
import { Make as Make_3 } from "../../../../../Components/Icon/Icon.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B } from "../../../Icon/Icon.TypeExtensions.fs.js";
import { Icon_get_CheckMark } from "../../../../../Icons.fs.js";
import * as react from "react";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeAsyncDataProps_Static_379524B4 } from "../../../AsyncData/AsyncData.TypeExtensions.fs.js";
import { Make as Make_4 } from "../../../../../ReactXP/Components/ActivityIndicator/ActivityIndicator.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B } from "../../../../ReactXP/Components/ActivityIndicator/ActivityIndicator.TypeExtensions.fs.js";
import { Make as Make_5 } from "../../../../../Components/AsyncData/AsyncData.typext.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __dynamicStyles, __currStyles_1, __implicitProps, __list_19;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.ScrollView";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5(void 0, true), (__currClass = ("scroll-view" + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__dynamicStyles = Option_getOrElse(empty(), map((value_2) => singleton(RulesBasic_width(value_2 - 4)), estate.ModelState.MaybeFieldWidth)), (__currStyles_1 = append(__currStyles, processDynamicStyles(__dynamicStyles)), (__implicitProps = toList(delay(() => append_1((__currClass !== "") ? singleton_1(["ClassName", __currClass]) : empty_1(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton_1(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ScrollView", __currStyles_1)]) : empty_1()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))))((__list_19 = singleton(RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_2, __implicitProps_1, whenAvailable, __list_18;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make_1((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = ("view" + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append_1((__currClass_1 !== "") ? singleton_1(["ClassName", __currClass_1]) : empty_1(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton_1(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty_1()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((whenAvailable = ((items) => {
                let __list_11, __list_2, nonemptyItems, __list_10;
                const children = (__list_11 = singleton(isEmpty(items) ? RenderResult_Make_6E3A73D((__list_2 = singleton(RenderResult_Make_E25108F((tupledArg_2) => {
                    let __currExplicitProps_2, __currClass_2, __currStyles_3, __implicitProps_2, __list_1;
                    const __sibI_2 = tupledArg_2[0] | 0;
                    const __sibC_2 = tupledArg_2[1] | 0;
                    const __pFQN_2 = tupledArg_2[2];
                    const __parentFQN_3 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_2 = ("no-items-message" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append_1((__currClass_2 !== "") ? singleton_1(["ClassName", __currClass_2]) : empty_1(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton_1(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty_1()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list_1 = singleton(RenderResult_Make_E25108F((tupledArg_3) => {
                        let __currExplicitProps_3, __currClass_3, __currStyles_4, __implicitProps_3;
                        const __sibI_3 = tupledArg_3[0] | 0;
                        const __sibC_3 = tupledArg_3[1] | 0;
                        const __pFQN_3 = tupledArg_3[2];
                        const __parentFQN_4 = "LibClient.Components.UiText";
                        let arg10;
                        const __list = singleton(RenderResult_Make_E25108F((tupledArg_4) => makeTextNode("No items", tupledArg_4[0], tupledArg_4[1], tupledArg_4[2])));
                        arg10 = RenderResult_ToRawElements(__parentFQN_4, __list);
                        return Make_2((__currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_3 = ("no-items-message-text" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append_1((__currClass_3 !== "") ? singleton_1(["ClassName", __currClass_3]) : empty_1(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton_1(["__style", __currStyles_4]) : empty_1()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))(arg10);
                    })), RenderResult_ToRawElements(__parentFQN_3, __list_1)));
                })), RenderResult_ToRawElements(__parentFQN_2, __list_2))) : ((nonemptyItems = items, RenderResult_Make_6E3A73D((__list_10 = singleton(RenderResult_Make_410EF94B(ofSeq(mapIndexed((index, item) => {
                    let __list_9;
                    const isHighlighted = equals(index, estate.ModelState.MaybeHighlightedItemIndex);
                    return RenderResult_Make_Z1C694E5A((__list_9 = singleton(RenderResult_Make_E25108F((tupledArg_5) => {
                        let __currExplicitProps_4, __currClass_4, __currStyles_5, __implicitProps_4, __list_8;
                        const __sibI_4 = tupledArg_5[0] | 0;
                        const __sibC_4 = tupledArg_5[1] | 0;
                        const __pFQN_4 = tupledArg_5[2];
                        const __parentFQN_5 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_4 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (arg20) => {
                            Actions$1__Select(actions, index, item, arg20);
                        }), (__currClass_4 = (("item" + format(" {0} {1}", (index === 0) ? "first" : "", isHighlighted ? "highlighted" : "")) + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append_1((__currClass_4 !== "") ? singleton_1(["ClassName", __currClass_4]) : empty_1(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton_1(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_5)]) : empty_1()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))((__list_8 = ofArray([RenderResult_Make_E25108F((tupledArg_6) => {
                            let __currExplicitProps_5, __currClass_5, __currStyles_6, __implicitProps_5, __list_3;
                            const __sibI_5 = tupledArg_6[0] | 0;
                            const __sibC_5 = tupledArg_6[1] | 0;
                            const __pFQN_5 = tupledArg_6[2];
                            const __parentFQN_6 = "ReactXP.Components.View";
                            return Make_1((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_5 = ("item-selectedness" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append_1((__currClass_5 !== "") ? singleton_1(["ClassName", __currClass_5]) : empty_1(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton_1(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_6)]) : empty_1()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))((__list_3 = singleton(SelectableValue$1__IsSelected_2B595(estate.ModelState.Value, item) ? RenderResult_Make_E25108F((tupledArg_7) => {
                                let __currExplicitProps_6, __currClass_6, __currStyles_7, __implicitProps_6;
                                const __sibI_6 = tupledArg_7[0] | 0;
                                const __sibC_6 = tupledArg_7[1] | 0;
                                const __pFQN_6 = tupledArg_7[2];
                                const __parentFQN_7 = "LibClient.Components.Icon";
                                return Make_3((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, Icon_get_CheckMark())), (__currClass_6 = ("item-selected-icon" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append_1((__currClass_6 !== "") ? singleton_1(["ClassName", __currClass_6]) : empty_1(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton_1(["__style", __currStyles_7]) : empty_1()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))([]);
                            }) : (new RenderResult(0))), RenderResult_ToRawElements(__parentFQN_6, __list_3)));
                        }), RenderResult_Make_E25108F((tupledArg_8) => {
                            let __currExplicitProps_7, __currClass_7, __currStyles_8, __implicitProps_7, __list_7, matchValue, render_1, __list_6, toItemInfo, __list_5;
                            const __sibI_7 = tupledArg_8[0] | 0;
                            const __sibC_7 = tupledArg_8[1] | 0;
                            const __pFQN_7 = tupledArg_8[2];
                            const __parentFQN_8 = "ReactXP.Components.View";
                            return Make_1((__currExplicitProps_7 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_7 = ("item-body" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append_1((__currClass_7 !== "") ? singleton_1(["ClassName", __currClass_7]) : empty_1(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton_1(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty_1()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))((__list_7 = singleton((matchValue = props.ItemView, (matchValue.tag === 1) ? ((render_1 = matchValue.fields[0], RenderResult_Make_6E3A73D((__list_6 = singleton(RenderResult_Make_2B31D457(render_1(item))), RenderResult_ToRawElements(__parentFQN_8, __list_6))))) : ((toItemInfo = matchValue.fields[0], RenderResult_Make_6E3A73D((__list_5 = singleton(RenderResult_Make_E25108F((tupledArg_9) => {
                                let text_1, __currExplicitProps_8, __currClass_8, __currStyles_9, __implicitProps_8;
                                const __sibI_8 = tupledArg_9[0] | 0;
                                const __sibC_8 = tupledArg_9[1] | 0;
                                const __pFQN_8 = tupledArg_9[2];
                                const __parentFQN_9 = "LibClient.Components.UiText";
                                let arg10_7;
                                const __list_4 = singleton(RenderResult_Make_E25108F((text_1 = format("{0}", toItemInfo(item).Label), (tupledArg_10) => makeTextNode(text_1, tupledArg_10[0], tupledArg_10[1], tupledArg_10[2]))));
                                arg10_7 = RenderResult_ToRawElements(__parentFQN_9, __list_4);
                                return Make_2((__currExplicitProps_8 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_8 = (("item-label" + format(" {0}", isHighlighted ? "highlighted" : "")) + nthChildClasses(__sibI_8, __sibC_8)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append_1((__currClass_8 !== "") ? singleton_1(["ClassName", __currClass_8]) : empty_1(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton_1(["__style", __currStyles_9]) : empty_1()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))(arg10_7);
                            })), RenderResult_ToRawElements(__parentFQN_8, __list_5))))))), RenderResult_ToRawElements(__parentFQN_8, __list_7)));
                        })]), RenderResult_ToRawElements(__parentFQN_5, __list_8)));
                    })), RenderResult_ToLeaves_410EF94B(__list_9)));
                }, nonemptyItems)))), RenderResult_ToRawElements(__parentFQN_2, __list_10)))))), RenderResult_ToRawElements(__parentFQN_2, __list_11));
                return react.createElement(react.Fragment, {}, ...children);
            }), (__list_18 = singleton(RenderResult_Make_E25108F((tupledArg_11) => {
                const __sibI_9 = tupledArg_11[0] | 0;
                const __sibC_9 = tupledArg_11[1] | 0;
                const __pFQN_9 = tupledArg_11[2];
                const __parentFQN_10 = "LibClient.Components.AsyncData";
                let arg00_32;
                const __currExplicitProps_13 = LibClient_Components_TypeExtensions_TEs__TEs_MakeAsyncDataProps_Static_379524B4(estate.ModelState.SelectableItems, (items_1) => {
                    let __list_12;
                    const children_1 = (__list_12 = singleton(RenderResult_Make_2B31D457(whenAvailable(items_1))), RenderResult_ToRawElements(__parentFQN_10, __list_12));
                    return react.createElement(react.Fragment, {}, ...children_1);
                }, void 0, (maybeOldData) => {
                    let __list_17, oldData, __list_16, __list_14;
                    const children_2 = (__list_17 = singleton((maybeOldData != null) ? ((oldData = maybeOldData, RenderResult_Make_6E3A73D((__list_16 = ofArray([RenderResult_Make_2B31D457(whenAvailable(oldData)), RenderResult_Make_E25108F((tupledArg_14) => {
                        let __currExplicitProps_11, __currClass_11, __currStyles_12, __implicitProps_11, __list_15;
                        const __sibI_12 = tupledArg_14[0] | 0;
                        const __sibC_12 = tupledArg_14[1] | 0;
                        const __pFQN_12 = tupledArg_14[2];
                        const __parentFQN_13 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_11 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_11 = ("activity-indicator-overlay" + nthChildClasses(__sibI_12, __sibC_12)), (__currStyles_12 = findApplicableStyles(__mergedStyles, __currClass_11), (__implicitProps_11 = toList(delay(() => append_1((__currClass_11 !== "") ? singleton_1(["ClassName", __currClass_11]) : empty_1(), delay(() => ((!isEmpty(__currStyles_12)) ? singleton_1(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_12)]) : empty_1()))))), injectImplicitProps(__implicitProps_11, __currExplicitProps_11))))))((__list_15 = singleton(RenderResult_Make_E25108F((tupledArg_15) => {
                            let __currExplicitProps_12, __currClass_12, __currStyles_13, __implicitProps_12;
                            const __sibI_13 = tupledArg_15[0] | 0;
                            const __sibC_13 = tupledArg_15[1] | 0;
                            const __pFQN_13 = tupledArg_15[2];
                            const __parentFQN_14 = "ReactXP.Components.ActivityIndicator";
                            return Make_4((__currExplicitProps_12 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B("#aaaaaa", "medium"), (__currClass_12 = nthChildClasses(__sibI_13, __sibC_13), (__currStyles_13 = findApplicableStyles(__mergedStyles, __currClass_12), (__implicitProps_12 = toList(delay(() => append_1((__currClass_12 !== "") ? singleton_1(["ClassName", __currClass_12]) : empty_1(), delay(() => ((!isEmpty(__currStyles_13)) ? singleton_1(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ActivityIndicator", __currStyles_13)]) : empty_1()))))), injectImplicitProps(__implicitProps_12, __currExplicitProps_12))))))(empty());
                        })), RenderResult_ToRawElements(__parentFQN_13, __list_15)));
                    })]), RenderResult_ToRawElements(__parentFQN_10, __list_16))))) : RenderResult_Make_6E3A73D((__list_14 = singleton(RenderResult_Make_E25108F((tupledArg_12) => {
                        let __currExplicitProps_9, __currClass_9, __currStyles_10, __implicitProps_9, __list_13;
                        const __sibI_10 = tupledArg_12[0] | 0;
                        const __sibC_10 = tupledArg_12[1] | 0;
                        const __pFQN_10 = tupledArg_12[2];
                        const __parentFQN_11 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_9 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_9 = ("activity-indicator-block" + nthChildClasses(__sibI_10, __sibC_10)), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append_1((__currClass_9 !== "") ? singleton_1(["ClassName", __currClass_9]) : empty_1(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton_1(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_10)]) : empty_1()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))((__list_13 = singleton(RenderResult_Make_E25108F((tupledArg_13) => {
                            let __currExplicitProps_10, __currClass_10, __currStyles_11, __implicitProps_10;
                            const __sibI_11 = tupledArg_13[0] | 0;
                            const __sibC_11 = tupledArg_13[1] | 0;
                            const __pFQN_11 = tupledArg_13[2];
                            const __parentFQN_12 = "ReactXP.Components.ActivityIndicator";
                            return Make_4((__currExplicitProps_10 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B("#aaaaaa", "medium"), (__currClass_10 = nthChildClasses(__sibI_11, __sibC_11), (__currStyles_11 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append_1((__currClass_10 !== "") ? singleton_1(["ClassName", __currClass_10]) : empty_1(), delay(() => ((!isEmpty(__currStyles_11)) ? singleton_1(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ActivityIndicator", __currStyles_11)]) : empty_1()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))(empty());
                        })), RenderResult_ToRawElements(__parentFQN_11, __list_13)));
                    })), RenderResult_ToRawElements(__parentFQN_10, __list_14)))), RenderResult_ToRawElements(__parentFQN_10, __list_17));
                    return react.createElement(react.Fragment, {}, ...children_2);
                });
                const __currClass_13 = nthChildClasses(__sibI_9, __sibC_9);
                const __currStyles_14 = findApplicableStyles(__mergedStyles, __currClass_13);
                const __implicitProps_13 = toList(delay(() => append_1((__currClass_13 !== "") ? singleton_1(["ClassName", __currClass_13]) : empty_1(), delay(() => ((!isEmpty(__currStyles_14)) ? singleton_1(["__style", __currStyles_14]) : empty_1())))));
                arg00_32 = injectImplicitProps(__implicitProps_13, __currExplicitProps_13);
                return Make_5()(arg00_32)([]);
            })), RenderResult_ToRawElements(__parentFQN_2, __list_18))));
        })), RenderResult_ToRawElements(__parentFQN_1, __list_19)));
    })));
}

