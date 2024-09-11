import { map, defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToLeaves_410EF94B, RenderResult_Make_410EF94B, RenderResult, makeTextNode, RenderResult_ToRawElements, RenderResult_Make_6E3A73D, nthChildClasses, RenderResult_Make_Z1C694E5A, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../RenderResult.fs.js";
import { Make } from "../../../../../Components/With/ScreenSize/ScreenSize.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06 } from "../../../With/ScreenSize/ScreenSize.TypeExtensions.fs.js";
import { Make as Make_1 } from "../../../../../Components/Pointer/State/State.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakePointer_StateProps_Static_76B68C13 } from "../../../Pointer/State/State.TypeExtensions.fs.js";
import { State__get_Name } from "../../../../../Components/Nav/Top/Item/Item.typext.fs.js";
import { format, printf, toText } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { Make as Make_2 } from "../../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { TopLevelBlockClass } from "../../../../../RenderHelpers.fs.js";
import { ScreenSize__get_Class } from "../../../../../Responsive.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, singleton as singleton_1, isEmpty } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { uncurry, curry } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Make as Make_3 } from "../../../../../Components/Icon/Icon.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B } from "../../../Icon/Icon.TypeExtensions.fs.js";
import { Make as Make_4 } from "../../../../../Components/Badge/Badge.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeBadgeProps_Static_ZE5CF26C } from "../../../Badge/Badge.TypeExtensions.fs.js";
import { Make as Make_5 } from "../../../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../../../UiText/UiText.TypeExtensions.fs.js";
import { Option_getOrElse } from "../../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { Make as Make_6 } from "../../../../../Components/TapCapture/TapCapture.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6 } from "../../../TapCapture/TapCapture.TypeExtensions.fs.js";
import * as react from "react";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps_33, __currClass_33, __currStyles_33, __implicitProps_33;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.With.ScreenSize";
        return Make((__currExplicitProps_33 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06((screenSize) => {
            let __list_33;
            const children_1 = (__list_33 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
                let __currExplicitProps_32, __currClass_32, __currStyles_32, __implicitProps_32;
                const __sibI_1 = tupledArg_1[0] | 0;
                const __sibC_1 = tupledArg_1[1] | 0;
                const __pFQN_1 = tupledArg_1[2];
                const __parentFQN_2 = "LibClient.Components.Pointer.State";
                return Make_1((__currExplicitProps_32 = LibClient_Components_TypeExtensions_TEs__TEs_MakePointer_StateProps_Static_76B68C13((pointerState) => {
                    let __list_32, stateClass, isSelected, matchValue, selectedClass, hoveredClass, depressedClass, sharedClassSet, __list_31;
                    const children = (__list_32 = singleton_1((stateClass = ("state-" + State__get_Name(props.State)), (isSelected = ((matchValue = props.State, (matchValue.tag === 3) ? true : (matchValue.tag === 4))), (selectedClass = (isSelected ? "selected" : ""), (hoveredClass = ((pointerState.IsHovered && (!pointerState.IsDepressed)) ? "hovered" : ""), (depressedClass = (pointerState.IsDepressed ? "depressed" : ""), (sharedClassSet = toText(printf("%s %s %s %s"))(stateClass)(selectedClass)(hoveredClass)(depressedClass), RenderResult_Make_Z1C694E5A((__list_31 = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_30, matchValue_1, icon_3, __list_27, badge_1, icon_2, __list_14, __list_29, label_3, __list_24, badge_2, label_2, __list_19, icon_1, label_1, __list_11, badge, icon, label, __list_5, matchValue_2, onPress;
                        const __sibI_2 = tupledArg_2[0] | 0;
                        const __sibC_2 = tupledArg_2[1] | 0;
                        const __pFQN_2 = tupledArg_2[2];
                        const __parentFQN_3 = "ReactXP.Components.View";
                        return Make_2((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = (format("{0}{1}{2}{3}{4}{5}{6}", "item ", sharedClassSet, " ", TopLevelBlockClass, " ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_30 = ofArray([(matchValue_1 = props.Style, (matchValue_1.fields[0] == null) ? ((curry(2, matchValue_1.fields[1]) != null) ? ((matchValue_1.fields[2] == null) ? ((icon_3 = curry(2, matchValue_1.fields[1]), RenderResult_Make_6E3A73D((__list_27 = singleton_1(RenderResult_Make_E25108F((tupledArg_37) => {
                            let __currExplicitProps_27, __currClass_27, __currStyles_27, __implicitProps_27, __list_26;
                            const __sibI_29 = tupledArg_37[0] | 0;
                            const __sibC_29 = tupledArg_37[1] | 0;
                            const __pFQN_29 = tupledArg_37[2];
                            const __parentFQN_30 = "ReactXP.Components.View";
                            return Make_2((__currExplicitProps_27 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_27 = ("item-content-container" + nthChildClasses(__sibI_29, __sibC_29)), (__currStyles_27 = findApplicableStyles(__mergedStyles, __currClass_27), (__implicitProps_27 = toList(delay(() => append((__currClass_27 !== "") ? singleton(["ClassName", __currClass_27]) : empty(), delay(() => ((!isEmpty(__currStyles_27)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_27)]) : empty()))))), injectImplicitProps(__implicitProps_27, __currExplicitProps_27))))))((__list_26 = singleton_1(RenderResult_Make_E25108F((tupledArg_38) => {
                                let __currExplicitProps_28, __currClass_28, __currStyles_28, __implicitProps_28, __list_25;
                                const __sibI_30 = tupledArg_38[0] | 0;
                                const __sibC_30 = tupledArg_38[1] | 0;
                                const __pFQN_30 = tupledArg_38[2];
                                const __parentFQN_31 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_28 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_28 = ("adjust-icon-vertical-position" + nthChildClasses(__sibI_30, __sibC_30)), (__currStyles_28 = findApplicableStyles(__mergedStyles, __currClass_28), (__implicitProps_28 = toList(delay(() => append((__currClass_28 !== "") ? singleton(["ClassName", __currClass_28]) : empty(), delay(() => ((!isEmpty(__currStyles_28)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_28)]) : empty()))))), injectImplicitProps(__implicitProps_28, __currExplicitProps_28))))))((__list_25 = singleton_1(RenderResult_Make_E25108F((tupledArg_39) => {
                                    let __currExplicitProps_29, __currClass_29, __currStyles_29, __implicitProps_29;
                                    const __sibI_31 = tupledArg_39[0] | 0;
                                    const __sibC_31 = tupledArg_39[1] | 0;
                                    const __pFQN_31 = tupledArg_39[2];
                                    const __parentFQN_32 = "LibClient.Components.Icon";
                                    return Make_3((__currExplicitProps_29 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, icon_3)), (__currClass_29 = (format("{0}{1}{2}{3}{4}", "icon ", sharedClassSet, " ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_31, __sibC_31)), (__currStyles_29 = findApplicableStyles(__mergedStyles, __currClass_29), (__implicitProps_29 = toList(delay(() => append((__currClass_29 !== "") ? singleton(["ClassName", __currClass_29]) : empty(), delay(() => ((!isEmpty(__currStyles_29)) ? singleton(["__style", __currStyles_29]) : empty()))))), injectImplicitProps(__implicitProps_29, __currExplicitProps_29))))))([]);
                                })), RenderResult_ToRawElements(__parentFQN_31, __list_25)));
                            })), RenderResult_ToRawElements(__parentFQN_30, __list_26)));
                        })), RenderResult_ToRawElements(__parentFQN_3, __list_27))))) : ((badge_1 = matchValue_1.fields[2], (icon_2 = curry(2, matchValue_1.fields[1]), RenderResult_Make_6E3A73D((__list_14 = singleton_1(RenderResult_Make_E25108F((tupledArg_20) => {
                            let __currExplicitProps_14, __currClass_14, __currStyles_14, __implicitProps_14, __list_13;
                            const __sibI_16 = tupledArg_20[0] | 0;
                            const __sibC_16 = tupledArg_20[1] | 0;
                            const __pFQN_16 = tupledArg_20[2];
                            const __parentFQN_17 = "ReactXP.Components.View";
                            return Make_2((__currExplicitProps_14 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_14 = ("item-content-container item-content-container-with-badge" + nthChildClasses(__sibI_16, __sibC_16)), (__currStyles_14 = findApplicableStyles(__mergedStyles, __currClass_14), (__implicitProps_14 = toList(delay(() => append((__currClass_14 !== "") ? singleton(["ClassName", __currClass_14]) : empty(), delay(() => ((!isEmpty(__currStyles_14)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_14)]) : empty()))))), injectImplicitProps(__implicitProps_14, __currExplicitProps_14))))))((__list_13 = ofArray([RenderResult_Make_E25108F((tupledArg_21) => {
                                let __currExplicitProps_15, __currClass_15, __currStyles_15, __implicitProps_15, __list_12;
                                const __sibI_17 = tupledArg_21[0] | 0;
                                const __sibC_17 = tupledArg_21[1] | 0;
                                const __pFQN_17 = tupledArg_21[2];
                                const __parentFQN_18 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_15 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_15 = ("adjust-icon-vertical-position" + nthChildClasses(__sibI_17, __sibC_17)), (__currStyles_15 = findApplicableStyles(__mergedStyles, __currClass_15), (__implicitProps_15 = toList(delay(() => append((__currClass_15 !== "") ? singleton(["ClassName", __currClass_15]) : empty(), delay(() => ((!isEmpty(__currStyles_15)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_15)]) : empty()))))), injectImplicitProps(__implicitProps_15, __currExplicitProps_15))))))((__list_12 = singleton_1(RenderResult_Make_E25108F((tupledArg_22) => {
                                    let __currExplicitProps_16, __currClass_16, __currStyles_16, __implicitProps_16;
                                    const __sibI_18 = tupledArg_22[0] | 0;
                                    const __sibC_18 = tupledArg_22[1] | 0;
                                    const __pFQN_18 = tupledArg_22[2];
                                    const __parentFQN_19 = "LibClient.Components.Icon";
                                    return Make_3((__currExplicitProps_16 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, icon_2)), (__currClass_16 = (format("{0}{1}{2}{3}{4}", "icon ", sharedClassSet, " ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_18, __sibC_18)), (__currStyles_16 = findApplicableStyles(__mergedStyles, __currClass_16), (__implicitProps_16 = toList(delay(() => append((__currClass_16 !== "") ? singleton(["ClassName", __currClass_16]) : empty(), delay(() => ((!isEmpty(__currStyles_16)) ? singleton(["__style", __currStyles_16]) : empty()))))), injectImplicitProps(__implicitProps_16, __currExplicitProps_16))))))([]);
                                })), RenderResult_ToRawElements(__parentFQN_18, __list_12)));
                            }), RenderResult_Make_E25108F((tupledArg_23) => {
                                let __currExplicitProps_17, __currClass_17, __currStyles_17, __implicitProps_17;
                                const __sibI_19 = tupledArg_23[0] | 0;
                                const __sibC_19 = tupledArg_23[1] | 0;
                                const __pFQN_19 = tupledArg_23[2];
                                const __parentFQN_20 = "LibClient.Components.Badge";
                                return Make_4((__currExplicitProps_17 = LibClient_Components_TypeExtensions_TEs__TEs_MakeBadgeProps_Static_ZE5CF26C(badge_1), (__currClass_17 = (format("{0}{1}{2}", "badge ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_19, __sibC_19)), (__currStyles_17 = findApplicableStyles(__mergedStyles, __currClass_17), (__implicitProps_17 = toList(delay(() => append((__currClass_17 !== "") ? singleton(["ClassName", __currClass_17]) : empty(), delay(() => ((!isEmpty(__currStyles_17)) ? singleton(["__style", __currStyles_17]) : empty()))))), injectImplicitProps(__implicitProps_17, __currExplicitProps_17))))))([]);
                            })]), RenderResult_ToRawElements(__parentFQN_17, __list_13)));
                        })), RenderResult_ToRawElements(__parentFQN_3, __list_14))))))) : RenderResult_Make_6E3A73D((__list_29 = singleton_1(RenderResult_Make_E25108F((tupledArg_40) => {
                            let __currExplicitProps_30, __currClass_30, __currStyles_30, __implicitProps_30;
                            const __sibI_32 = tupledArg_40[0] | 0;
                            const __sibC_32 = tupledArg_40[1] | 0;
                            const __pFQN_32 = tupledArg_40[2];
                            const __parentFQN_33 = "LibClient.Components.UiText";
                            let arg10_46;
                            const __list_28 = singleton_1(RenderResult_Make_E25108F((tupledArg_41) => makeTextNode("combination not supported", tupledArg_41[0], tupledArg_41[1], tupledArg_41[2])));
                            arg10_46 = RenderResult_ToRawElements(__parentFQN_33, __list_28);
                            return Make_5((__currExplicitProps_30 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_30 = nthChildClasses(__sibI_32, __sibC_32), (__currStyles_30 = findApplicableStyles(__mergedStyles, __currClass_30), (__implicitProps_30 = toList(delay(() => append((__currClass_30 !== "") ? singleton(["ClassName", __currClass_30]) : empty(), delay(() => ((!isEmpty(__currStyles_30)) ? singleton(["__style", __currStyles_30]) : empty()))))), injectImplicitProps(__implicitProps_30, __currExplicitProps_30))))))(arg10_46);
                        })), RenderResult_ToRawElements(__parentFQN_3, __list_29)))) : ((curry(2, matchValue_1.fields[1]) == null) ? ((matchValue_1.fields[2] == null) ? ((label_3 = matchValue_1.fields[0], RenderResult_Make_6E3A73D((__list_24 = singleton_1(RenderResult_Make_E25108F((tupledArg_31) => {
                            let __currExplicitProps_23, __currClass_23, __currStyles_23, __implicitProps_23, __list_23;
                            const __sibI_25 = tupledArg_31[0] | 0;
                            const __sibC_25 = tupledArg_31[1] | 0;
                            const __pFQN_25 = tupledArg_31[2];
                            const __parentFQN_26 = "ReactXP.Components.View";
                            return Make_2((__currExplicitProps_23 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_23 = ("item-content-container" + nthChildClasses(__sibI_25, __sibC_25)), (__currStyles_23 = findApplicableStyles(__mergedStyles, __currClass_23), (__implicitProps_23 = toList(delay(() => append((__currClass_23 !== "") ? singleton(["ClassName", __currClass_23]) : empty(), delay(() => ((!isEmpty(__currStyles_23)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_23)]) : empty()))))), injectImplicitProps(__implicitProps_23, __currExplicitProps_23))))))((__list_23 = singleton_1(RenderResult_Make_E25108F((tupledArg_32) => {
                                let __currExplicitProps_24, __currClass_24, __currStyles_24, __implicitProps_24, __list_22;
                                const __sibI_26 = tupledArg_32[0] | 0;
                                const __sibC_26 = tupledArg_32[1] | 0;
                                const __pFQN_26 = tupledArg_32[2];
                                const __parentFQN_27 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_24 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_24 = ("label-content" + nthChildClasses(__sibI_26, __sibC_26)), (__currStyles_24 = findApplicableStyles(__mergedStyles, __currClass_24), (__implicitProps_24 = toList(delay(() => append((__currClass_24 !== "") ? singleton(["ClassName", __currClass_24]) : empty(), delay(() => ((!isEmpty(__currStyles_24)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_24)]) : empty()))))), injectImplicitProps(__implicitProps_24, __currExplicitProps_24))))))((__list_22 = ofArray([RenderResult_Make_E25108F((tupledArg_33) => {
                                    let text_6, __currExplicitProps_25, __currClass_25, __currStyles_25, __implicitProps_25;
                                    const __sibI_27 = tupledArg_33[0] | 0;
                                    const __sibC_27 = tupledArg_33[1] | 0;
                                    const __pFQN_27 = tupledArg_33[2];
                                    const __parentFQN_28 = "LibClient.Components.UiText";
                                    let arg10_35;
                                    const __list_20 = singleton_1(RenderResult_Make_E25108F((text_6 = format("{0}", label_3), (tupledArg_34) => makeTextNode(text_6, tupledArg_34[0], tupledArg_34[1], tupledArg_34[2]))));
                                    arg10_35 = RenderResult_ToRawElements(__parentFQN_28, __list_20);
                                    return Make_5((__currExplicitProps_25 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_25 = (format("{0}{1}{2}{3}{4}", "", sharedClassSet, " ", ScreenSize__get_Class(screenSize), " label-sentinel") + nthChildClasses(__sibI_27, __sibC_27)), (__currStyles_25 = findApplicableStyles(__mergedStyles, __currClass_25), (__implicitProps_25 = toList(delay(() => append((__currClass_25 !== "") ? singleton(["ClassName", __currClass_25]) : empty(), delay(() => ((!isEmpty(__currStyles_25)) ? singleton(["__style", __currStyles_25]) : empty()))))), injectImplicitProps(__implicitProps_25, __currExplicitProps_25))))))(arg10_35);
                                }), RenderResult_Make_E25108F((tupledArg_35) => {
                                    let text_7, __currExplicitProps_26, __currClass_26, __currStyles_26, __implicitProps_26;
                                    const __sibI_28 = tupledArg_35[0] | 0;
                                    const __sibC_28 = tupledArg_35[1] | 0;
                                    const __pFQN_28 = tupledArg_35[2];
                                    const __parentFQN_29 = "LibClient.Components.UiText";
                                    let arg10_37;
                                    const __list_21 = singleton_1(RenderResult_Make_E25108F((text_7 = format("{0}", label_3), (tupledArg_36) => makeTextNode(text_7, tupledArg_36[0], tupledArg_36[1], tupledArg_36[2]))));
                                    arg10_37 = RenderResult_ToRawElements(__parentFQN_29, __list_21);
                                    return Make_5((__currExplicitProps_26 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_26 = (format("{0}{1}{2}{3}{4}", "", sharedClassSet, " ", ScreenSize__get_Class(screenSize), " label") + nthChildClasses(__sibI_28, __sibC_28)), (__currStyles_26 = findApplicableStyles(__mergedStyles, __currClass_26), (__implicitProps_26 = toList(delay(() => append((__currClass_26 !== "") ? singleton(["ClassName", __currClass_26]) : empty(), delay(() => ((!isEmpty(__currStyles_26)) ? singleton(["__style", __currStyles_26]) : empty()))))), injectImplicitProps(__implicitProps_26, __currExplicitProps_26))))))(arg10_37);
                                })]), RenderResult_ToRawElements(__parentFQN_27, __list_22)));
                            })), RenderResult_ToRawElements(__parentFQN_26, __list_23)));
                        })), RenderResult_ToRawElements(__parentFQN_3, __list_24))))) : ((badge_2 = matchValue_1.fields[2], (label_2 = matchValue_1.fields[0], RenderResult_Make_6E3A73D((__list_19 = singleton_1(RenderResult_Make_E25108F((tupledArg_24) => {
                            let __currExplicitProps_18, __currClass_18, __currStyles_18, __implicitProps_18, __list_18;
                            const __sibI_20 = tupledArg_24[0] | 0;
                            const __sibC_20 = tupledArg_24[1] | 0;
                            const __pFQN_20 = tupledArg_24[2];
                            const __parentFQN_21 = "ReactXP.Components.View";
                            return Make_2((__currExplicitProps_18 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_18 = ("item-content-container item-content-container-with-badge" + nthChildClasses(__sibI_20, __sibC_20)), (__currStyles_18 = findApplicableStyles(__mergedStyles, __currClass_18), (__implicitProps_18 = toList(delay(() => append((__currClass_18 !== "") ? singleton(["ClassName", __currClass_18]) : empty(), delay(() => ((!isEmpty(__currStyles_18)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_18)]) : empty()))))), injectImplicitProps(__implicitProps_18, __currExplicitProps_18))))))((__list_18 = ofArray([RenderResult_Make_E25108F((tupledArg_25) => {
                                let __currExplicitProps_19, __currClass_19, __currStyles_19, __implicitProps_19, __list_17;
                                const __sibI_21 = tupledArg_25[0] | 0;
                                const __sibC_21 = tupledArg_25[1] | 0;
                                const __pFQN_21 = tupledArg_25[2];
                                const __parentFQN_22 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_19 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_19 = ("label-content" + nthChildClasses(__sibI_21, __sibC_21)), (__currStyles_19 = findApplicableStyles(__mergedStyles, __currClass_19), (__implicitProps_19 = toList(delay(() => append((__currClass_19 !== "") ? singleton(["ClassName", __currClass_19]) : empty(), delay(() => ((!isEmpty(__currStyles_19)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_19)]) : empty()))))), injectImplicitProps(__implicitProps_19, __currExplicitProps_19))))))((__list_17 = ofArray([RenderResult_Make_E25108F((tupledArg_26) => {
                                    let text_4, __currExplicitProps_20, __currClass_20, __currStyles_20, __implicitProps_20;
                                    const __sibI_22 = tupledArg_26[0] | 0;
                                    const __sibC_22 = tupledArg_26[1] | 0;
                                    const __pFQN_22 = tupledArg_26[2];
                                    const __parentFQN_23 = "LibClient.Components.UiText";
                                    let arg10_27;
                                    const __list_15 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}", label_2), (tupledArg_27) => makeTextNode(text_4, tupledArg_27[0], tupledArg_27[1], tupledArg_27[2]))));
                                    arg10_27 = RenderResult_ToRawElements(__parentFQN_23, __list_15);
                                    return Make_5((__currExplicitProps_20 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_20 = (format("{0}{1}{2}{3}{4}", "", sharedClassSet, " ", ScreenSize__get_Class(screenSize), " label-sentinel") + nthChildClasses(__sibI_22, __sibC_22)), (__currStyles_20 = findApplicableStyles(__mergedStyles, __currClass_20), (__implicitProps_20 = toList(delay(() => append((__currClass_20 !== "") ? singleton(["ClassName", __currClass_20]) : empty(), delay(() => ((!isEmpty(__currStyles_20)) ? singleton(["__style", __currStyles_20]) : empty()))))), injectImplicitProps(__implicitProps_20, __currExplicitProps_20))))))(arg10_27);
                                }), RenderResult_Make_E25108F((tupledArg_28) => {
                                    let text_5, __currExplicitProps_21, __currClass_21, __currStyles_21, __implicitProps_21;
                                    const __sibI_23 = tupledArg_28[0] | 0;
                                    const __sibC_23 = tupledArg_28[1] | 0;
                                    const __pFQN_23 = tupledArg_28[2];
                                    const __parentFQN_24 = "LibClient.Components.UiText";
                                    let arg10_29;
                                    const __list_16 = singleton_1(RenderResult_Make_E25108F((text_5 = format("{0}", label_2), (tupledArg_29) => makeTextNode(text_5, tupledArg_29[0], tupledArg_29[1], tupledArg_29[2]))));
                                    arg10_29 = RenderResult_ToRawElements(__parentFQN_24, __list_16);
                                    return Make_5((__currExplicitProps_21 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_21 = (format("{0}{1}{2}{3}{4}", "", sharedClassSet, " ", ScreenSize__get_Class(screenSize), " label") + nthChildClasses(__sibI_23, __sibC_23)), (__currStyles_21 = findApplicableStyles(__mergedStyles, __currClass_21), (__implicitProps_21 = toList(delay(() => append((__currClass_21 !== "") ? singleton(["ClassName", __currClass_21]) : empty(), delay(() => ((!isEmpty(__currStyles_21)) ? singleton(["__style", __currStyles_21]) : empty()))))), injectImplicitProps(__implicitProps_21, __currExplicitProps_21))))))(arg10_29);
                                })]), RenderResult_ToRawElements(__parentFQN_22, __list_17)));
                            }), RenderResult_Make_E25108F((tupledArg_30) => {
                                let __currExplicitProps_22, __currClass_22, __currStyles_22, __implicitProps_22;
                                const __sibI_24 = tupledArg_30[0] | 0;
                                const __sibC_24 = tupledArg_30[1] | 0;
                                const __pFQN_24 = tupledArg_30[2];
                                const __parentFQN_25 = "LibClient.Components.Badge";
                                return Make_4((__currExplicitProps_22 = LibClient_Components_TypeExtensions_TEs__TEs_MakeBadgeProps_Static_ZE5CF26C(badge_2), (__currClass_22 = (format("{0}{1}{2}", "badge ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_24, __sibC_24)), (__currStyles_22 = findApplicableStyles(__mergedStyles, __currClass_22), (__implicitProps_22 = toList(delay(() => append((__currClass_22 !== "") ? singleton(["ClassName", __currClass_22]) : empty(), delay(() => ((!isEmpty(__currStyles_22)) ? singleton(["__style", __currStyles_22]) : empty()))))), injectImplicitProps(__implicitProps_22, __currExplicitProps_22))))))([]);
                            })]), RenderResult_ToRawElements(__parentFQN_21, __list_18)));
                        })), RenderResult_ToRawElements(__parentFQN_3, __list_19))))))) : ((matchValue_1.fields[2] == null) ? ((icon_1 = curry(2, matchValue_1.fields[1]), (label_1 = matchValue_1.fields[0], RenderResult_Make_6E3A73D((__list_11 = singleton_1(RenderResult_Make_E25108F((tupledArg_12) => {
                            let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8, __list_10;
                            const __sibI_10 = tupledArg_12[0] | 0;
                            const __sibC_10 = tupledArg_12[1] | 0;
                            const __pFQN_10 = tupledArg_12[2];
                            const __parentFQN_11 = "ReactXP.Components.View";
                            return Make_2((__currExplicitProps_8 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_8 = ("item-content-container" + nthChildClasses(__sibI_10, __sibC_10)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))((__list_10 = ofArray([RenderResult_Make_E25108F((tupledArg_13) => {
                                let __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9, __list_6;
                                const __sibI_11 = tupledArg_13[0] | 0;
                                const __sibC_11 = tupledArg_13[1] | 0;
                                const __pFQN_11 = tupledArg_13[2];
                                const __parentFQN_12 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_9 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_9 = ("adjust-icon-vertical-position" + nthChildClasses(__sibI_11, __sibC_11)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_9)]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))((__list_6 = singleton_1(RenderResult_Make_E25108F((tupledArg_14) => {
                                    let __currExplicitProps_10, __currClass_10, __currStyles_10, __implicitProps_10;
                                    const __sibI_12 = tupledArg_14[0] | 0;
                                    const __sibC_12 = tupledArg_14[1] | 0;
                                    const __pFQN_12 = tupledArg_14[2];
                                    const __parentFQN_13 = "LibClient.Components.Icon";
                                    return Make_3((__currExplicitProps_10 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, icon_1)), (__currClass_10 = (format("{0}{1}{2}{3}{4}", "icon ", sharedClassSet, " ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_12, __sibC_12)), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append((__currClass_10 !== "") ? singleton(["ClassName", __currClass_10]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["__style", __currStyles_10]) : empty()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))([]);
                                })), RenderResult_ToRawElements(__parentFQN_12, __list_6)));
                            }), RenderResult_Make_E25108F((tupledArg_15) => {
                                let __currExplicitProps_11, __currClass_11, __currStyles_11, __implicitProps_11, __list_9;
                                const __sibI_13 = tupledArg_15[0] | 0;
                                const __sibC_13 = tupledArg_15[1] | 0;
                                const __pFQN_13 = tupledArg_15[2];
                                const __parentFQN_14 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_11 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_11 = ("label-content" + nthChildClasses(__sibI_13, __sibC_13)), (__currStyles_11 = findApplicableStyles(__mergedStyles, __currClass_11), (__implicitProps_11 = toList(delay(() => append((__currClass_11 !== "") ? singleton(["ClassName", __currClass_11]) : empty(), delay(() => ((!isEmpty(__currStyles_11)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_11)]) : empty()))))), injectImplicitProps(__implicitProps_11, __currExplicitProps_11))))))((__list_9 = ofArray([RenderResult_Make_E25108F((tupledArg_16) => {
                                    let text_2, __currExplicitProps_12, __currClass_12, __currStyles_12, __implicitProps_12;
                                    const __sibI_14 = tupledArg_16[0] | 0;
                                    const __sibC_14 = tupledArg_16[1] | 0;
                                    const __pFQN_14 = tupledArg_16[2];
                                    const __parentFQN_15 = "LibClient.Components.UiText";
                                    let arg10_15;
                                    const __list_7 = singleton_1(RenderResult_Make_E25108F((text_2 = format("{0}", label_1), (tupledArg_17) => makeTextNode(text_2, tupledArg_17[0], tupledArg_17[1], tupledArg_17[2]))));
                                    arg10_15 = RenderResult_ToRawElements(__parentFQN_15, __list_7);
                                    return Make_5((__currExplicitProps_12 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_12 = (format("{0}{1}{2}{3}{4}", "", sharedClassSet, " ", ScreenSize__get_Class(screenSize), " label-sentinel") + nthChildClasses(__sibI_14, __sibC_14)), (__currStyles_12 = findApplicableStyles(__mergedStyles, __currClass_12), (__implicitProps_12 = toList(delay(() => append((__currClass_12 !== "") ? singleton(["ClassName", __currClass_12]) : empty(), delay(() => ((!isEmpty(__currStyles_12)) ? singleton(["__style", __currStyles_12]) : empty()))))), injectImplicitProps(__implicitProps_12, __currExplicitProps_12))))))(arg10_15);
                                }), RenderResult_Make_E25108F((tupledArg_18) => {
                                    let text_3, __currExplicitProps_13, __currClass_13, __currStyles_13, __implicitProps_13;
                                    const __sibI_15 = tupledArg_18[0] | 0;
                                    const __sibC_15 = tupledArg_18[1] | 0;
                                    const __pFQN_15 = tupledArg_18[2];
                                    const __parentFQN_16 = "LibClient.Components.UiText";
                                    let arg10_17;
                                    const __list_8 = singleton_1(RenderResult_Make_E25108F((text_3 = format("{0}", label_1), (tupledArg_19) => makeTextNode(text_3, tupledArg_19[0], tupledArg_19[1], tupledArg_19[2]))));
                                    arg10_17 = RenderResult_ToRawElements(__parentFQN_16, __list_8);
                                    return Make_5((__currExplicitProps_13 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_13 = (format("{0}{1}{2}{3}{4}", "", sharedClassSet, " ", ScreenSize__get_Class(screenSize), " label") + nthChildClasses(__sibI_15, __sibC_15)), (__currStyles_13 = findApplicableStyles(__mergedStyles, __currClass_13), (__implicitProps_13 = toList(delay(() => append((__currClass_13 !== "") ? singleton(["ClassName", __currClass_13]) : empty(), delay(() => ((!isEmpty(__currStyles_13)) ? singleton(["__style", __currStyles_13]) : empty()))))), injectImplicitProps(__implicitProps_13, __currExplicitProps_13))))))(arg10_17);
                                })]), RenderResult_ToRawElements(__parentFQN_14, __list_9)));
                            })]), RenderResult_ToRawElements(__parentFQN_11, __list_10)));
                        })), RenderResult_ToRawElements(__parentFQN_3, __list_11)))))) : ((badge = matchValue_1.fields[2], (icon = curry(2, matchValue_1.fields[1]), (label = matchValue_1.fields[0], RenderResult_Make_6E3A73D((__list_5 = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_4;
                            const __sibI_3 = tupledArg_3[0] | 0;
                            const __sibC_3 = tupledArg_3[1] | 0;
                            const __pFQN_3 = tupledArg_3[2];
                            const __parentFQN_4 = "ReactXP.Components.View";
                            return Make_2((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = ("item-content-container item-content-container-with-badge" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_4 = ofArray([RenderResult_Make_E25108F((tupledArg_4) => {
                                let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list;
                                const __sibI_4 = tupledArg_4[0] | 0;
                                const __sibC_4 = tupledArg_4[1] | 0;
                                const __pFQN_4 = tupledArg_4[2];
                                const __parentFQN_5 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_2 = ("adjust-icon-vertical-position" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list = singleton_1(RenderResult_Make_E25108F((tupledArg_5) => {
                                    let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                                    const __sibI_5 = tupledArg_5[0] | 0;
                                    const __sibC_5 = tupledArg_5[1] | 0;
                                    const __pFQN_5 = tupledArg_5[2];
                                    const __parentFQN_6 = "LibClient.Components.Icon";
                                    return Make_3((__currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, icon)), (__currClass_3 = (format("{0}{1}{2}{3}{4}", "icon ", sharedClassSet, " ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["__style", __currStyles_3]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))([]);
                                })), RenderResult_ToRawElements(__parentFQN_5, __list)));
                            }), RenderResult_Make_E25108F((tupledArg_6) => {
                                let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                                const __sibI_6 = tupledArg_6[0] | 0;
                                const __sibC_6 = tupledArg_6[1] | 0;
                                const __pFQN_6 = tupledArg_6[2];
                                const __parentFQN_7 = "LibClient.Components.Badge";
                                return Make_4((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeBadgeProps_Static_ZE5CF26C(badge), (__currClass_4 = (format("{0}{1}{2}", "badge ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))([]);
                            }), RenderResult_Make_E25108F((tupledArg_7) => {
                                let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5, __list_3;
                                const __sibI_7 = tupledArg_7[0] | 0;
                                const __sibC_7 = tupledArg_7[1] | 0;
                                const __pFQN_7 = tupledArg_7[2];
                                const __parentFQN_8 = "ReactXP.Components.View";
                                return Make_2((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_5 = ("label-content label-content-with-icon-badge" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_5)]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))((__list_3 = ofArray([RenderResult_Make_E25108F((tupledArg_8) => {
                                    let text, __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                                    const __sibI_8 = tupledArg_8[0] | 0;
                                    const __sibC_8 = tupledArg_8[1] | 0;
                                    const __pFQN_8 = tupledArg_8[2];
                                    const __parentFQN_9 = "LibClient.Components.UiText";
                                    let arg10_6;
                                    const __list_1 = singleton_1(RenderResult_Make_E25108F((text = format("{0}", label), (tupledArg_9) => makeTextNode(text, tupledArg_9[0], tupledArg_9[1], tupledArg_9[2]))));
                                    arg10_6 = RenderResult_ToRawElements(__parentFQN_9, __list_1);
                                    return Make_5((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_6 = (format("{0}{1}{2}{3}{4}", "", sharedClassSet, " ", ScreenSize__get_Class(screenSize), " label-sentinel") + nthChildClasses(__sibI_8, __sibC_8)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["__style", __currStyles_6]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))(arg10_6);
                                }), RenderResult_Make_E25108F((tupledArg_10) => {
                                    let text_1, __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
                                    const __sibI_9 = tupledArg_10[0] | 0;
                                    const __sibC_9 = tupledArg_10[1] | 0;
                                    const __pFQN_9 = tupledArg_10[2];
                                    const __parentFQN_10 = "LibClient.Components.UiText";
                                    let arg10_8;
                                    const __list_2 = singleton_1(RenderResult_Make_E25108F((text_1 = format("{0}", label), (tupledArg_11) => makeTextNode(text_1, tupledArg_11[0], tupledArg_11[1], tupledArg_11[2]))));
                                    arg10_8 = RenderResult_ToRawElements(__parentFQN_10, __list_2);
                                    return Make_5((__currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_7 = (format("{0}{1}{2}{3}{4}", "", sharedClassSet, " ", ScreenSize__get_Class(screenSize), " label") + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))(arg10_8);
                                })]), RenderResult_ToRawElements(__parentFQN_8, __list_3)));
                            })]), RenderResult_ToRawElements(__parentFQN_4, __list_4)));
                        })), RenderResult_ToRawElements(__parentFQN_3, __list_5)))))))))), Option_getOrElse(new RenderResult(0), map((onPress_1) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_42) => {
                            let __currExplicitProps_31, __currClass_31, __currStyles_31, __implicitProps_31;
                            const __sibI_33 = tupledArg_42[0] | 0;
                            const __sibC_33 = tupledArg_42[1] | 0;
                            const __pFQN_33 = tupledArg_42[2];
                            const __parentFQN_34 = "LibClient.Components.TapCapture";
                            return Make_6((__currExplicitProps_31 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6(onPress_1, void 0, void 0, void 0, void 0, pointerState), (__currClass_31 = ("tap-capture" + nthChildClasses(__sibI_33, __sibC_33)), (__currStyles_31 = findApplicableStyles(__mergedStyles, __currClass_31), (__implicitProps_31 = toList(delay(() => append((__currClass_31 !== "") ? singleton(["ClassName", __currClass_31]) : empty(), delay(() => ((!isEmpty(__currStyles_31)) ? singleton(["__style", __currStyles_31]) : empty()))))), injectImplicitProps(__implicitProps_31, __currExplicitProps_31))))))([]);
                        }))), (matchValue_2 = props.State, (matchValue_2.tag === 0) ? ((onPress = matchValue_2.fields[0], onPress)) : ((matchValue_2.tag === 4) ? ((onPress = matchValue_2.fields[0], onPress)) : (void 0)))))]), RenderResult_ToRawElements(__parentFQN_3, __list_30)));
                    })), RenderResult_ToLeaves_410EF94B(__list_31)))))))))), RenderResult_ToRawElements(__parentFQN_2, __list_32));
                    return react.createElement(react.Fragment, {}, ...children);
                }), (__currClass_32 = nthChildClasses(__sibI_1, __sibC_1), (__currStyles_32 = findApplicableStyles(__mergedStyles, __currClass_32), (__implicitProps_32 = toList(delay(() => append((__currClass_32 !== "") ? singleton(["ClassName", __currClass_32]) : empty(), delay(() => ((!isEmpty(__currStyles_32)) ? singleton(["__style", __currStyles_32]) : empty()))))), injectImplicitProps(__implicitProps_32, __currExplicitProps_32))))))([]);
            })), RenderResult_ToRawElements(__parentFQN_1, __list_33));
            return react.createElement(react.Fragment, {}, ...children_1);
        }), (__currClass_33 = nthChildClasses(__sibI, __sibC), (__currStyles_33 = findApplicableStyles(__mergedStyles, __currClass_33), (__implicitProps_33 = toList(delay(() => append((__currClass_33 !== "") ? singleton(["ClassName", __currClass_33]) : empty(), delay(() => ((!isEmpty(__currStyles_33)) ? singleton(["__style", __currStyles_33]) : empty()))))), injectImplicitProps(__implicitProps_33, __currExplicitProps_33))))))([]);
    })));
}

