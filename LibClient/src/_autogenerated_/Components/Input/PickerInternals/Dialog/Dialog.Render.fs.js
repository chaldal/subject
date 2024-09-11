import { defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../../ReactXP/Styles/Runtime.fs.js";
import { makeTextNode, RenderResult_Make_2B31D457, RenderResult_Make_6E3A73D, RenderResult, RenderResult_Make_410EF94B, RenderResult_ToRawElements, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../RenderResult.fs.js";
import { Make } from "../../../../../Components/With/ScreenSize/ScreenSize.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06 } from "../../../With/ScreenSize/ScreenSize.TypeExtensions.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_RefProps_Static_ZEBF9CCE } from "../../../With/Ref/Ref.TypeExtensions.fs.js";
import { Make as Make_1 } from "../../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { mapIndexed, empty, singleton, append, delay, toList } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, ofSeq, singleton as singleton_1, empty as empty_1, isEmpty } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_2 } from "../../../../../ReactXP/Components/TextInput/TextInput.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeTextInputProps_Static_5931E0E0 } from "../../../../ReactXP/Components/TextInput/TextInput.TypeExtensions.fs.js";
import { NonemptyStringModule_ofString, NonemptyStringModule_optionToString } from "../../../../../../../LibLangFsharp/src/NonemptyString.fs.js";
import { Actions$1__TryCancel_411CDB90, Actions$1__Toggle, Actions$1__OnQueryChange_Z2972C1DC } from "../../../../../Components/Input/PickerInternals/Dialog/Dialog.typext.fs.js";
import { uncurry } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { format } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { ScreenSize__get_Class } from "../../../../../Responsive.fs.js";
import * as react from "react";
import { Make as Make_3 } from "../../../../../Components/With/Ref/Ref.typext.fs.js";
import { Make as Make_4 } from "../../../../../ReactXP/Components/ScrollView/ScrollView.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5 } from "../../../../ReactXP/Components/ScrollView/ScrollView.TypeExtensions.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeItemListProps_Static_55018457 } from "../../../ItemList/ItemList.TypeExtensions.fs.js";
import { ButtonHighLevelStateFactory_MakeLowLevel_3536472E, SelectableValue$1__get_CanSelectMultiple, SelectableValue$1__IsSelected_2B595 } from "../../../../../Input.fs.js";
import { Make as Make_5 } from "../../../../../Components/Icon/Icon.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B } from "../../../Icon/Icon.TypeExtensions.fs.js";
import { Icon_get_CheckMark } from "../../../../../Icons.fs.js";
import { Make as Make_6 } from "../../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../../Text/Text.TypeExtensions.fs.js";
import { Make as Make_7 } from "../../../../../Components/TapCapture/TapCapture.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6 } from "../../../TapCapture/TapCapture.TypeExtensions.fs.js";
import { Make as Make_8, Style } from "../../../../../Components/ItemList/ItemList.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeAsyncDataProps_Static_379524B4 } from "../../../AsyncData/AsyncData.TypeExtensions.fs.js";
import { Make as Make_9 } from "../../../../../ReactXP/Components/ActivityIndicator/ActivityIndicator.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B } from "../../../../ReactXP/Components/ActivityIndicator/ActivityIndicator.TypeExtensions.fs.js";
import { Make as Make_10 } from "../../../../../Components/AsyncData/AsyncData.typext.fs.js";
import { Actionable, Make as Make_11 } from "../../../../../Components/Button/Button.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52 } from "../../../Button/Button.TypeExtensions.fs.js";
import { Make as Make_12 } from "../../../../../Components/Buttons/Buttons.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonsProps_Static_519A9F32 } from "../../../Buttons/Buttons.TypeExtensions.fs.js";
import { DialogPosition, OnBackground, OnEscape, When, Make as Make_13 } from "../../../../../Components/Dialog/Shell/WhiteRounded/Raw/Raw.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_Shell_WhiteRounded_RawProps_Static_Z79AF04F8 } from "../../../Dialog/Shell/WhiteRounded/Raw/Raw.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, pstate, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps_19, __currClass_19, __currStyles_19, __implicitProps_19;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.With.ScreenSize";
        return Make((__currExplicitProps_19 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06((screenSize) => {
            let __list_19;
            const children_5 = (__list_19 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
                let __currExplicitProps, __currClass, __currStyles, __implicitProps;
                const __sibI_1 = tupledArg_1[0] | 0;
                const __sibC_1 = tupledArg_1[1] | 0;
                const __pFQN_1 = tupledArg_1[2];
                const __parentFQN_2 = "LibClient.Components.Dialog.Shell.WhiteRounded.Raw";
                let arg10_1;
                const __list_18 = ofArray([RenderResult_Make_E25108F((tupledArg_2) => {
                    const __sibI_2 = tupledArg_2[0] | 0;
                    const __sibC_2 = tupledArg_2[1] | 0;
                    const __pFQN_2 = tupledArg_2[2];
                    const __parentFQN_3 = "LibClient.Components.With.Ref";
                    let arg00_3;
                    const __currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_RefProps_Static_ZEBF9CCE((tupledArg_3) => {
                        let __list_1;
                        const bindInput = tupledArg_3[0];
                        const children = (__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
                            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list;
                            const __sibI_3 = tupledArg_4[0] | 0;
                            const __sibC_3 = tupledArg_4[1] | 0;
                            const __pFQN_3 = tupledArg_4[2];
                            const __parentFQN_4 = "ReactXP.Components.View";
                            return Make_1((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (e) => {
                                e.stopPropagation();
                            }), (__currClass_1 = nthChildClasses(__sibI_3, __sibC_3), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list = singleton_1(RenderResult_Make_E25108F((tupledArg_5) => {
                                let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
                                const __sibI_4 = tupledArg_5[0] | 0;
                                const __sibC_4 = tupledArg_5[1] | 0;
                                const __pFQN_4 = tupledArg_5[2];
                                const __parentFQN_5 = "ReactXP.Components.TextInput";
                                return Make_2((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeTextInputProps_Static_5931E0E0(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, "Type to filter", void 0, void 0, NonemptyStringModule_optionToString(estate.ModelState.MaybeQuery), void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (arg) => {
                                    Actions$1__OnQueryChange_Z2972C1DC(actions, NonemptyStringModule_ofString(arg));
                                }, uncurry(2, void 0), void 0, uncurry(2, void 0), bindInput), (__currClass_2 = (format("{0}{1}{2}", "text-input ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.TextInput", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))(empty_1());
                            })), RenderResult_ToRawElements(__parentFQN_4, __list)));
                        })), RenderResult_ToRawElements(__parentFQN_3, __list_1));
                        return react.createElement(react.Fragment, {}, ...children);
                    }, (input) => {
                        input.requestFocus();
                    });
                    const __currClass_3 = nthChildClasses(__sibI_2, __sibC_2);
                    const __currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3);
                    const __implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["__style", __currStyles_3]) : empty())))));
                    arg00_3 = injectImplicitProps(__implicitProps_3, __currExplicitProps_3);
                    return Make_3()(arg00_3)([]);
                }), RenderResult_Make_E25108F((tupledArg_6) => {
                    let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4, whenAvailable, __list_16;
                    const __sibI_5 = tupledArg_6[0] | 0;
                    const __sibC_5 = tupledArg_6[1] | 0;
                    const __pFQN_5 = tupledArg_6[2];
                    const __parentFQN_6 = "ReactXP.Components.ScrollView";
                    return Make_4((__currExplicitProps_4 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5(void 0, true), (__currClass_4 = ("scroll-view" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ScrollView", __currStyles_4)]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))((whenAvailable = ((items) => {
                        let __list_9;
                        const children_2 = (__list_9 = singleton_1(RenderResult_Make_E25108F((tupledArg_7) => {
                            const __sibI_6 = tupledArg_7[0] | 0;
                            const __sibC_6 = tupledArg_7[1] | 0;
                            const __pFQN_6 = tupledArg_7[2];
                            const __parentFQN_7 = "LibClient.Components.ItemList";
                            let arg00_10;
                            const __currExplicitProps_11 = LibClient_Components_TypeExtensions_TEs__TEs_MakeItemListProps_Static_55018457(items, (items_1) => {
                                let __list_8;
                                const children_1 = (__list_8 = singleton_1(RenderResult_Make_410EF94B(ofSeq(mapIndexed((index, item) => RenderResult_Make_E25108F((tupledArg_8) => {
                                    let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5, __list_7;
                                    const __sibI_7 = tupledArg_8[0] | 0;
                                    const __sibC_7 = tupledArg_8[1] | 0;
                                    const __pFQN_7 = tupledArg_8[2];
                                    const __parentFQN_8 = "ReactXP.Components.View";
                                    return Make_1((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_5 = ("item" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_5)]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))((__list_7 = ofArray([RenderResult_Make_E25108F((tupledArg_9) => {
                                        let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6, __list_2;
                                        const __sibI_8 = tupledArg_9[0] | 0;
                                        const __sibC_8 = tupledArg_9[1] | 0;
                                        const __pFQN_8 = tupledArg_9[2];
                                        const __parentFQN_9 = "ReactXP.Components.View";
                                        return Make_1((__currExplicitProps_6 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_6 = ("item-selectedness" + nthChildClasses(__sibI_8, __sibC_8)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_6)]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))((__list_2 = singleton_1(SelectableValue$1__IsSelected_2B595(estate.ModelState.Value, item) ? RenderResult_Make_E25108F((tupledArg_10) => {
                                            let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
                                            const __sibI_9 = tupledArg_10[0] | 0;
                                            const __sibC_9 = tupledArg_10[1] | 0;
                                            const __pFQN_9 = tupledArg_10[2];
                                            const __parentFQN_10 = "LibClient.Components.Icon";
                                            return Make_5((__currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, Icon_get_CheckMark())), (__currClass_7 = ("item-selected-icon" + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))([]);
                                        }) : (new RenderResult(0))), RenderResult_ToRawElements(__parentFQN_9, __list_2)));
                                    }), RenderResult_Make_E25108F((tupledArg_11) => {
                                        let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8, __list_6, matchValue, render_1, __list_5, toItemInfo, __list_4;
                                        const __sibI_10 = tupledArg_11[0] | 0;
                                        const __sibC_10 = tupledArg_11[1] | 0;
                                        const __pFQN_10 = tupledArg_11[2];
                                        const __parentFQN_11 = "ReactXP.Components.View";
                                        return Make_1((__currExplicitProps_8 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_8 = ("item-body" + nthChildClasses(__sibI_10, __sibC_10)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))((__list_6 = singleton_1((matchValue = props.Parameters.ItemView, (matchValue.tag === 1) ? ((render_1 = matchValue.fields[0], RenderResult_Make_6E3A73D((__list_5 = singleton_1(RenderResult_Make_2B31D457(render_1(item))), RenderResult_ToRawElements(__parentFQN_11, __list_5))))) : ((toItemInfo = matchValue.fields[0], RenderResult_Make_6E3A73D((__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_12) => {
                                            let text, __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9;
                                            const __sibI_11 = tupledArg_12[0] | 0;
                                            const __sibC_11 = tupledArg_12[1] | 0;
                                            const __pFQN_11 = tupledArg_12[2];
                                            const __parentFQN_12 = "LibClient.Components.Text";
                                            let arg10_8;
                                            const __list_3 = singleton_1(RenderResult_Make_E25108F((text = format("{0}", toItemInfo(item).Label), (tupledArg_13) => makeTextNode(text, tupledArg_13[0], tupledArg_13[1], tupledArg_13[2]))));
                                            arg10_8 = RenderResult_ToRawElements(__parentFQN_12, __list_3);
                                            return Make_6((__currExplicitProps_9 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_9 = nthChildClasses(__sibI_11, __sibC_11), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["__style", __currStyles_9]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))(arg10_8);
                                        })), RenderResult_ToRawElements(__parentFQN_11, __list_4))))))), RenderResult_ToRawElements(__parentFQN_11, __list_6)));
                                    }), RenderResult_Make_E25108F((tupledArg_14) => {
                                        let __currExplicitProps_10, __currClass_10, __currStyles_10, __implicitProps_10;
                                        const __sibI_12 = tupledArg_14[0] | 0;
                                        const __sibC_12 = tupledArg_14[1] | 0;
                                        const __pFQN_12 = tupledArg_14[2];
                                        const __parentFQN_13 = "LibClient.Components.TapCapture";
                                        return Make_7((__currExplicitProps_10 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6((arg20) => {
                                            Actions$1__Toggle(actions, index, item, arg20);
                                        }), (__currClass_10 = nthChildClasses(__sibI_12, __sibC_12), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append((__currClass_10 !== "") ? singleton(["ClassName", __currClass_10]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["__style", __currStyles_10]) : empty()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))([]);
                                    })]), RenderResult_ToRawElements(__parentFQN_8, __list_7)));
                                }), items_1)))), RenderResult_ToRawElements(__parentFQN_7, __list_8));
                                return react.createElement(react.Fragment, {}, ...children_1);
                            }, new Style(0));
                            const __currClass_11 = "item-list" + nthChildClasses(__sibI_6, __sibC_6);
                            const __currStyles_11 = findApplicableStyles(__mergedStyles, __currClass_11);
                            const __implicitProps_11 = toList(delay(() => append((__currClass_11 !== "") ? singleton(["ClassName", __currClass_11]) : empty(), delay(() => ((!isEmpty(__currStyles_11)) ? singleton(["__style", __currStyles_11]) : empty())))));
                            arg00_10 = injectImplicitProps(__implicitProps_11, __currExplicitProps_11);
                            return Make_8()(arg00_10)([]);
                        })), RenderResult_ToRawElements(__parentFQN_6, __list_9));
                        return react.createElement(react.Fragment, {}, ...children_2);
                    }), (__list_16 = singleton_1(RenderResult_Make_E25108F((tupledArg_15) => {
                        const __sibI_13 = tupledArg_15[0] | 0;
                        const __sibC_13 = tupledArg_15[1] | 0;
                        const __pFQN_13 = tupledArg_15[2];
                        const __parentFQN_14 = "LibClient.Components.AsyncData";
                        let arg00_35;
                        const __currExplicitProps_16 = LibClient_Components_TypeExtensions_TEs__TEs_MakeAsyncDataProps_Static_379524B4(estate.ModelState.SelectableItems, (items_2) => {
                            let __list_10;
                            const children_3 = (__list_10 = singleton_1(RenderResult_Make_2B31D457(whenAvailable(items_2))), RenderResult_ToRawElements(__parentFQN_14, __list_10));
                            return react.createElement(react.Fragment, {}, ...children_3);
                        }, void 0, (maybeOldData) => {
                            let __list_15, oldData, __list_14, __list_12;
                            const children_4 = (__list_15 = singleton_1((maybeOldData != null) ? ((oldData = maybeOldData, RenderResult_Make_6E3A73D((__list_14 = ofArray([RenderResult_Make_2B31D457(whenAvailable(oldData)), RenderResult_Make_E25108F((tupledArg_18) => {
                                let __currExplicitProps_14, __currClass_14, __currStyles_14, __implicitProps_14, __list_13;
                                const __sibI_16 = tupledArg_18[0] | 0;
                                const __sibC_16 = tupledArg_18[1] | 0;
                                const __pFQN_16 = tupledArg_18[2];
                                const __parentFQN_17 = "ReactXP.Components.View";
                                return Make_1((__currExplicitProps_14 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_14 = ("activity-indicator-overlay" + nthChildClasses(__sibI_16, __sibC_16)), (__currStyles_14 = findApplicableStyles(__mergedStyles, __currClass_14), (__implicitProps_14 = toList(delay(() => append((__currClass_14 !== "") ? singleton(["ClassName", __currClass_14]) : empty(), delay(() => ((!isEmpty(__currStyles_14)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_14)]) : empty()))))), injectImplicitProps(__implicitProps_14, __currExplicitProps_14))))))((__list_13 = singleton_1(RenderResult_Make_E25108F((tupledArg_19) => {
                                    let __currExplicitProps_15, __currClass_15, __currStyles_15, __implicitProps_15;
                                    const __sibI_17 = tupledArg_19[0] | 0;
                                    const __sibC_17 = tupledArg_19[1] | 0;
                                    const __pFQN_17 = tupledArg_19[2];
                                    const __parentFQN_18 = "ReactXP.Components.ActivityIndicator";
                                    return Make_9((__currExplicitProps_15 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B("#aaaaaa", "medium"), (__currClass_15 = nthChildClasses(__sibI_17, __sibC_17), (__currStyles_15 = findApplicableStyles(__mergedStyles, __currClass_15), (__implicitProps_15 = toList(delay(() => append((__currClass_15 !== "") ? singleton(["ClassName", __currClass_15]) : empty(), delay(() => ((!isEmpty(__currStyles_15)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ActivityIndicator", __currStyles_15)]) : empty()))))), injectImplicitProps(__implicitProps_15, __currExplicitProps_15))))))(empty_1());
                                })), RenderResult_ToRawElements(__parentFQN_17, __list_13)));
                            })]), RenderResult_ToRawElements(__parentFQN_14, __list_14))))) : RenderResult_Make_6E3A73D((__list_12 = singleton_1(RenderResult_Make_E25108F((tupledArg_16) => {
                                let __currExplicitProps_12, __currClass_12, __currStyles_12, __implicitProps_12, __list_11;
                                const __sibI_14 = tupledArg_16[0] | 0;
                                const __sibC_14 = tupledArg_16[1] | 0;
                                const __pFQN_14 = tupledArg_16[2];
                                const __parentFQN_15 = "ReactXP.Components.View";
                                return Make_1((__currExplicitProps_12 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_12 = ("activity-indicator-block" + nthChildClasses(__sibI_14, __sibC_14)), (__currStyles_12 = findApplicableStyles(__mergedStyles, __currClass_12), (__implicitProps_12 = toList(delay(() => append((__currClass_12 !== "") ? singleton(["ClassName", __currClass_12]) : empty(), delay(() => ((!isEmpty(__currStyles_12)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_12)]) : empty()))))), injectImplicitProps(__implicitProps_12, __currExplicitProps_12))))))((__list_11 = singleton_1(RenderResult_Make_E25108F((tupledArg_17) => {
                                    let __currExplicitProps_13, __currClass_13, __currStyles_13, __implicitProps_13;
                                    const __sibI_15 = tupledArg_17[0] | 0;
                                    const __sibC_15 = tupledArg_17[1] | 0;
                                    const __pFQN_15 = tupledArg_17[2];
                                    const __parentFQN_16 = "ReactXP.Components.ActivityIndicator";
                                    return Make_9((__currExplicitProps_13 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B("#aaaaaa", "medium"), (__currClass_13 = nthChildClasses(__sibI_15, __sibC_15), (__currStyles_13 = findApplicableStyles(__mergedStyles, __currClass_13), (__implicitProps_13 = toList(delay(() => append((__currClass_13 !== "") ? singleton(["ClassName", __currClass_13]) : empty(), delay(() => ((!isEmpty(__currStyles_13)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ActivityIndicator", __currStyles_13)]) : empty()))))), injectImplicitProps(__implicitProps_13, __currExplicitProps_13))))))(empty_1());
                                })), RenderResult_ToRawElements(__parentFQN_15, __list_11)));
                            })), RenderResult_ToRawElements(__parentFQN_14, __list_12)))), RenderResult_ToRawElements(__parentFQN_14, __list_15));
                            return react.createElement(react.Fragment, {}, ...children_4);
                        });
                        const __currClass_16 = nthChildClasses(__sibI_13, __sibC_13);
                        const __currStyles_16 = findApplicableStyles(__mergedStyles, __currClass_16);
                        const __implicitProps_16 = toList(delay(() => append((__currClass_16 !== "") ? singleton(["ClassName", __currClass_16]) : empty(), delay(() => ((!isEmpty(__currStyles_16)) ? singleton(["__style", __currStyles_16]) : empty())))));
                        arg00_35 = injectImplicitProps(__implicitProps_16, __currExplicitProps_16);
                        return Make_10()(arg00_35)([]);
                    })), RenderResult_ToRawElements(__parentFQN_6, __list_16))));
                }), SelectableValue$1__get_CanSelectMultiple(estate.ModelState.Value) ? RenderResult_Make_E25108F((tupledArg_20) => {
                    let __currExplicitProps_17, __currClass_17, __currStyles_17, __implicitProps_17;
                    const __sibI_18 = tupledArg_20[0] | 0;
                    const __sibC_18 = tupledArg_20[1] | 0;
                    const __pFQN_18 = tupledArg_20[2];
                    const __parentFQN_19 = "LibClient.Components.Buttons";
                    let arg10_26;
                    const __list_17 = singleton_1(RenderResult_Make_E25108F((tupledArg_21) => {
                        let __currExplicitProps_18, __currClass_18, __currStyles_18, __implicitProps_18;
                        const __sibI_19 = tupledArg_21[0] | 0;
                        const __sibC_19 = tupledArg_21[1] | 0;
                        const __pFQN_19 = tupledArg_21[2];
                        const __parentFQN_20 = "LibClient.Components.Button";
                        return Make_11((__currExplicitProps_18 = LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52("Done", ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Actionable((arg00_55) => {
                            Actions$1__TryCancel_411CDB90(actions, arg00_55);
                        }))), (__currClass_18 = nthChildClasses(__sibI_19, __sibC_19), (__currStyles_18 = findApplicableStyles(__mergedStyles, __currClass_18), (__implicitProps_18 = toList(delay(() => append((__currClass_18 !== "") ? singleton(["ClassName", __currClass_18]) : empty(), delay(() => ((!isEmpty(__currStyles_18)) ? singleton(["__style", __currStyles_18]) : empty()))))), injectImplicitProps(__implicitProps_18, __currExplicitProps_18))))))([]);
                    }));
                    arg10_26 = RenderResult_ToRawElements(__parentFQN_19, __list_17);
                    return Make_12((__currExplicitProps_17 = LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonsProps_Static_519A9F32(), (__currClass_17 = nthChildClasses(__sibI_18, __sibC_18), (__currStyles_17 = findApplicableStyles(__mergedStyles, __currClass_17), (__implicitProps_17 = toList(delay(() => append((__currClass_17 !== "") ? singleton(["ClassName", __currClass_17]) : empty(), delay(() => ((!isEmpty(__currStyles_17)) ? singleton(["__style", __currStyles_17]) : empty()))))), injectImplicitProps(__implicitProps_17, __currExplicitProps_17))))))(arg10_26);
                }) : (new RenderResult(0))]);
                arg10_1 = RenderResult_ToRawElements(__parentFQN_2, __list_18);
                return Make_13((__currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_Shell_WhiteRounded_RawProps_Static_Z79AF04F8(When(ofArray([OnEscape, OnBackground]), (arg00_2) => {
                    Actions$1__TryCancel_411CDB90(actions, arg00_2);
                }), new DialogPosition(0)), (__currClass = nthChildClasses(__sibI_1, __sibC_1), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))(arg10_1);
            })), RenderResult_ToRawElements(__parentFQN_1, __list_19));
            return react.createElement(react.Fragment, {}, ...children_5);
        }), (__currClass_19 = nthChildClasses(__sibI, __sibC), (__currStyles_19 = findApplicableStyles(__mergedStyles, __currClass_19), (__implicitProps_19 = toList(delay(() => append((__currClass_19 !== "") ? singleton(["ClassName", __currClass_19]) : empty(), delay(() => ((!isEmpty(__currStyles_19)) ? singleton(["__style", __currStyles_19]) : empty()))))), injectImplicitProps(__implicitProps_19, __currExplicitProps_19))))))([]);
    })));
}

