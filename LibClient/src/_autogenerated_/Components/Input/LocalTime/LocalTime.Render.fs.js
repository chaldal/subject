import { some, map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToRawElements, makeTextNode, RenderResult_Make_410EF94B, RenderResult, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { InputValidity__get_InvalidReason, InputValidity__Or_Z67BF8BF7, InputValidity__get_IsInvalid, InputValidity } from "../../../../Input.fs.js";
import { Make } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, singleton as singleton_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Option_getOrElse } from "../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { Value__SetPeriod_Z524259A4, periodPickerItems, Value__SetMinutes_Z2972C1DC, Actions__RefHoursInput_60F3BB8A, Actions__OnBlur_Z1134E2DE, Actions__OnFocus_Z1134E2DE, Field, Value__InternalFieldValidity_Z50D5E715, Value__SetHours_Z2972C1DC, Value__get_InternalValidity, Actions__Focus_Z5B3E8D2 } from "../../../../Components/Input/LocalTime/LocalTime.typext.fs.js";
import { format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { Make as Make_1 } from "../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../Text/Text.TypeExtensions.fs.js";
import { Make as Make_2 } from "../../../../Components/Input/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_TextProps_Static_CF9FCE9 } from "../Text/Text.TypeExtensions.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_Input_PickerProps_Static_3FD7D463 } from "../../Legacy/Input/Picker/Picker.TypeExtensions.fs.js";
import { Make as Make_3, OnChange$1, SelectedItem$1 } from "../../../../Components/Legacy/Input/Picker/Picker.typext.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    let externalValidityForFields, patternInput, rawPeriodOffset, rawMinutes, rawHours;
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, (externalValidityForFields = ((props.Validity.tag === 0) ? (new InputValidity(0)) : (new InputValidity(1))), (patternInput = props.Value.Raw, (rawPeriodOffset = (patternInput[2] | 0), (rawMinutes = patternInput[1], (rawHours = patternInput[0], RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_6;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = ("view" + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_6 = ofArray([Option_getOrElse(new RenderResult(0), map((label) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_1;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (arg00) => {
                Actions__Focus_Z5B3E8D2(actions, arg00);
            }), (__currClass_1 = nthChildClasses(__sibI_1, __sibC_1), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                let text, __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
                const __sibI_2 = tupledArg_2[0] | 0;
                const __sibC_2 = tupledArg_2[1] | 0;
                const __pFQN_2 = tupledArg_2[2];
                const __parentFQN_3 = "LibClient.Components.Text";
                let arg10;
                const __list = singleton_1(RenderResult_Make_E25108F((text = format("{0}", label), (tupledArg_3) => makeTextNode(text, tupledArg_3[0], tupledArg_3[1], tupledArg_3[2]))));
                arg10 = RenderResult_ToRawElements(__parentFQN_3, __list);
                return Make_1((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_2 = (("label" + format(" {0} {1}", InputValidity__get_IsInvalid(InputValidity__Or_Z67BF8BF7(Value__get_InternalValidity(props.Value), props.Validity)) ? "invalid" : "", estate.IsFocused ? "focused" : "")) + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))(arg10);
            })), RenderResult_ToRawElements(__parentFQN_2, __list_1)));
        }))), props.Label)), RenderResult_Make_E25108F((tupledArg_4) => {
            let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3, __list_3;
            const __sibI_3 = tupledArg_4[0] | 0;
            const __sibC_3 = tupledArg_4[1] | 0;
            const __pFQN_3 = tupledArg_4[2];
            const __parentFQN_4 = "ReactXP.Components.View";
            return Make((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("fields" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))((__list_3 = ofArray([RenderResult_Make_E25108F((tupledArg_5) => {
                let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                const __sibI_4 = tupledArg_5[0] | 0;
                const __sibC_4 = tupledArg_5[1] | 0;
                const __pFQN_4 = tupledArg_5[2];
                const __parentFQN_5 = "LibClient.Components.Input.Text";
                return Make_2((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_TextProps_Static_CF9FCE9(rawHours, (arg) => {
                    props.OnChange(Value__SetHours_Z2972C1DC(props.Value, arg));
                }, InputValidity__Or_Z67BF8BF7(Value__InternalFieldValidity_Z50D5E715(props.Value, new Field(0)), externalValidityForFields), void 0, props.RequestFocusOnMount, void 0, void 0, void 0, void 0, (arg00_9) => {
                    Actions__OnFocus_Z1134E2DE(actions, arg00_9);
                }, (arg00_10) => {
                    Actions__OnBlur_Z1134E2DE(actions, arg00_10);
                }, "00", void 0, void 0, 2, void 0, (arg00_11) => {
                    Actions__RefHoursInput_60F3BB8A(actions, arg00_11);
                }, void 0, void 0, some(props.OnEnterKeyPress)), (__currClass_4 = ("field" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))([]);
            }), RenderResult_Make_E25108F((tupledArg_6) => {
                let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5;
                const __sibI_5 = tupledArg_6[0] | 0;
                const __sibC_5 = tupledArg_6[1] | 0;
                const __pFQN_5 = tupledArg_6[2];
                const __parentFQN_6 = "LibClient.Components.Text";
                let arg10_4;
                const __list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_7) => makeTextNode(":", tupledArg_7[0], tupledArg_7[1], tupledArg_7[2])));
                arg10_4 = RenderResult_ToRawElements(__parentFQN_6, __list_2);
                return Make_1((__currExplicitProps_5 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_5 = ("colon" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))(arg10_4);
            }), RenderResult_Make_E25108F((tupledArg_8) => {
                let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                const __sibI_6 = tupledArg_8[0] | 0;
                const __sibC_6 = tupledArg_8[1] | 0;
                const __pFQN_6 = tupledArg_8[2];
                const __parentFQN_7 = "LibClient.Components.Input.Text";
                return Make_2((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_TextProps_Static_CF9FCE9(rawMinutes, (arg_1) => {
                    props.OnChange(Value__SetMinutes_Z2972C1DC(props.Value, arg_1));
                }, InputValidity__Or_Z67BF8BF7(Value__InternalFieldValidity_Z50D5E715(props.Value, new Field(1)), externalValidityForFields), void 0, void 0, void 0, void 0, void 0, void 0, (arg00_19) => {
                    Actions__OnFocus_Z1134E2DE(actions, arg00_19);
                }, (arg00_20) => {
                    Actions__OnBlur_Z1134E2DE(actions, arg00_20);
                }, "00", void 0, void 0, 2, void 0, void 0, void 0, void 0, some(props.OnEnterKeyPress)), (__currClass_6 = ("field" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["__style", __currStyles_6]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))([]);
            }), RenderResult_Make_E25108F((tupledArg_9) => {
                const __sibI_7 = tupledArg_9[0] | 0;
                const __sibC_7 = tupledArg_9[1] | 0;
                const __pFQN_7 = tupledArg_9[2];
                const __parentFQN_8 = "LibClient.Components.Legacy.Input.Picker";
                let arg00_22;
                const __currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_Input_PickerProps_Static_3FD7D463(periodPickerItems, new SelectedItem$1(1, rawPeriodOffset), new OnChange$1(1, (arg_3) => {
                    props.OnChange(Value__SetPeriod_Z524259A4(props.Value, arg_3[1]));
                }), externalValidityForFields);
                const __currClass_7 = "picker" + nthChildClasses(__sibI_7, __sibC_7);
                const __currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7);
                const __implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty())))));
                arg00_22 = injectImplicitProps(__implicitProps_7, __currExplicitProps_7);
                return Make_3()(arg00_22)([]);
            })]), RenderResult_ToRawElements(__parentFQN_4, __list_3)));
        }), Option_getOrElse(new RenderResult(0), map((reason) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_10) => {
            let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8, __list_5;
            const __sibI_8 = tupledArg_10[0] | 0;
            const __sibC_8 = tupledArg_10[1] | 0;
            const __pFQN_8 = tupledArg_10[2];
            const __parentFQN_9 = "ReactXP.Components.View";
            return Make((__currExplicitProps_8 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_8 = nthChildClasses(__sibI_8, __sibC_8), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))((__list_5 = singleton_1(RenderResult_Make_E25108F((tupledArg_11) => {
                let text_2, __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9;
                const __sibI_9 = tupledArg_11[0] | 0;
                const __sibC_9 = tupledArg_11[1] | 0;
                const __pFQN_9 = tupledArg_11[2];
                const __parentFQN_10 = "LibClient.Components.Text";
                let arg10_9;
                const __list_4 = singleton_1(RenderResult_Make_E25108F((text_2 = format("{0}", reason), (tupledArg_12) => makeTextNode(text_2, tupledArg_12[0], tupledArg_12[1], tupledArg_12[2]))));
                arg10_9 = RenderResult_ToRawElements(__parentFQN_10, __list_4);
                return Make_1((__currExplicitProps_9 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_9 = ("invalid-reason" + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["__style", __currStyles_9]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))(arg10_9);
            })), RenderResult_ToRawElements(__parentFQN_9, __list_5)));
        }))), InputValidity__get_InvalidReason(InputValidity__Or_Z67BF8BF7(Value__get_InternalValidity(props.Value), props.Validity))))]), RenderResult_ToRawElements(__parentFQN_1, __list_6)));
    }))))))));
}

