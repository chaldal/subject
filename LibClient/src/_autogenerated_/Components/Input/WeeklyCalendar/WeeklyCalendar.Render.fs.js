import { map as map_1, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_410EF94B, RenderResult, RenderResult_ToRawElements, makeTextNode, RenderResult_Make_6E3A73D, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../Components/With/ScreenSize/ScreenSize.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06 } from "../../With/ScreenSize/ScreenSize.TypeExtensions.fs.js";
import { Make as Make_1 } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { substring, format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../../RenderHelpers.fs.js";
import { map, empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, empty as empty_1, ofSeq, singleton as singleton_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { equals } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { InputValidity__get_InvalidReason, InputValidity__get_IsInvalid, InputValidity } from "../../../../Input.fs.js";
import { Make as Make_2 } from "../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../Text/Text.TypeExtensions.fs.js";
import { Make as Make_3 } from "../../../../ReactXP/Components/ScrollView/ScrollView.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5 } from "../../../ReactXP/Components/ScrollView/ScrollView.TypeExtensions.fs.js";
import { generateWeek } from "../../../../Components/Input/WeeklyCalendar/WeeklyCalendar.typext.fs.js";
import { ScreenSize__get_Class } from "../../../../Responsive.fs.js";
import { name, getUnionFields } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Reflection.js";
import { Date__get_Day, DayOfTheWeek$reflection, Date__get_DayOfTheWeek } from "../../../../../../LibLangFsharp/src/DateTimeExtensions.fs.js";
import { contains } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Set.js";
import { Make as Make_4 } from "../../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../../UiText/UiText.TypeExtensions.fs.js";
import { Make as Make_5 } from "../../../../Components/TapCapture/TapCapture.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6 } from "../../TapCapture/TapCapture.TypeExtensions.fs.js";
import { Microsoft_FSharp_Collections_FSharpSet$1__Set$1_Toggle_Z63A0D504 } from "../../../../../../LibLangFsharp/src/SetExtensions.fs.js";
import { Option_getOrElse } from "../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import * as react from "react";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps_16, __currClass_16, __currStyles_16, __implicitProps_16;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.With.ScreenSize";
        return Make((__currExplicitProps_16 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06((screenSize) => {
            let __list_16;
            const children = (__list_16 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
                let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_15;
                const __sibI_1 = tupledArg_1[0] | 0;
                const __sibC_1 = tupledArg_1[1] | 0;
                const __pFQN_1 = tupledArg_1[2];
                const __parentFQN_2 = "ReactXP.Components.View";
                return Make_1((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = (format("{0}{1}{2}", "view ", TopLevelBlockClass, "") + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_15 = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                    let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_14, matchValue, __list_5, label, __list_2;
                    const __sibI_2 = tupledArg_2[0] | 0;
                    const __sibC_2 = tupledArg_2[1] | 0;
                    const __pFQN_2 = tupledArg_2[2];
                    const __parentFQN_3 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = ("weekly-calendar-container" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_14 = ofArray([(matchValue = props.Label, (matchValue == null) ? RenderResult_Make_6E3A73D((__list_5 = singleton_1(equals(props.Validity, new InputValidity(1)) ? RenderResult_Make_E25108F((tupledArg_6) => {
                        let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4, __list_4;
                        const __sibI_5 = tupledArg_6[0] | 0;
                        const __sibC_5 = tupledArg_6[1] | 0;
                        const __pFQN_5 = tupledArg_6[2];
                        const __parentFQN_6 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_4 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_4 = ("label invalid" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_4)]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))((__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_7) => {
                            let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5;
                            const __sibI_6 = tupledArg_7[0] | 0;
                            const __sibC_6 = tupledArg_7[1] | 0;
                            const __pFQN_6 = tupledArg_7[2];
                            const __parentFQN_7 = "LibClient.Components.Text";
                            let arg10_5;
                            const __list_3 = singleton_1(RenderResult_Make_E25108F((tupledArg_8) => makeTextNode("Required", tupledArg_8[0], tupledArg_8[1], tupledArg_8[2])));
                            arg10_5 = RenderResult_ToRawElements(__parentFQN_7, __list_3);
                            return Make_2((__currExplicitProps_5 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_5 = ("label-text invalid" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))(arg10_5);
                        })), RenderResult_ToRawElements(__parentFQN_6, __list_4)));
                    }) : (new RenderResult(0))), RenderResult_ToRawElements(__parentFQN_3, __list_5))) : ((label = matchValue, RenderResult_Make_6E3A73D((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                        let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list_1;
                        const __sibI_3 = tupledArg_3[0] | 0;
                        const __sibC_3 = tupledArg_3[1] | 0;
                        const __pFQN_3 = tupledArg_3[2];
                        const __parentFQN_4 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_2 = ("label" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
                            let text, __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                            const __sibI_4 = tupledArg_4[0] | 0;
                            const __sibC_4 = tupledArg_4[1] | 0;
                            const __pFQN_4 = tupledArg_4[2];
                            const __parentFQN_5 = "LibClient.Components.Text";
                            let arg10_1;
                            const __list = singleton_1(RenderResult_Make_E25108F((text = format("{0}", label), (tupledArg_5) => makeTextNode(text, tupledArg_5[0], tupledArg_5[1], tupledArg_5[2]))));
                            arg10_1 = RenderResult_ToRawElements(__parentFQN_5, __list);
                            return Make_2((__currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_3 = (("label-text" + format(" {0}", InputValidity__get_IsInvalid(props.Validity) ? "invalid" : "")) + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["__style", __currStyles_3]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))(arg10_1);
                        })), RenderResult_ToRawElements(__parentFQN_4, __list_1)));
                    })), RenderResult_ToRawElements(__parentFQN_3, __list_2)))))), RenderResult_Make_E25108F((tupledArg_9) => {
                        let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6, __list_11, days;
                        const __sibI_7 = tupledArg_9[0] | 0;
                        const __sibC_7 = tupledArg_9[1] | 0;
                        const __pFQN_7 = tupledArg_9[2];
                        const __parentFQN_8 = "ReactXP.Components.ScrollView";
                        return Make_3((__currExplicitProps_6 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5(void 0, void 0, true), (__currClass_6 = nthChildClasses(__sibI_7, __sibC_7), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ScrollView", __currStyles_6)]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))((__list_11 = singleton_1((days = generateWeek(estate.StartDate), RenderResult_Make_E25108F((tupledArg_10) => {
                            let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7, __list_10;
                            const __sibI_8 = tupledArg_10[0] | 0;
                            const __sibC_8 = tupledArg_10[1] | 0;
                            const __pFQN_8 = tupledArg_10[2];
                            const __parentFQN_9 = "ReactXP.Components.View";
                            return Make_1((__currExplicitProps_7 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_7 = (format("{0}{1}{2}", "weekly-calendar ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_8, __sibC_8)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_7)]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))((__list_10 = singleton_1(RenderResult_Make_410EF94B(ofSeq(map((day) => RenderResult_Make_E25108F((tupledArg_11) => {
                                let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8, __list_9, isSelected;
                                const __sibI_9 = tupledArg_11[0] | 0;
                                const __sibC_9 = tupledArg_11[1] | 0;
                                const __pFQN_9 = tupledArg_11[2];
                                const __parentFQN_10 = "ReactXP.Components.View";
                                return Make_1((__currExplicitProps_8 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_8 = (format("{0}{1}{2}", "day ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))((__list_9 = ofArray([RenderResult_Make_E25108F((tupledArg_12) => {
                                    let text_2, case$_1, __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9;
                                    const __sibI_10 = tupledArg_12[0] | 0;
                                    const __sibC_10 = tupledArg_12[1] | 0;
                                    const __pFQN_10 = tupledArg_12[2];
                                    const __parentFQN_11 = "LibClient.Components.Text";
                                    let arg10_9;
                                    const __list_6 = singleton_1(RenderResult_Make_E25108F((text_2 = format("{0}", substring((case$_1 = getUnionFields(Date__get_DayOfTheWeek(day), DayOfTheWeek$reflection())[0], name(case$_1)), 0, 3)), (tupledArg_13) => makeTextNode(text_2, tupledArg_13[0], tupledArg_13[1], tupledArg_13[2]))));
                                    arg10_9 = RenderResult_ToRawElements(__parentFQN_11, __list_6);
                                    return Make_2((__currExplicitProps_9 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_9 = ("day-of-week" + nthChildClasses(__sibI_10, __sibC_10)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["__style", __currStyles_9]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))(arg10_9);
                                }), (isSelected = contains(day, props.Value), RenderResult_Make_E25108F((tupledArg_14) => {
                                    let __currExplicitProps_10, __currClass_10, __currStyles_10, __implicitProps_10, __list_8;
                                    const __sibI_11 = tupledArg_14[0] | 0;
                                    const __sibC_11 = tupledArg_14[1] | 0;
                                    const __pFQN_11 = tupledArg_14[2];
                                    const __parentFQN_12 = "ReactXP.Components.View";
                                    return Make_1((__currExplicitProps_10 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_10 = ("date" + nthChildClasses(__sibI_11, __sibC_11)), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append((__currClass_10 !== "") ? singleton(["ClassName", __currClass_10]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_10)]) : empty()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))((__list_8 = ofArray([isSelected ? RenderResult_Make_E25108F((tupledArg_15) => {
                                        let __currExplicitProps_11, __currClass_11, __currStyles_11, __implicitProps_11;
                                        const __sibI_12 = tupledArg_15[0] | 0;
                                        const __sibC_12 = tupledArg_15[1] | 0;
                                        const __pFQN_12 = tupledArg_15[2];
                                        const __parentFQN_13 = "ReactXP.Components.View";
                                        return Make_1((__currExplicitProps_11 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_11 = (format("{0}{1}{2}", "circle ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_12, __sibC_12)), (__currStyles_11 = findApplicableStyles(__mergedStyles, __currClass_11), (__implicitProps_11 = toList(delay(() => append((__currClass_11 !== "") ? singleton(["ClassName", __currClass_11]) : empty(), delay(() => ((!isEmpty(__currStyles_11)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_11)]) : empty()))))), injectImplicitProps(__implicitProps_11, __currExplicitProps_11))))))(empty_1());
                                    }) : (new RenderResult(0)), RenderResult_Make_E25108F((tupledArg_16) => {
                                        let text_3, __currExplicitProps_12, __currClass_12, __currStyles_12, __implicitProps_12;
                                        const __sibI_13 = tupledArg_16[0] | 0;
                                        const __sibC_13 = tupledArg_16[1] | 0;
                                        const __pFQN_13 = tupledArg_16[2];
                                        const __parentFQN_14 = "LibClient.Components.UiText";
                                        let arg10_11;
                                        const __list_7 = singleton_1(RenderResult_Make_E25108F((text_3 = format("{0}", Date__get_Day(day)), (tupledArg_17) => makeTextNode(text_3, tupledArg_17[0], tupledArg_17[1], tupledArg_17[2]))));
                                        arg10_11 = RenderResult_ToRawElements(__parentFQN_14, __list_7);
                                        return Make_4((__currExplicitProps_12 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_12 = (("date-text" + format(" {0}", isSelected ? "selected" : "")) + nthChildClasses(__sibI_13, __sibC_13)), (__currStyles_12 = findApplicableStyles(__mergedStyles, __currClass_12), (__implicitProps_12 = toList(delay(() => append((__currClass_12 !== "") ? singleton(["ClassName", __currClass_12]) : empty(), delay(() => ((!isEmpty(__currStyles_12)) ? singleton(["__style", __currStyles_12]) : empty()))))), injectImplicitProps(__implicitProps_12, __currExplicitProps_12))))))(arg10_11);
                                    })]), RenderResult_ToRawElements(__parentFQN_12, __list_8)));
                                })), RenderResult_Make_E25108F((tupledArg_18) => {
                                    let __currExplicitProps_13, __currClass_13, __currStyles_13, __implicitProps_13;
                                    const __sibI_14 = tupledArg_18[0] | 0;
                                    const __sibC_14 = tupledArg_18[1] | 0;
                                    const __pFQN_14 = tupledArg_18[2];
                                    const __parentFQN_15 = "LibClient.Components.TapCapture";
                                    return Make_5((__currExplicitProps_13 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6((_arg1) => {
                                        props.OnChange(Microsoft_FSharp_Collections_FSharpSet$1__Set$1_Toggle_Z63A0D504(props.Value, day));
                                    }), (__currClass_13 = nthChildClasses(__sibI_14, __sibC_14), (__currStyles_13 = findApplicableStyles(__mergedStyles, __currClass_13), (__implicitProps_13 = toList(delay(() => append((__currClass_13 !== "") ? singleton(["ClassName", __currClass_13]) : empty(), delay(() => ((!isEmpty(__currStyles_13)) ? singleton(["__style", __currStyles_13]) : empty()))))), injectImplicitProps(__implicitProps_13, __currExplicitProps_13))))))([]);
                                })]), RenderResult_ToRawElements(__parentFQN_10, __list_9)));
                            }), days)))), RenderResult_ToRawElements(__parentFQN_9, __list_10)));
                        }))), RenderResult_ToRawElements(__parentFQN_8, __list_11)));
                    }), Option_getOrElse(new RenderResult(0), map_1((reason) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_19) => {
                        let __currExplicitProps_14, __currClass_14, __currStyles_14, __implicitProps_14, __list_13;
                        const __sibI_15 = tupledArg_19[0] | 0;
                        const __sibC_15 = tupledArg_19[1] | 0;
                        const __pFQN_15 = tupledArg_19[2];
                        const __parentFQN_16 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_14 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_14 = nthChildClasses(__sibI_15, __sibC_15), (__currStyles_14 = findApplicableStyles(__mergedStyles, __currClass_14), (__implicitProps_14 = toList(delay(() => append((__currClass_14 !== "") ? singleton(["ClassName", __currClass_14]) : empty(), delay(() => ((!isEmpty(__currStyles_14)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_14)]) : empty()))))), injectImplicitProps(__implicitProps_14, __currExplicitProps_14))))))((__list_13 = singleton_1(RenderResult_Make_E25108F((tupledArg_20) => {
                            let text_4, __currExplicitProps_15, __currClass_15, __currStyles_15, __implicitProps_15;
                            const __sibI_16 = tupledArg_20[0] | 0;
                            const __sibC_16 = tupledArg_20[1] | 0;
                            const __pFQN_16 = tupledArg_20[2];
                            const __parentFQN_17 = "LibClient.Components.Text";
                            let arg10_18;
                            const __list_12 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}", reason), (tupledArg_21) => makeTextNode(text_4, tupledArg_21[0], tupledArg_21[1], tupledArg_21[2]))));
                            arg10_18 = RenderResult_ToRawElements(__parentFQN_17, __list_12);
                            return Make_2((__currExplicitProps_15 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_15 = ("invalid-reason" + nthChildClasses(__sibI_16, __sibC_16)), (__currStyles_15 = findApplicableStyles(__mergedStyles, __currClass_15), (__implicitProps_15 = toList(delay(() => append((__currClass_15 !== "") ? singleton(["ClassName", __currClass_15]) : empty(), delay(() => ((!isEmpty(__currStyles_15)) ? singleton(["__style", __currStyles_15]) : empty()))))), injectImplicitProps(__implicitProps_15, __currExplicitProps_15))))))(arg10_18);
                        })), RenderResult_ToRawElements(__parentFQN_16, __list_13)));
                    }))), InputValidity__get_InvalidReason(props.Validity)))]), RenderResult_ToRawElements(__parentFQN_3, __list_14)));
                })), RenderResult_ToRawElements(__parentFQN_2, __list_15)));
            })), RenderResult_ToRawElements(__parentFQN_1, __list_16));
            return react.createElement(react.Fragment, {}, ...children);
        }), (__currClass_16 = nthChildClasses(__sibI, __sibC), (__currStyles_16 = findApplicableStyles(__mergedStyles, __currClass_16), (__implicitProps_16 = toList(delay(() => append((__currClass_16 !== "") ? singleton(["ClassName", __currClass_16]) : empty(), delay(() => ((!isEmpty(__currStyles_16)) ? singleton(["__style", __currStyles_16]) : empty()))))), injectImplicitProps(__implicitProps_16, __currExplicitProps_16))))))([]);
    })));
}

