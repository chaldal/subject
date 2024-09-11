import { defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToLeaves_410EF94B, RenderResult, RenderResult_Make_Z1C694E5A, RenderResult_Make_410EF94B, RenderResult_ToRawElements, makeTextNode, RenderResult_Make_6E3A73D, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../RenderResult.fs.js";
import { Make } from "../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { format } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../RenderHelpers.fs.js";
import { map, empty, singleton, append, delay, toList } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { empty as empty_1, ofSeq, ofArray, singleton as singleton_1, isEmpty } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_1 } from "../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../UiText/UiText.TypeExtensions.fs.js";
import { Make as Make_2 } from "../../../Components/Timestamp/Timestamp.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTimestampProps_Static_Z2F0F0510 } from "../Timestamp/Timestamp.TypeExtensions.fs.js";
import { UniDateTime_Of_Z53C0511E } from "../../../Services/DateService/DateService.fs.js";
import { Actionable, Make as Make_3 } from "../../../Components/IconButton/IconButton.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeIconButtonProps_Static_Z7C2F413F } from "../IconButton/IconButton.TypeExtensions.fs.js";
import { ButtonHighLevelStateFactory_MakeLowLevel_3536472E } from "../../../Input.fs.js";
import { isInsideSelectableRange, Actions__NextMonth_411CDB90, Actions__PrevMonth_411CDB90 } from "../../../Components/DateSelector/DateSelector.typext.fs.js";
import { equals, uncurry } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Icon_get_ChevronRight, Icon_get_ChevronLeft } from "../../../Icons.fs.js";
import { date, day as day_2, month } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/DateOffset.js";
import { Noop } from "../../../../../LibLangFsharp/src/Documentational.fs.js";
import { equals as equals_1 } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Date.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_13;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = (format("{0}{1}{2}", "view ", TopLevelBlockClass, "") + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_13 = ofArray([RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_3, matchValue, __list_2, selectedDate, __list;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = ("header" + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_3 = singleton_1((matchValue = props.MaybeSelected, (matchValue == null) ? RenderResult_Make_6E3A73D((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                const __sibI_3 = tupledArg_3[0] | 0;
                const __sibC_3 = tupledArg_3[1] | 0;
                const __pFQN_3 = tupledArg_3[2];
                const __parentFQN_4 = "LibClient.Components.UiText";
                let arg10_2;
                const __list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => makeTextNode("Select Day", tupledArg_4[0], tupledArg_4[1], tupledArg_4[2])));
                arg10_2 = RenderResult_ToRawElements(__parentFQN_4, __list_1);
                return Make_1((__currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_3 = ("header-text" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["__style", __currStyles_3]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))(arg10_2);
            })), RenderResult_ToRawElements(__parentFQN_2, __list_2))) : ((selectedDate = matchValue, RenderResult_Make_6E3A73D((__list = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
                const __sibI_2 = tupledArg_2[0] | 0;
                const __sibC_2 = tupledArg_2[1] | 0;
                const __pFQN_2 = tupledArg_2[2];
                const __parentFQN_3 = "LibClient.Components.Timestamp";
                return Make_2((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTimestampProps_Static_Z2F0F0510(UniDateTime_Of_Z53C0511E(selectedDate), "ddd, MMM dd"), (__currClass_2 = ("header-text" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))([]);
            })), RenderResult_ToRawElements(__parentFQN_2, __list))))))), RenderResult_ToRawElements(__parentFQN_2, __list_3)));
        }), RenderResult_Make_E25108F((tupledArg_5) => {
            let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4, __list_5;
            const __sibI_4 = tupledArg_5[0] | 0;
            const __sibC_4 = tupledArg_5[1] | 0;
            const __pFQN_4 = tupledArg_5[2];
            const __parentFQN_5 = "ReactXP.Components.View";
            return Make((__currExplicitProps_4 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_4 = ("navigation-controls" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_4)]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))((__list_5 = ofArray([RenderResult_Make_E25108F((tupledArg_6) => {
                let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5;
                const __sibI_5 = tupledArg_6[0] | 0;
                const __sibC_5 = tupledArg_6[1] | 0;
                const __pFQN_5 = tupledArg_6[2];
                const __parentFQN_6 = "LibClient.Components.Timestamp";
                return Make_2((__currExplicitProps_5 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTimestampProps_Static_Z2F0F0510(UniDateTime_Of_Z53C0511E(estate.CurrentMonthFirstDay), "MMMM yyyy"), (__currClass_5 = ("navigation-controls-text" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))([]);
            }), RenderResult_Make_E25108F((tupledArg_7) => {
                let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6, __list_4;
                const __sibI_6 = tupledArg_7[0] | 0;
                const __sibC_6 = tupledArg_7[1] | 0;
                const __pFQN_6 = tupledArg_7[2];
                const __parentFQN_7 = "ReactXP.Components.View";
                return Make((__currExplicitProps_6 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_6 = ("arrow-container" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_6)]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))((__list_4 = ofArray([RenderResult_Make_E25108F((tupledArg_8) => {
                    let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
                    const __sibI_7 = tupledArg_8[0] | 0;
                    const __sibC_7 = tupledArg_8[1] | 0;
                    const __pFQN_7 = tupledArg_8[2];
                    const __parentFQN_8 = "LibClient.Components.IconButton";
                    return Make_3((__currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconButtonProps_Static_Z7C2F413F(ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Actionable((arg00_15) => {
                        Actions__PrevMonth_411CDB90(actions, arg00_15);
                    })), uncurry(2, Icon_get_ChevronLeft())), (__currClass_7 = ("icon-button" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))([]);
                }), RenderResult_Make_E25108F((tupledArg_9) => {
                    let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8;
                    const __sibI_8 = tupledArg_9[0] | 0;
                    const __sibC_8 = tupledArg_9[1] | 0;
                    const __pFQN_8 = tupledArg_9[2];
                    const __parentFQN_9 = "LibClient.Components.IconButton";
                    return Make_3((__currExplicitProps_8 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconButtonProps_Static_Z7C2F413F(ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Actionable((arg00_18) => {
                        Actions__NextMonth_411CDB90(actions, arg00_18);
                    })), uncurry(2, Icon_get_ChevronRight())), (__currClass_8 = ("icon-button" + nthChildClasses(__sibI_8, __sibC_8)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["__style", __currStyles_8]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))([]);
                })]), RenderResult_ToRawElements(__parentFQN_7, __list_4)));
            })]), RenderResult_ToRawElements(__parentFQN_5, __list_5)));
        }), RenderResult_Make_E25108F((tupledArg_10) => {
            let __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9, __list_8;
            const __sibI_9 = tupledArg_10[0] | 0;
            const __sibC_9 = tupledArg_10[1] | 0;
            const __pFQN_9 = tupledArg_10[2];
            const __parentFQN_10 = "ReactXP.Components.View";
            return Make((__currExplicitProps_9 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_9 = ("weekday-headers-row" + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_9)]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))((__list_8 = singleton_1(RenderResult_Make_410EF94B(ofSeq(map((day) => RenderResult_Make_E25108F((tupledArg_11) => {
                let __currExplicitProps_10, __currClass_10, __currStyles_10, __implicitProps_10, __list_7;
                const __sibI_10 = tupledArg_11[0] | 0;
                const __sibC_10 = tupledArg_11[1] | 0;
                const __pFQN_10 = tupledArg_11[2];
                const __parentFQN_11 = "ReactXP.Components.View";
                return Make((__currExplicitProps_10 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_10 = ("weekday-header" + nthChildClasses(__sibI_10, __sibC_10)), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append((__currClass_10 !== "") ? singleton(["ClassName", __currClass_10]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_10)]) : empty()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))((__list_7 = singleton_1(RenderResult_Make_E25108F((tupledArg_12) => {
                    let text_1, __currExplicitProps_11, __currClass_11, __currStyles_11, __implicitProps_11;
                    const __sibI_11 = tupledArg_12[0] | 0;
                    const __sibC_11 = tupledArg_12[1] | 0;
                    const __pFQN_11 = tupledArg_12[2];
                    const __parentFQN_12 = "LibClient.Components.UiText";
                    let arg10_11;
                    const __list_6 = singleton_1(RenderResult_Make_E25108F((text_1 = format("{0}", day), (tupledArg_13) => makeTextNode(text_1, tupledArg_13[0], tupledArg_13[1], tupledArg_13[2]))));
                    arg10_11 = RenderResult_ToRawElements(__parentFQN_12, __list_6);
                    return Make_1((__currExplicitProps_11 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_11 = ("weekday-header-text" + nthChildClasses(__sibI_11, __sibC_11)), (__currStyles_11 = findApplicableStyles(__mergedStyles, __currClass_11), (__implicitProps_11 = toList(delay(() => append((__currClass_11 !== "") ? singleton(["ClassName", __currClass_11]) : empty(), delay(() => ((!isEmpty(__currStyles_11)) ? singleton(["__style", __currStyles_11]) : empty()))))), injectImplicitProps(__implicitProps_11, __currExplicitProps_11))))))(arg10_11);
                })), RenderResult_ToRawElements(__parentFQN_11, __list_7)));
            }), ["S", "M", "T", "W", "T", "F", "S"])))), RenderResult_ToRawElements(__parentFQN_10, __list_8)));
        }), RenderResult_Make_410EF94B(ofSeq(map((row) => RenderResult_Make_E25108F((tupledArg_14) => {
            let __currExplicitProps_12, __currClass_12, __currStyles_12, __implicitProps_12, __list_12;
            const __sibI_12 = tupledArg_14[0] | 0;
            const __sibC_12 = tupledArg_14[1] | 0;
            const __pFQN_12 = tupledArg_14[2];
            const __parentFQN_13 = "ReactXP.Components.View";
            return Make((__currExplicitProps_12 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_12 = ("row" + nthChildClasses(__sibI_12, __sibC_12)), (__currStyles_12 = findApplicableStyles(__mergedStyles, __currClass_12), (__implicitProps_12 = toList(delay(() => append((__currClass_12 !== "") ? singleton(["ClassName", __currClass_12]) : empty(), delay(() => ((!isEmpty(__currStyles_12)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_12)]) : empty()))))), injectImplicitProps(__implicitProps_12, __currExplicitProps_12))))))((__list_12 = singleton_1(RenderResult_Make_410EF94B(ofSeq(map((day_1) => {
                let __list_11, canSelect;
                const isOtherMonth = !(month(day_1) === month(estate.CurrentMonthFirstDay));
                return RenderResult_Make_Z1C694E5A((__list_11 = ofArray([(!isOtherMonth) ? ((canSelect = (!isInsideSelectableRange(day_1, props.MinDate)), RenderResult_Make_E25108F((tupledArg_15) => {
                    let __currExplicitProps_13, __currClass_13, __currStyles_13, __implicitProps_13, __list_10;
                    const __sibI_13 = tupledArg_15[0] | 0;
                    const __sibC_13 = tupledArg_15[1] | 0;
                    const __pFQN_13 = tupledArg_15[2];
                    const __parentFQN_14 = "ReactXP.Components.View";
                    return Make((__currExplicitProps_13 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (_arg1) => {
                        if (canSelect) {
                            props.OnChange(day_1);
                        }
                    }), (__currClass_13 = (("day" + format(" {0}", equals(day_1, props.MaybeSelected) ? "selected" : "")) + nthChildClasses(__sibI_13, __sibC_13)), (__currStyles_13 = findApplicableStyles(__mergedStyles, __currClass_13), (__implicitProps_13 = toList(delay(() => append((__currClass_13 !== "") ? singleton(["ClassName", __currClass_13]) : empty(), delay(() => ((!isEmpty(__currStyles_13)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_13)]) : empty()))))), injectImplicitProps(__implicitProps_13, __currExplicitProps_13))))))((__list_10 = singleton_1(RenderResult_Make_E25108F((tupledArg_16) => {
                        let text_2, __currExplicitProps_14, __currClass_14, __currStyles_14, __implicitProps_14;
                        const __sibI_14 = tupledArg_16[0] | 0;
                        const __sibC_14 = tupledArg_16[1] | 0;
                        const __pFQN_14 = tupledArg_16[2];
                        const __parentFQN_15 = "LibClient.Components.UiText";
                        let arg10_15;
                        const __list_9 = singleton_1(RenderResult_Make_E25108F((text_2 = format("{0}", day_2(day_1)), (tupledArg_17) => makeTextNode(text_2, tupledArg_17[0], tupledArg_17[1], tupledArg_17[2]))));
                        arg10_15 = RenderResult_ToRawElements(__parentFQN_15, __list_9);
                        return Make_1((__currExplicitProps_14 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_14 = (("day-text" + format(" {0} {1}", equals_1(date(day_1), date(estate.CurrentDate)) ? "today" : "", (!canSelect) ? "disabled" : "")) + nthChildClasses(__sibI_14, __sibC_14)), (__currStyles_14 = findApplicableStyles(__mergedStyles, __currClass_14), (__implicitProps_14 = toList(delay(() => append((__currClass_14 !== "") ? singleton(["ClassName", __currClass_14]) : empty(), delay(() => ((!isEmpty(__currStyles_14)) ? singleton(["__style", __currStyles_14]) : empty()))))), injectImplicitProps(__implicitProps_14, __currExplicitProps_14))))))(arg10_15);
                    })), RenderResult_ToRawElements(__parentFQN_14, __list_10)));
                }))) : (new RenderResult(0)), isOtherMonth ? RenderResult_Make_E25108F((tupledArg_18) => {
                    let __currExplicitProps_15, __currClass_15, __currStyles_15, __implicitProps_15;
                    const __sibI_15 = tupledArg_18[0] | 0;
                    const __sibC_15 = tupledArg_18[1] | 0;
                    const __pFQN_15 = tupledArg_18[2];
                    const __parentFQN_16 = "ReactXP.Components.View";
                    return Make((__currExplicitProps_15 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_15 = ("day other-month" + nthChildClasses(__sibI_15, __sibC_15)), (__currStyles_15 = findApplicableStyles(__mergedStyles, __currClass_15), (__implicitProps_15 = toList(delay(() => append((__currClass_15 !== "") ? singleton(["ClassName", __currClass_15]) : empty(), delay(() => ((!isEmpty(__currStyles_15)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_15)]) : empty()))))), injectImplicitProps(__implicitProps_15, __currExplicitProps_15))))))(empty_1());
                }) : (new RenderResult(0))]), RenderResult_ToLeaves_410EF94B(__list_11)));
            }, row)))), RenderResult_ToRawElements(__parentFQN_13, __list_12)));
        }), estate.Rows)))]), RenderResult_ToRawElements(__parentFQN_1, __list_13)));
    })));
}

