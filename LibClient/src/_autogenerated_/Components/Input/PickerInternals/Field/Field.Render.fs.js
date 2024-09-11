import { map, defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../../ReactXP/Styles/Runtime.fs.js";
import { makeTextNode, RenderResult_Make_2B31D457, RenderResult_Make_410EF94B, RenderResult_ToRawElements, RenderResult, RenderResult_ToLeaves_410EF94B, RenderResult_Make_Z1C694E5A, nthChildClasses, RenderResult_Make_E25108F, RenderResult_Make_6E3A73D, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../RenderResult.fs.js";
import { Make } from "../../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { Actions$1__Unselect, Actions$1__OnChangeQuery_Z2972C1DC, Actions$1__OnBlur_Z1134E2DE, Actions$1__OnFocus_Z1134E2DE, Actions$1__OnKeyPress_Z1A8D693D, extractPlaceholderColor, Actions$1__ShowItemSelector_411CDB90, Actions$1__Clear_411CDB90, Actions$1__OnLayout_5199929D } from "../../../../../Components/Input/PickerInternals/Field/Field.typext.fs.js";
import { format } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../../../RenderHelpers.fs.js";
import { map as map_1, empty, singleton, append, delay, toList } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofSeq, empty as empty_1, singleton as singleton_1, ofArray, isEmpty } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { InputValidity__get_InvalidReason, SelectableValue$1__get_IsEmpty, InputValidity__get_IsInvalid } from "../../../../../Input.fs.js";
import { uncurry, equals } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Make as Make_1 } from "../../../../../Components/Icon/Icon.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B } from "../../../Icon/Icon.TypeExtensions.fs.js";
import { Icon_get_ChevronDown, Icon_get_X } from "../../../../../Icons.fs.js";
import { Make as Make_2 } from "../../../../../Components/TapCapture/TapCapture.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6 } from "../../../TapCapture/TapCapture.TypeExtensions.fs.js";
import { Option_getOrElse } from "../../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { OrderedSet$1__get_ToSeq, OrderedSet$1__get_Count, OrderedSet$1__get_IsEmpty } from "../../../../../../../LibLangFsharp/src/OrderedSet.fs.js";
import { Make as Make_3 } from "../../../../../Components/Responsive/Responsive.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeResponsiveProps_Static_502D6660 } from "../../../Responsive/Responsive.TypeExtensions.fs.js";
import { Make as Make_4 } from "../../../../../ReactXP/Components/TextInput/TextInput.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeTextInputProps_Static_5931E0E0 } from "../../../../ReactXP/Components/TextInput/TextInput.TypeExtensions.fs.js";
import { Color__get_ToReactXPString } from "../../../../../ColorModule.fs.js";
import { NonemptyStringModule_ofString, NonemptyStringModule_optionToString } from "../../../../../../../LibLangFsharp/src/NonemptyString.fs.js";
import * as react from "react";
import { Make as Make_5 } from "../../../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../../../UiText/UiText.TypeExtensions.fs.js";
import { DeleteState$1 } from "../../../../../Components/Input/Picker/Model.fs.js";
import { Make as Make_6 } from "../../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../../Text/Text.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    let __list_35;
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_6E3A73D((__list_35 = singleton_1(RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_34;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (arg00) => {
            Actions$1__OnLayout_5199929D(actions, arg00);
        }), (__currClass = (format("{0}{1}{2}", "view ", TopLevelBlockClass, "") + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_34 = ofArray([RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_29;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = (("border" + format(" {0} {1}", InputValidity__get_IsInvalid(props.Validity) ? "border-invalid" : "", estate.IsFocused ? "border-focused" : "")) + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_29 = ofArray([RenderResult_Make_E25108F((tupledArg_2) => {
                let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list_7;
                const __sibI_2 = tupledArg_2[0] | 0;
                const __sibC_2 = tupledArg_2[1] | 0;
                const __pFQN_2 = tupledArg_2[2];
                const __parentFQN_3 = "ReactXP.Components.View";
                return Make((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_2 = ("picker-actions" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list_7 = ofArray([RenderResult_Make_E25108F((tupledArg_3) => {
                    let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3, __list_5, matchValue, maybeSelectedValues, __list_1, __list, maybeSelectedValues_1, __list_4;
                    const __sibI_3 = tupledArg_3[0] | 0;
                    const __sibC_3 = tupledArg_3[1] | 0;
                    const __pFQN_3 = tupledArg_3[2];
                    const __parentFQN_4 = "ReactXP.Components.View";
                    return Make((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = nthChildClasses(__sibI_3, __sibC_3), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))((__list_5 = singleton_1((matchValue = props.Value, (matchValue.tag === 0) ? ((maybeSelectedValues = matchValue.fields[0], RenderResult_Make_6E3A73D((__list_1 = singleton_1((!equals(maybeSelectedValues, void 0)) ? RenderResult_Make_Z1C694E5A((__list = ofArray([RenderResult_Make_E25108F((tupledArg_4) => {
                        let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                        const __sibI_4 = tupledArg_4[0] | 0;
                        const __sibC_4 = tupledArg_4[1] | 0;
                        const __pFQN_4 = tupledArg_4[2];
                        const __parentFQN_5 = "LibClient.Components.Icon";
                        return Make_1((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, Icon_get_X())), (__currClass_4 = ("icon" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))([]);
                    }), RenderResult_Make_E25108F((tupledArg_5) => {
                        let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5;
                        const __sibI_5 = tupledArg_5[0] | 0;
                        const __sibC_5 = tupledArg_5[1] | 0;
                        const __pFQN_5 = tupledArg_5[2];
                        const __parentFQN_6 = "LibClient.Components.TapCapture";
                        return Make_2((__currExplicitProps_5 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6((arg00_4) => {
                            Actions$1__Clear_411CDB90(actions, arg00_4);
                        }), (__currClass_5 = nthChildClasses(__sibI_5, __sibC_5), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))([]);
                    })]), RenderResult_ToLeaves_410EF94B(__list))) : (new RenderResult(0))), RenderResult_ToRawElements(__parentFQN_4, __list_1))))) : ((matchValue.tag === 3) ? ((maybeSelectedValues_1 = matchValue.fields[0], RenderResult_Make_6E3A73D((__list_4 = singleton_1(Option_getOrElse(new RenderResult(0), map((selectedValues) => {
                        let __list_3, __list_2;
                        return RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_Z1C694E5A((__list_3 = singleton_1((!OrderedSet$1__get_IsEmpty(selectedValues)) ? RenderResult_Make_Z1C694E5A((__list_2 = ofArray([RenderResult_Make_E25108F((tupledArg_6) => {
                            let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                            const __sibI_6 = tupledArg_6[0] | 0;
                            const __sibC_6 = tupledArg_6[1] | 0;
                            const __pFQN_6 = tupledArg_6[2];
                            const __parentFQN_7 = "LibClient.Components.Icon";
                            return Make_1((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, Icon_get_X())), (__currClass_6 = ("icon" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["__style", __currStyles_6]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))([]);
                        }), RenderResult_Make_E25108F((tupledArg_7) => {
                            let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
                            const __sibI_7 = tupledArg_7[0] | 0;
                            const __sibC_7 = tupledArg_7[1] | 0;
                            const __pFQN_7 = tupledArg_7[2];
                            const __parentFQN_8 = "LibClient.Components.TapCapture";
                            return Make_2((__currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6((arg00_13) => {
                                Actions$1__Clear_411CDB90(actions, arg00_13);
                            }), (__currClass_7 = nthChildClasses(__sibI_7, __sibC_7), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))([]);
                        })]), RenderResult_ToLeaves_410EF94B(__list_2))) : (new RenderResult(0))), RenderResult_ToLeaves_410EF94B(__list_3)))));
                    }, maybeSelectedValues_1))), RenderResult_ToRawElements(__parentFQN_4, __list_4))))) : (new RenderResult(0))))), RenderResult_ToRawElements(__parentFQN_4, __list_5)));
                }), RenderResult_Make_E25108F((tupledArg_8) => {
                    let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8, __list_6;
                    const __sibI_8 = tupledArg_8[0] | 0;
                    const __sibC_8 = tupledArg_8[1] | 0;
                    const __pFQN_8 = tupledArg_8[2];
                    const __parentFQN_9 = "ReactXP.Components.View";
                    return Make((__currExplicitProps_8 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_8 = nthChildClasses(__sibI_8, __sibC_8), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))((__list_6 = ofArray([RenderResult_Make_E25108F((tupledArg_9) => {
                        let __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9;
                        const __sibI_9 = tupledArg_9[0] | 0;
                        const __sibC_9 = tupledArg_9[1] | 0;
                        const __pFQN_9 = tupledArg_9[2];
                        const __parentFQN_10 = "LibClient.Components.Icon";
                        return Make_1((__currExplicitProps_9 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, Icon_get_ChevronDown())), (__currClass_9 = ("icon" + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["__style", __currStyles_9]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))([]);
                    }), RenderResult_Make_E25108F((tupledArg_10) => {
                        let __currExplicitProps_10, __currClass_10, __currStyles_10, __implicitProps_10;
                        const __sibI_10 = tupledArg_10[0] | 0;
                        const __sibC_10 = tupledArg_10[1] | 0;
                        const __pFQN_10 = tupledArg_10[2];
                        const __parentFQN_11 = "LibClient.Components.TapCapture";
                        return Make_2((__currExplicitProps_10 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6((arg00_26) => {
                            Actions$1__ShowItemSelector_411CDB90(actions, arg00_26);
                        }), (__currClass_10 = ("tap-area" + nthChildClasses(__sibI_10, __sibC_10)), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append((__currClass_10 !== "") ? singleton(["ClassName", __currClass_10]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["__style", __currStyles_10]) : empty()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))([]);
                    })]), RenderResult_ToRawElements(__parentFQN_9, __list_6)));
                })]), RenderResult_ToRawElements(__parentFQN_3, __list_7)));
            }), RenderResult_Make_E25108F((tupledArg_11) => {
                let __currExplicitProps_14, __currClass_14, __currStyles_14, __implicitProps_14;
                const __sibI_11 = tupledArg_11[0] | 0;
                const __sibC_11 = tupledArg_11[1] | 0;
                const __pFQN_11 = tupledArg_11[2];
                const __parentFQN_12 = "LibClient.Components.Responsive";
                return Make_3((__currExplicitProps_14 = LibClient_Components_TypeExtensions_TEs__TEs_MakeResponsiveProps_Static_502D6660((_arg1) => {
                    let __list_8;
                    const children = (__list_8 = singleton_1(RenderResult_Make_E25108F((tupledArg_12) => {
                        let __currExplicitProps_11, matchValue_1, value_2, __currClass_11, __currStyles_11, __implicitProps_11;
                        const __sibI_12 = tupledArg_12[0] | 0;
                        const __sibC_12 = tupledArg_12[1] | 0;
                        const __pFQN_12 = tupledArg_12[2];
                        const __parentFQN_13 = "ReactXP.Components.TextInput";
                        return Make_4((__currExplicitProps_11 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeTextInputProps_Static_5931E0E0(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (matchValue_1 = [SelectableValue$1__get_IsEmpty(props.Value), props.Placeholder], matchValue_1[0] ? ((matchValue_1[1] != null) ? ((value_2 = matchValue_1[1], value_2)) : "") : ""), Color__get_ToReactXPString(extractPlaceholderColor(__mergedStyles)), void 0, NonemptyStringModule_optionToString(estate.MaybeQuery), void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (arg00_33) => {
                            Actions$1__OnKeyPress_Z1A8D693D(actions, arg00_33);
                        }, (arg00_34) => {
                            Actions$1__OnFocus_Z1134E2DE(actions, arg00_34);
                        }, (arg00_35) => {
                            Actions$1__OnBlur_Z1134E2DE(actions, arg00_35);
                        }, void 0, (arg) => {
                            Actions$1__OnChangeQuery_Z2972C1DC(actions, NonemptyStringModule_ofString(arg));
                        }), (__currClass_11 = ("text-input" + nthChildClasses(__sibI_12, __sibC_12)), (__currStyles_11 = findApplicableStyles(__mergedStyles, __currClass_11), (__implicitProps_11 = toList(delay(() => append((__currClass_11 !== "") ? singleton(["ClassName", __currClass_11]) : empty(), delay(() => ((!isEmpty(__currStyles_11)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.TextInput", __currStyles_11)]) : empty()))))), injectImplicitProps(__implicitProps_11, __currExplicitProps_11))))))(empty_1());
                    })), RenderResult_ToRawElements(__parentFQN_12, __list_8));
                    return react.createElement(react.Fragment, {}, ...children);
                }, (_arg2) => {
                    let __list_10;
                    const children_1 = (__list_10 = singleton_1(RenderResult_Make_E25108F((tupledArg_13) => {
                        let __currExplicitProps_12, __currClass_12, __currStyles_12, __implicitProps_12, __list_9;
                        const __sibI_13 = tupledArg_13[0] | 0;
                        const __sibC_13 = tupledArg_13[1] | 0;
                        const __pFQN_13 = tupledArg_13[2];
                        const __parentFQN_14 = "ReactXP.Components.View";
                        return Make((__currExplicitProps_12 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_12 = ("handheld-full-width-tap-area" + nthChildClasses(__sibI_13, __sibC_13)), (__currStyles_12 = findApplicableStyles(__mergedStyles, __currClass_12), (__implicitProps_12 = toList(delay(() => append((__currClass_12 !== "") ? singleton(["ClassName", __currClass_12]) : empty(), delay(() => ((!isEmpty(__currStyles_12)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_12)]) : empty()))))), injectImplicitProps(__implicitProps_12, __currExplicitProps_12))))))((__list_9 = singleton_1(RenderResult_Make_E25108F((tupledArg_14) => {
                            let __currExplicitProps_13, __currClass_13, __currStyles_13, __implicitProps_13;
                            const __sibI_14 = tupledArg_14[0] | 0;
                            const __sibC_14 = tupledArg_14[1] | 0;
                            const __pFQN_14 = tupledArg_14[2];
                            const __parentFQN_15 = "LibClient.Components.TapCapture";
                            return Make_2((__currExplicitProps_13 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6((arg00_40) => {
                                Actions$1__ShowItemSelector_411CDB90(actions, arg00_40);
                            }), (__currClass_13 = ("tap-area" + nthChildClasses(__sibI_14, __sibC_14)), (__currStyles_13 = findApplicableStyles(__mergedStyles, __currClass_13), (__implicitProps_13 = toList(delay(() => append((__currClass_13 !== "") ? singleton(["ClassName", __currClass_13]) : empty(), delay(() => ((!isEmpty(__currStyles_13)) ? singleton(["__style", __currStyles_13]) : empty()))))), injectImplicitProps(__implicitProps_13, __currExplicitProps_13))))))([]);
                        })), RenderResult_ToRawElements(__parentFQN_14, __list_9)));
                    })), RenderResult_ToRawElements(__parentFQN_12, __list_10));
                    return react.createElement(react.Fragment, {}, ...children_1);
                }), (__currClass_14 = nthChildClasses(__sibI_11, __sibC_11), (__currStyles_14 = findApplicableStyles(__mergedStyles, __currClass_14), (__implicitProps_14 = toList(delay(() => append((__currClass_14 !== "") ? singleton(["ClassName", __currClass_14]) : empty(), delay(() => ((!isEmpty(__currStyles_14)) ? singleton(["__style", __currStyles_14]) : empty()))))), injectImplicitProps(__implicitProps_14, __currExplicitProps_14))))))([]);
            }), RenderResult_Make_E25108F((tupledArg_15) => {
                let __currExplicitProps_15, __currClass_15, __currStyles_15, __implicitProps_15, __list_28, matchValue_2, maybeSelectedValue_1, __list_20, maybeSelectedValues_2, __list_27, maybeSelectedValue, __list_15;
                const __sibI_15 = tupledArg_15[0] | 0;
                const __sibC_15 = tupledArg_15[1] | 0;
                const __pFQN_15 = tupledArg_15[2];
                const __parentFQN_16 = "ReactXP.Components.View";
                return Make((__currExplicitProps_15 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_15 = ("picker-values" + nthChildClasses(__sibI_15, __sibC_15)), (__currStyles_15 = findApplicableStyles(__mergedStyles, __currClass_15), (__implicitProps_15 = toList(delay(() => append((__currClass_15 !== "") ? singleton(["ClassName", __currClass_15]) : empty(), delay(() => ((!isEmpty(__currStyles_15)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_15)]) : empty()))))), injectImplicitProps(__implicitProps_15, __currExplicitProps_15))))))((__list_28 = singleton_1((matchValue_2 = props.Value, (matchValue_2.tag === 1) ? ((maybeSelectedValue_1 = matchValue_2.fields[0], RenderResult_Make_6E3A73D((__list_20 = singleton_1(Option_getOrElse(new RenderResult(0), map((item_1) => {
                    let __list_19, matchValue_4, renderItem_1, __list_18, toItemInfo_1, __list_17;
                    return RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_Z1C694E5A((__list_19 = singleton_1((matchValue_4 = props.ItemView, (matchValue_4.tag === 1) ? ((renderItem_1 = matchValue_4.fields[0], RenderResult_Make_6E3A73D((__list_18 = singleton_1(RenderResult_Make_2B31D457(renderItem_1(item_1))), RenderResult_ToRawElements(__parentFQN_16, __list_18))))) : ((toItemInfo_1 = matchValue_4.fields[0], RenderResult_Make_6E3A73D((__list_17 = singleton_1(RenderResult_Make_E25108F((tupledArg_18) => {
                        let text_1, __currExplicitProps_17, __currClass_17, __currStyles_17, __implicitProps_17;
                        const __sibI_17 = tupledArg_18[0] | 0;
                        const __sibC_17 = tupledArg_18[1] | 0;
                        const __pFQN_17 = tupledArg_18[2];
                        const __parentFQN_18 = "LibClient.Components.UiText";
                        let arg10_21;
                        const __list_16 = singleton_1(RenderResult_Make_E25108F((text_1 = format("{0}", toItemInfo_1(item_1).Label), (tupledArg_19) => makeTextNode(text_1, tupledArg_19[0], tupledArg_19[1], tupledArg_19[2]))));
                        arg10_21 = RenderResult_ToRawElements(__parentFQN_18, __list_16);
                        return Make_5((__currExplicitProps_17 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_17 = ("selected-item" + nthChildClasses(__sibI_17, __sibC_17)), (__currStyles_17 = findApplicableStyles(__mergedStyles, __currClass_17), (__implicitProps_17 = toList(delay(() => append((__currClass_17 !== "") ? singleton(["ClassName", __currClass_17]) : empty(), delay(() => ((!isEmpty(__currStyles_17)) ? singleton(["__style", __currStyles_17]) : empty()))))), injectImplicitProps(__implicitProps_17, __currExplicitProps_17))))))(arg10_21);
                    })), RenderResult_ToRawElements(__parentFQN_16, __list_17))))))), RenderResult_ToLeaves_410EF94B(__list_19)))));
                }, maybeSelectedValue_1))), RenderResult_ToRawElements(__parentFQN_16, __list_20))))) : ((matchValue_2.tag === 2) ? ((maybeSelectedValues_2 = matchValue_2.fields[0], RenderResult_Make_6E3A73D((__list_27 = singleton_1(Option_getOrElse(new RenderResult(0), map((selectedValues_1) => {
                    let __list_26;
                    return RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_Z1C694E5A((__list_26 = singleton_1(RenderResult_Make_410EF94B(ofSeq(map_1((item_2) => RenderResult_Make_E25108F((tupledArg_20) => {
                        let __currExplicitProps_18, __currClass_18, __currStyles_18, __implicitProps_18, __list_25, matchValue_5, renderItem_2, __list_23, toItemInfo_2, __list_22;
                        const __sibI_18 = tupledArg_20[0] | 0;
                        const __sibC_18 = tupledArg_20[1] | 0;
                        const __pFQN_18 = tupledArg_20[2];
                        const __parentFQN_19 = "ReactXP.Components.View";
                        return Make((__currExplicitProps_18 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_18 = (("tag" + format(" {0}", equals(estate.ModelState.DeleteState, new DeleteState$1(1, item_2)) ? "highlighted" : "")) + nthChildClasses(__sibI_18, __sibC_18)), (__currStyles_18 = findApplicableStyles(__mergedStyles, __currClass_18), (__implicitProps_18 = toList(delay(() => append((__currClass_18 !== "") ? singleton(["ClassName", __currClass_18]) : empty(), delay(() => ((!isEmpty(__currStyles_18)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_18)]) : empty()))))), injectImplicitProps(__implicitProps_18, __currExplicitProps_18))))))((__list_25 = ofArray([(matchValue_5 = props.ItemView, (matchValue_5.tag === 1) ? ((renderItem_2 = matchValue_5.fields[0], RenderResult_Make_6E3A73D((__list_23 = singleton_1(RenderResult_Make_2B31D457(renderItem_2(item_2))), RenderResult_ToRawElements(__parentFQN_19, __list_23))))) : ((toItemInfo_2 = matchValue_5.fields[0], RenderResult_Make_6E3A73D((__list_22 = singleton_1(RenderResult_Make_E25108F((tupledArg_21) => {
                            let text_2, __currExplicitProps_19, __currClass_19, __currStyles_19, __implicitProps_19;
                            const __sibI_19 = tupledArg_21[0] | 0;
                            const __sibC_19 = tupledArg_21[1] | 0;
                            const __pFQN_19 = tupledArg_21[2];
                            const __parentFQN_20 = "LibClient.Components.UiText";
                            let arg10_26;
                            const __list_21 = singleton_1(RenderResult_Make_E25108F((text_2 = format("{0}", toItemInfo_2(item_2).Label), (tupledArg_22) => makeTextNode(text_2, tupledArg_22[0], tupledArg_22[1], tupledArg_22[2]))));
                            arg10_26 = RenderResult_ToRawElements(__parentFQN_20, __list_21);
                            return Make_5((__currExplicitProps_19 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_19 = ("selected-item tag-text" + nthChildClasses(__sibI_19, __sibC_19)), (__currStyles_19 = findApplicableStyles(__mergedStyles, __currClass_19), (__implicitProps_19 = toList(delay(() => append((__currClass_19 !== "") ? singleton(["ClassName", __currClass_19]) : empty(), delay(() => ((!isEmpty(__currStyles_19)) ? singleton(["__style", __currStyles_19]) : empty()))))), injectImplicitProps(__implicitProps_19, __currExplicitProps_19))))))(arg10_26);
                        })), RenderResult_ToRawElements(__parentFQN_19, __list_22)))))), ((OrderedSet$1__get_Count(selectedValues_1) > 1) ? true : (props.Value.tag === 3)) ? RenderResult_Make_E25108F((tupledArg_23) => {
                            let __currExplicitProps_20, __currClass_20, __currStyles_20, __implicitProps_20, __list_24;
                            const __sibI_20 = tupledArg_23[0] | 0;
                            const __sibC_20 = tupledArg_23[1] | 0;
                            const __pFQN_20 = tupledArg_23[2];
                            const __parentFQN_21 = "ReactXP.Components.View";
                            return Make((__currExplicitProps_20 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_20 = nthChildClasses(__sibI_20, __sibC_20), (__currStyles_20 = findApplicableStyles(__mergedStyles, __currClass_20), (__implicitProps_20 = toList(delay(() => append((__currClass_20 !== "") ? singleton(["ClassName", __currClass_20]) : empty(), delay(() => ((!isEmpty(__currStyles_20)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_20)]) : empty()))))), injectImplicitProps(__implicitProps_20, __currExplicitProps_20))))))((__list_24 = ofArray([RenderResult_Make_E25108F((tupledArg_24) => {
                                let __currExplicitProps_21, __currClass_21, __currStyles_21, __implicitProps_21;
                                const __sibI_21 = tupledArg_24[0] | 0;
                                const __sibC_21 = tupledArg_24[1] | 0;
                                const __pFQN_21 = tupledArg_24[2];
                                const __parentFQN_22 = "LibClient.Components.Icon";
                                return Make_1((__currExplicitProps_21 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, Icon_get_X())), (__currClass_21 = ("icon" + nthChildClasses(__sibI_21, __sibC_21)), (__currStyles_21 = findApplicableStyles(__mergedStyles, __currClass_21), (__implicitProps_21 = toList(delay(() => append((__currClass_21 !== "") ? singleton(["ClassName", __currClass_21]) : empty(), delay(() => ((!isEmpty(__currStyles_21)) ? singleton(["__style", __currStyles_21]) : empty()))))), injectImplicitProps(__implicitProps_21, __currExplicitProps_21))))))([]);
                            }), RenderResult_Make_E25108F((tupledArg_25) => {
                                let __currExplicitProps_22, __currClass_22, __currStyles_22, __implicitProps_22;
                                const __sibI_22 = tupledArg_25[0] | 0;
                                const __sibC_22 = tupledArg_25[1] | 0;
                                const __pFQN_22 = tupledArg_25[2];
                                const __parentFQN_23 = "LibClient.Components.TapCapture";
                                return Make_2((__currExplicitProps_22 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6((arg10_32) => {
                                    Actions$1__Unselect(actions, item_2, arg10_32);
                                }), (__currClass_22 = nthChildClasses(__sibI_22, __sibC_22), (__currStyles_22 = findApplicableStyles(__mergedStyles, __currClass_22), (__implicitProps_22 = toList(delay(() => append((__currClass_22 !== "") ? singleton(["ClassName", __currClass_22]) : empty(), delay(() => ((!isEmpty(__currStyles_22)) ? singleton(["__style", __currStyles_22]) : empty()))))), injectImplicitProps(__implicitProps_22, __currExplicitProps_22))))))([]);
                            })]), RenderResult_ToRawElements(__parentFQN_21, __list_24)));
                        }) : (new RenderResult(0))]), RenderResult_ToRawElements(__parentFQN_19, __list_25)));
                    }), OrderedSet$1__get_ToSeq(selectedValues_1))))), RenderResult_ToLeaves_410EF94B(__list_26)))));
                }, maybeSelectedValues_2))), RenderResult_ToRawElements(__parentFQN_16, __list_27))))) : ((matchValue_2.tag === 3) ? ((maybeSelectedValues_2 = matchValue_2.fields[0], RenderResult_Make_6E3A73D((__list_27 = singleton_1(Option_getOrElse(new RenderResult(0), map((selectedValues_1) => {
                    let __list_26;
                    return RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_Z1C694E5A((__list_26 = singleton_1(RenderResult_Make_410EF94B(ofSeq(map_1((item_2) => RenderResult_Make_E25108F((tupledArg_20) => {
                        let __currExplicitProps_18, __currClass_18, __currStyles_18, __implicitProps_18, __list_25, matchValue_5, renderItem_2, __list_23, toItemInfo_2, __list_22;
                        const __sibI_18 = tupledArg_20[0] | 0;
                        const __sibC_18 = tupledArg_20[1] | 0;
                        const __pFQN_18 = tupledArg_20[2];
                        const __parentFQN_19 = "ReactXP.Components.View";
                        return Make((__currExplicitProps_18 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_18 = (("tag" + format(" {0}", equals(estate.ModelState.DeleteState, new DeleteState$1(1, item_2)) ? "highlighted" : "")) + nthChildClasses(__sibI_18, __sibC_18)), (__currStyles_18 = findApplicableStyles(__mergedStyles, __currClass_18), (__implicitProps_18 = toList(delay(() => append((__currClass_18 !== "") ? singleton(["ClassName", __currClass_18]) : empty(), delay(() => ((!isEmpty(__currStyles_18)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_18)]) : empty()))))), injectImplicitProps(__implicitProps_18, __currExplicitProps_18))))))((__list_25 = ofArray([(matchValue_5 = props.ItemView, (matchValue_5.tag === 1) ? ((renderItem_2 = matchValue_5.fields[0], RenderResult_Make_6E3A73D((__list_23 = singleton_1(RenderResult_Make_2B31D457(renderItem_2(item_2))), RenderResult_ToRawElements(__parentFQN_19, __list_23))))) : ((toItemInfo_2 = matchValue_5.fields[0], RenderResult_Make_6E3A73D((__list_22 = singleton_1(RenderResult_Make_E25108F((tupledArg_21) => {
                            let text_2, __currExplicitProps_19, __currClass_19, __currStyles_19, __implicitProps_19;
                            const __sibI_19 = tupledArg_21[0] | 0;
                            const __sibC_19 = tupledArg_21[1] | 0;
                            const __pFQN_19 = tupledArg_21[2];
                            const __parentFQN_20 = "LibClient.Components.UiText";
                            let arg10_26;
                            const __list_21 = singleton_1(RenderResult_Make_E25108F((text_2 = format("{0}", toItemInfo_2(item_2).Label), (tupledArg_22) => makeTextNode(text_2, tupledArg_22[0], tupledArg_22[1], tupledArg_22[2]))));
                            arg10_26 = RenderResult_ToRawElements(__parentFQN_20, __list_21);
                            return Make_5((__currExplicitProps_19 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_19 = ("selected-item tag-text" + nthChildClasses(__sibI_19, __sibC_19)), (__currStyles_19 = findApplicableStyles(__mergedStyles, __currClass_19), (__implicitProps_19 = toList(delay(() => append((__currClass_19 !== "") ? singleton(["ClassName", __currClass_19]) : empty(), delay(() => ((!isEmpty(__currStyles_19)) ? singleton(["__style", __currStyles_19]) : empty()))))), injectImplicitProps(__implicitProps_19, __currExplicitProps_19))))))(arg10_26);
                        })), RenderResult_ToRawElements(__parentFQN_19, __list_22)))))), ((OrderedSet$1__get_Count(selectedValues_1) > 1) ? true : (props.Value.tag === 3)) ? RenderResult_Make_E25108F((tupledArg_23) => {
                            let __currExplicitProps_20, __currClass_20, __currStyles_20, __implicitProps_20, __list_24;
                            const __sibI_20 = tupledArg_23[0] | 0;
                            const __sibC_20 = tupledArg_23[1] | 0;
                            const __pFQN_20 = tupledArg_23[2];
                            const __parentFQN_21 = "ReactXP.Components.View";
                            return Make((__currExplicitProps_20 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_20 = nthChildClasses(__sibI_20, __sibC_20), (__currStyles_20 = findApplicableStyles(__mergedStyles, __currClass_20), (__implicitProps_20 = toList(delay(() => append((__currClass_20 !== "") ? singleton(["ClassName", __currClass_20]) : empty(), delay(() => ((!isEmpty(__currStyles_20)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_20)]) : empty()))))), injectImplicitProps(__implicitProps_20, __currExplicitProps_20))))))((__list_24 = ofArray([RenderResult_Make_E25108F((tupledArg_24) => {
                                let __currExplicitProps_21, __currClass_21, __currStyles_21, __implicitProps_21;
                                const __sibI_21 = tupledArg_24[0] | 0;
                                const __sibC_21 = tupledArg_24[1] | 0;
                                const __pFQN_21 = tupledArg_24[2];
                                const __parentFQN_22 = "LibClient.Components.Icon";
                                return Make_1((__currExplicitProps_21 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, Icon_get_X())), (__currClass_21 = ("icon" + nthChildClasses(__sibI_21, __sibC_21)), (__currStyles_21 = findApplicableStyles(__mergedStyles, __currClass_21), (__implicitProps_21 = toList(delay(() => append((__currClass_21 !== "") ? singleton(["ClassName", __currClass_21]) : empty(), delay(() => ((!isEmpty(__currStyles_21)) ? singleton(["__style", __currStyles_21]) : empty()))))), injectImplicitProps(__implicitProps_21, __currExplicitProps_21))))))([]);
                            }), RenderResult_Make_E25108F((tupledArg_25) => {
                                let __currExplicitProps_22, __currClass_22, __currStyles_22, __implicitProps_22;
                                const __sibI_22 = tupledArg_25[0] | 0;
                                const __sibC_22 = tupledArg_25[1] | 0;
                                const __pFQN_22 = tupledArg_25[2];
                                const __parentFQN_23 = "LibClient.Components.TapCapture";
                                return Make_2((__currExplicitProps_22 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6((arg10_32) => {
                                    Actions$1__Unselect(actions, item_2, arg10_32);
                                }), (__currClass_22 = nthChildClasses(__sibI_22, __sibC_22), (__currStyles_22 = findApplicableStyles(__mergedStyles, __currClass_22), (__implicitProps_22 = toList(delay(() => append((__currClass_22 !== "") ? singleton(["ClassName", __currClass_22]) : empty(), delay(() => ((!isEmpty(__currStyles_22)) ? singleton(["__style", __currStyles_22]) : empty()))))), injectImplicitProps(__implicitProps_22, __currExplicitProps_22))))))([]);
                            })]), RenderResult_ToRawElements(__parentFQN_21, __list_24)));
                        }) : (new RenderResult(0))]), RenderResult_ToRawElements(__parentFQN_19, __list_25)));
                    }), OrderedSet$1__get_ToSeq(selectedValues_1))))), RenderResult_ToLeaves_410EF94B(__list_26)))));
                }, maybeSelectedValues_2))), RenderResult_ToRawElements(__parentFQN_16, __list_27))))) : ((maybeSelectedValue = matchValue_2.fields[0], RenderResult_Make_6E3A73D((__list_15 = singleton_1(Option_getOrElse(new RenderResult(0), map((item) => {
                    let __list_14, matchValue_3, renderItem, __list_13, toItemInfo, __list_12;
                    return RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_Z1C694E5A((__list_14 = singleton_1((matchValue_3 = props.ItemView, (matchValue_3.tag === 1) ? ((renderItem = matchValue_3.fields[0], RenderResult_Make_6E3A73D((__list_13 = singleton_1(RenderResult_Make_2B31D457(renderItem(item))), RenderResult_ToRawElements(__parentFQN_16, __list_13))))) : ((toItemInfo = matchValue_3.fields[0], RenderResult_Make_6E3A73D((__list_12 = singleton_1(RenderResult_Make_E25108F((tupledArg_16) => {
                        let text, __currExplicitProps_16, __currClass_16, __currStyles_16, __implicitProps_16;
                        const __sibI_16 = tupledArg_16[0] | 0;
                        const __sibC_16 = tupledArg_16[1] | 0;
                        const __pFQN_16 = tupledArg_16[2];
                        const __parentFQN_17 = "LibClient.Components.UiText";
                        let arg10_16;
                        const __list_11 = singleton_1(RenderResult_Make_E25108F((text = format("{0}", toItemInfo(item).Label), (tupledArg_17) => makeTextNode(text, tupledArg_17[0], tupledArg_17[1], tupledArg_17[2]))));
                        arg10_16 = RenderResult_ToRawElements(__parentFQN_17, __list_11);
                        return Make_5((__currExplicitProps_16 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_16 = ("selected-item" + nthChildClasses(__sibI_16, __sibC_16)), (__currStyles_16 = findApplicableStyles(__mergedStyles, __currClass_16), (__implicitProps_16 = toList(delay(() => append((__currClass_16 !== "") ? singleton(["ClassName", __currClass_16]) : empty(), delay(() => ((!isEmpty(__currStyles_16)) ? singleton(["__style", __currStyles_16]) : empty()))))), injectImplicitProps(__implicitProps_16, __currExplicitProps_16))))))(arg10_16);
                    })), RenderResult_ToRawElements(__parentFQN_16, __list_12))))))), RenderResult_ToLeaves_410EF94B(__list_14)))));
                }, maybeSelectedValue))), RenderResult_ToRawElements(__parentFQN_16, __list_15))))))))), RenderResult_ToRawElements(__parentFQN_16, __list_28)));
            })]), RenderResult_ToRawElements(__parentFQN_2, __list_29)));
        }), Option_getOrElse(new RenderResult(0), map((reason) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_26) => {
            let __currExplicitProps_23, __currClass_23, __currStyles_23, __implicitProps_23, __list_31;
            const __sibI_23 = tupledArg_26[0] | 0;
            const __sibC_23 = tupledArg_26[1] | 0;
            const __pFQN_23 = tupledArg_26[2];
            const __parentFQN_24 = "ReactXP.Components.View";
            return Make((__currExplicitProps_23 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_23 = nthChildClasses(__sibI_23, __sibC_23), (__currStyles_23 = findApplicableStyles(__mergedStyles, __currClass_23), (__implicitProps_23 = toList(delay(() => append((__currClass_23 !== "") ? singleton(["ClassName", __currClass_23]) : empty(), delay(() => ((!isEmpty(__currStyles_23)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_23)]) : empty()))))), injectImplicitProps(__implicitProps_23, __currExplicitProps_23))))))((__list_31 = singleton_1(RenderResult_Make_E25108F((tupledArg_27) => {
                let text_3, __currExplicitProps_24, __currClass_24, __currStyles_24, __implicitProps_24;
                const __sibI_24 = tupledArg_27[0] | 0;
                const __sibC_24 = tupledArg_27[1] | 0;
                const __pFQN_24 = tupledArg_27[2];
                const __parentFQN_25 = "LibClient.Components.Text";
                let arg10_38;
                const __list_30 = singleton_1(RenderResult_Make_E25108F((text_3 = format("{0}", reason), (tupledArg_28) => makeTextNode(text_3, tupledArg_28[0], tupledArg_28[1], tupledArg_28[2]))));
                arg10_38 = RenderResult_ToRawElements(__parentFQN_25, __list_30);
                return Make_6((__currExplicitProps_24 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_24 = ("invalid-reason" + nthChildClasses(__sibI_24, __sibC_24)), (__currStyles_24 = findApplicableStyles(__mergedStyles, __currClass_24), (__implicitProps_24 = toList(delay(() => append((__currClass_24 !== "") ? singleton(["ClassName", __currClass_24]) : empty(), delay(() => ((!isEmpty(__currStyles_24)) ? singleton(["__style", __currStyles_24]) : empty()))))), injectImplicitProps(__implicitProps_24, __currExplicitProps_24))))))(arg10_38);
            })), RenderResult_ToRawElements(__parentFQN_24, __list_31)));
        }))), InputValidity__get_InvalidReason(props.Validity))), Option_getOrElse(new RenderResult(0), map((label) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_29) => {
            let __currExplicitProps_25, __currClass_25, __currStyles_25, __implicitProps_25, __list_33;
            const __sibI_25 = tupledArg_29[0] | 0;
            const __sibC_25 = tupledArg_29[1] | 0;
            const __pFQN_25 = tupledArg_29[2];
            const __parentFQN_26 = "ReactXP.Components.View";
            return Make((__currExplicitProps_25 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_25 = ("label" + nthChildClasses(__sibI_25, __sibC_25)), (__currStyles_25 = findApplicableStyles(__mergedStyles, __currClass_25), (__implicitProps_25 = toList(delay(() => append((__currClass_25 !== "") ? singleton(["ClassName", __currClass_25]) : empty(), delay(() => ((!isEmpty(__currStyles_25)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_25)]) : empty()))))), injectImplicitProps(__implicitProps_25, __currExplicitProps_25))))))((__list_33 = singleton_1(RenderResult_Make_E25108F((tupledArg_30) => {
                let text_4, __currExplicitProps_26, __currClass_26, __currStyles_26, __implicitProps_26;
                const __sibI_26 = tupledArg_30[0] | 0;
                const __sibC_26 = tupledArg_30[1] | 0;
                const __pFQN_26 = tupledArg_30[2];
                const __parentFQN_27 = "LibClient.Components.UiText";
                let arg10_41;
                const __list_32 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}", label), (tupledArg_31) => makeTextNode(text_4, tupledArg_31[0], tupledArg_31[1], tupledArg_31[2]))));
                arg10_41 = RenderResult_ToRawElements(__parentFQN_27, __list_32);
                return Make_5((__currExplicitProps_26 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_26 = (("label-text" + format(" {0} {1}", InputValidity__get_IsInvalid(props.Validity) ? "invalid" : "", estate.IsFocused ? "focused" : "")) + nthChildClasses(__sibI_26, __sibC_26)), (__currStyles_26 = findApplicableStyles(__mergedStyles, __currClass_26), (__implicitProps_26 = toList(delay(() => append((__currClass_26 !== "") ? singleton(["ClassName", __currClass_26]) : empty(), delay(() => ((!isEmpty(__currStyles_26)) ? singleton(["__style", __currStyles_26]) : empty()))))), injectImplicitProps(__implicitProps_26, __currExplicitProps_26))))))(arg10_41);
            })), RenderResult_ToRawElements(__parentFQN_26, __list_33)));
        }))), props.Label))]), RenderResult_ToRawElements(__parentFQN_1, __list_34)));
    })), RenderResult_ToRawElements(__parentFQN, __list_35)))));
}

