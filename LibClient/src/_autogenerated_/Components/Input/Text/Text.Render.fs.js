import { some, map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_410EF94B, RenderResult, RenderResult_ToRawElements, makeTextNode, RenderResult_Make_6E3A73D, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, empty as empty_1, singleton as singleton_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { InputValidity__get_InvalidReason, InputValidity__get_IsInvalid } from "../../../../Input.fs.js";
import { Make as Make_1 } from "../../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../../UiText/UiText.TypeExtensions.fs.js";
import { Make as Make_2 } from "../../../../ReactXP/Components/TextInput/TextInput.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeTextInputProps_Static_5931E0E0 } from "../../../ReactXP/Components/TextInput/TextInput.TypeExtensions.fs.js";
import { Option_getOrElse } from "../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { Color__get_ToReactXPString } from "../../../../ColorModule.fs.js";
import { Actions__Focus_411CDB90, Actions__get_OnKeyPressOption, Actions__RefTextInput_Z2DB391BB, Actions__OnBlur_Z1134E2DE, Actions__OnFocus_Z1134E2DE, extractPlaceholderColor } from "../../../../Components/Input/Text/Text.typext.fs.js";
import { NonemptyStringModule_ofString, NonemptyStringModule_optionToString } from "../../../../../../LibLangFsharp/src/NonemptyString.fs.js";
import { curry, uncurry } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Make as Make_3 } from "../../../../Components/Icon/Icon.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B } from "../../Icon/Icon.TypeExtensions.fs.js";
import { Make as Make_4 } from "../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../Text/Text.TypeExtensions.fs.js";
import { Make as Make_5 } from "../../../../Components/TapCapture/TapCapture.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6 } from "../../TapCapture/TapCapture.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    let isLabelSmall;
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, (isLabelSmall = (((props.Value != null) ? true : estate.IsFocused) ? true : (props.Placeholder != null)), RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_12;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = ((format("{0}{1}{2}", "view ", TopLevelBlockClass, "") + format(" {0}", (props.Label != null) ? "withLabel" : "")) + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_12 = ofArray([RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_7, matchValue, prefix, __list_1, __list_2, matchValue_1, icon, __list_5, text_1, __list_4, __list_6;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = (("border" + format(" {0} {1}", InputValidity__get_IsInvalid(props.Validity) ? "border-invalid" : "", estate.IsFocused ? "border-focused" : "")) + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_7 = ofArray([(matchValue = [isLabelSmall, props.Prefix], matchValue[0] ? ((matchValue[1] != null) ? ((prefix = matchValue[1], RenderResult_Make_6E3A73D((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                let text, __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
                const __sibI_2 = tupledArg_2[0] | 0;
                const __sibC_2 = tupledArg_2[1] | 0;
                const __pFQN_2 = tupledArg_2[2];
                const __parentFQN_3 = "LibClient.Components.UiText";
                let arg10;
                const __list = singleton_1(RenderResult_Make_E25108F((text = format("{0}", prefix), (tupledArg_3) => makeTextNode(text, tupledArg_3[0], tupledArg_3[1], tupledArg_3[2]))));
                arg10 = RenderResult_ToRawElements(__parentFQN_3, __list);
                return Make_1((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_2 = ("prefix" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))(arg10);
            })), RenderResult_ToRawElements(__parentFQN_2, __list_1))))) : RenderResult_Make_6E3A73D((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
                let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                const __sibI_3 = tupledArg_4[0] | 0;
                const __sibC_3 = tupledArg_4[1] | 0;
                const __pFQN_3 = tupledArg_4[2];
                const __parentFQN_4 = "ReactXP.Components.View";
                return Make((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("focus-preserving-sentinel" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))(empty_1());
            })), RenderResult_ToRawElements(__parentFQN_2, __list_2)))) : RenderResult_Make_6E3A73D((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
                let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                const __sibI_3 = tupledArg_4[0] | 0;
                const __sibC_3 = tupledArg_4[1] | 0;
                const __pFQN_3 = tupledArg_4[2];
                const __parentFQN_4 = "ReactXP.Components.View";
                return Make((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("focus-preserving-sentinel" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))(empty_1());
            })), RenderResult_ToRawElements(__parentFQN_2, __list_2)))), RenderResult_Make_E25108F((tupledArg_5) => {
                let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                const __sibI_4 = tupledArg_5[0] | 0;
                const __sibC_4 = tupledArg_5[1] | 0;
                const __pFQN_4 = tupledArg_5[2];
                const __parentFQN_5 = "ReactXP.Components.TextInput";
                return Make_2((__currExplicitProps_4 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeTextInputProps_Static_5931E0E0(void 0, void 0, props.RequestFocusOnMount, void 0, void 0, void 0, void 0, void 0, props.Multiline, Option_getOrElse("", props.Placeholder), Color__get_ToReactXPString(extractPlaceholderColor(__mergedStyles)), props.SecureTextEntry, NonemptyStringModule_optionToString(props.Value), void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (arg00_9) => {
                    Actions__OnFocus_Z1134E2DE(actions, arg00_9);
                }, (arg00_10) => {
                    Actions__OnBlur_Z1134E2DE(actions, arg00_10);
                }, void 0, (arg) => {
                    props.OnChange(NonemptyStringModule_ofString(arg));
                }, uncurry(2, void 0), void 0, uncurry(2, void 0), (arg00_11) => {
                    Actions__RefTextInput_Z2DB391BB(actions, arg00_11);
                }, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, some(map((value_2) => value_2, props.MaxLength)), void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, some(Actions__get_OnKeyPressOption(actions))), (__currClass_4 = ("text-input" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.TextInput", __currStyles_4)]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))(empty_1());
            }), (matchValue_1 = [isLabelSmall, props.Suffix], matchValue_1[0] ? ((matchValue_1[1] != null) ? ((matchValue_1[1].tag === 1) ? ((icon = curry(2, matchValue_1[1].fields[0]), RenderResult_Make_6E3A73D((__list_5 = singleton_1(RenderResult_Make_E25108F((tupledArg_8) => {
                let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                const __sibI_6 = tupledArg_8[0] | 0;
                const __sibC_6 = tupledArg_8[1] | 0;
                const __pFQN_6 = tupledArg_8[2];
                const __parentFQN_7 = "LibClient.Components.Icon";
                return Make_3((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, icon)), (__currClass_6 = ("suffix-icon" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["__style", __currStyles_6]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))([]);
            })), RenderResult_ToRawElements(__parentFQN_2, __list_5))))) : ((text_1 = matchValue_1[1].fields[0], RenderResult_Make_6E3A73D((__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_6) => {
                let text_2, __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5;
                const __sibI_5 = tupledArg_6[0] | 0;
                const __sibC_5 = tupledArg_6[1] | 0;
                const __pFQN_5 = tupledArg_6[2];
                const __parentFQN_6 = "LibClient.Components.Text";
                let arg10_4;
                const __list_3 = singleton_1(RenderResult_Make_E25108F((text_2 = format("{0}", text_1), (tupledArg_7) => makeTextNode(text_2, tupledArg_7[0], tupledArg_7[1], tupledArg_7[2]))));
                arg10_4 = RenderResult_ToRawElements(__parentFQN_6, __list_3);
                return Make_4((__currExplicitProps_5 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_5 = ("suffix-text" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))(arg10_4);
            })), RenderResult_ToRawElements(__parentFQN_2, __list_4)))))) : RenderResult_Make_6E3A73D((__list_6 = singleton_1(RenderResult_Make_E25108F((tupledArg_9) => {
                let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
                const __sibI_7 = tupledArg_9[0] | 0;
                const __sibC_7 = tupledArg_9[1] | 0;
                const __pFQN_7 = tupledArg_9[2];
                const __parentFQN_8 = "ReactXP.Components.View";
                return Make((__currExplicitProps_7 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_7 = ("focus-preserving-sentinel" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_7)]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))(empty_1());
            })), RenderResult_ToRawElements(__parentFQN_2, __list_6)))) : RenderResult_Make_6E3A73D((__list_6 = singleton_1(RenderResult_Make_E25108F((tupledArg_9) => {
                let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
                const __sibI_7 = tupledArg_9[0] | 0;
                const __sibC_7 = tupledArg_9[1] | 0;
                const __pFQN_7 = tupledArg_9[2];
                const __parentFQN_8 = "ReactXP.Components.View";
                return Make((__currExplicitProps_7 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_7 = ("focus-preserving-sentinel" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_7)]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))(empty_1());
            })), RenderResult_ToRawElements(__parentFQN_2, __list_6))))]), RenderResult_ToRawElements(__parentFQN_2, __list_7)));
        }), Option_getOrElse(new RenderResult(0), map((reason) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_10) => {
            let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8, __list_9;
            const __sibI_8 = tupledArg_10[0] | 0;
            const __sibC_8 = tupledArg_10[1] | 0;
            const __pFQN_8 = tupledArg_10[2];
            const __parentFQN_9 = "ReactXP.Components.View";
            return Make((__currExplicitProps_8 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_8 = nthChildClasses(__sibI_8, __sibC_8), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))((__list_9 = singleton_1(RenderResult_Make_E25108F((tupledArg_11) => {
                let text_3, __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9;
                const __sibI_9 = tupledArg_11[0] | 0;
                const __sibC_9 = tupledArg_11[1] | 0;
                const __pFQN_9 = tupledArg_11[2];
                const __parentFQN_10 = "LibClient.Components.Text";
                let arg10_11;
                const __list_8 = singleton_1(RenderResult_Make_E25108F((text_3 = format("{0}", reason), (tupledArg_12) => makeTextNode(text_3, tupledArg_12[0], tupledArg_12[1], tupledArg_12[2]))));
                arg10_11 = RenderResult_ToRawElements(__parentFQN_10, __list_8);
                return Make_4((__currExplicitProps_9 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_9 = ("invalid-reason" + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["__style", __currStyles_9]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))(arg10_11);
            })), RenderResult_ToRawElements(__parentFQN_9, __list_9)));
        }))), InputValidity__get_InvalidReason(props.Validity))), Option_getOrElse(new RenderResult(0), map((label) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_13) => {
            let __currExplicitProps_10, __currClass_10, __currStyles_10, __implicitProps_10, __list_11;
            const __sibI_10 = tupledArg_13[0] | 0;
            const __sibC_10 = tupledArg_13[1] | 0;
            const __pFQN_10 = tupledArg_13[2];
            const __parentFQN_11 = "ReactXP.Components.View";
            return Make((__currExplicitProps_10 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_10 = (("label" + format(" {0}", isLabelSmall ? "small" : "")) + nthChildClasses(__sibI_10, __sibC_10)), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append((__currClass_10 !== "") ? singleton(["ClassName", __currClass_10]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_10)]) : empty()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))((__list_11 = ofArray([RenderResult_Make_E25108F((tupledArg_14) => {
                let text_4, __currExplicitProps_11, __currClass_11, __currStyles_11, __implicitProps_11;
                const __sibI_11 = tupledArg_14[0] | 0;
                const __sibC_11 = tupledArg_14[1] | 0;
                const __pFQN_11 = tupledArg_14[2];
                const __parentFQN_12 = "LibClient.Components.UiText";
                let arg10_14;
                const __list_10 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}", label), (tupledArg_15) => makeTextNode(text_4, tupledArg_15[0], tupledArg_15[1], tupledArg_15[2]))));
                arg10_14 = RenderResult_ToRawElements(__parentFQN_12, __list_10);
                return Make_1((__currExplicitProps_11 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_11 = (("label-text" + format(" {0} {1} {2}", InputValidity__get_IsInvalid(props.Validity) ? "invalid" : "", estate.IsFocused ? "focused" : "", isLabelSmall ? "small" : "")) + nthChildClasses(__sibI_11, __sibC_11)), (__currStyles_11 = findApplicableStyles(__mergedStyles, __currClass_11), (__implicitProps_11 = toList(delay(() => append((__currClass_11 !== "") ? singleton(["ClassName", __currClass_11]) : empty(), delay(() => ((!isEmpty(__currStyles_11)) ? singleton(["__style", __currStyles_11]) : empty()))))), injectImplicitProps(__implicitProps_11, __currExplicitProps_11))))))(arg10_14);
            }), RenderResult_Make_E25108F((tupledArg_16) => {
                let __currExplicitProps_12, __currClass_12, __currStyles_12, __implicitProps_12;
                const __sibI_12 = tupledArg_16[0] | 0;
                const __sibC_12 = tupledArg_16[1] | 0;
                const __pFQN_12 = tupledArg_16[2];
                const __parentFQN_13 = "LibClient.Components.TapCapture";
                return Make_5((__currExplicitProps_12 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6((arg00_39) => {
                    Actions__Focus_411CDB90(actions, arg00_39);
                }), (__currClass_12 = nthChildClasses(__sibI_12, __sibC_12), (__currStyles_12 = findApplicableStyles(__mergedStyles, __currClass_12), (__implicitProps_12 = toList(delay(() => append((__currClass_12 !== "") ? singleton(["ClassName", __currClass_12]) : empty(), delay(() => ((!isEmpty(__currStyles_12)) ? singleton(["__style", __currStyles_12]) : empty()))))), injectImplicitProps(__implicitProps_12, __currExplicitProps_12))))))([]);
            })]), RenderResult_ToRawElements(__parentFQN_11, __list_11)));
        }))), props.Label))]), RenderResult_ToRawElements(__parentFQN_1, __list_12)));
    }))));
}

