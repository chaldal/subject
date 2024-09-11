import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToLeaves_410EF94B, RenderResult_Make_6E3A73D, makeTextNode, RenderResult_ToRawElements, RenderResult_Make_410EF94B, RenderResult, nthChildClasses, RenderResult_Make_Z1C694E5A, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../Components/Pointer/State/State.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakePointer_StateProps_Static_76B68C13 } from "../../Pointer/State/State.TypeExtensions.fs.js";
import { State__get_Name } from "../../../../Components/Sidebar/Item/Item.typext.fs.js";
import { format, printf, toText } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { Make as Make_1 } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { TopLevelBlockClass } from "../../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, empty as empty_1, singleton as singleton_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Option_getOrElse } from "../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { Make as Make_2 } from "../../../../Components/Icon/Icon.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B } from "../../Icon/Icon.TypeExtensions.fs.js";
import { curry, uncurry } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Make as Make_3 } from "../../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../../UiText/UiText.TypeExtensions.fs.js";
import { Make as Make_4 } from "../../../../ReactXP/Components/ActivityIndicator/ActivityIndicator.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B } from "../../../ReactXP/Components/ActivityIndicator/ActivityIndicator.TypeExtensions.fs.js";
import { $007CPositiveInteger$007C } from "../../../../../../LibLangFsharp/src/Positive.fs.js";
import { Make as Make_5 } from "../../../../Components/TapCapture/TapCapture.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6 } from "../../TapCapture/TapCapture.TypeExtensions.fs.js";
import * as react from "react";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps_12, __currClass_12, __currStyles_12, __implicitProps_12;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.Pointer.State";
        return Make((__currExplicitProps_12 = LibClient_Components_TypeExtensions_TEs__TEs_MakePointer_StateProps_Static_76B68C13((pointerState) => {
            let __list_13, stateClass, isSelected, selectedClass, hoveredClass, depressedClass, sharedClassSet, __list_12;
            const children = (__list_13 = singleton_1((stateClass = ("state-" + State__get_Name(props.State)), (isSelected = (props.State.tag === 3), (selectedClass = (isSelected ? "selected" : ""), (hoveredClass = ((pointerState.IsHovered && (!pointerState.IsDepressed)) ? "hovered" : ""), (depressedClass = (pointerState.IsDepressed ? "depressed" : ""), (sharedClassSet = toText(printf("%s %s %s %s"))(stateClass)(selectedClass)(hoveredClass)(depressedClass), RenderResult_Make_Z1C694E5A((__list_12 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
                let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_11, __list_4, __list_10, matchValue_2, onPress;
                const __sibI_1 = tupledArg_1[0] | 0;
                const __sibC_1 = tupledArg_1[1] | 0;
                const __pFQN_1 = tupledArg_1[2];
                const __parentFQN_2 = "ReactXP.Components.View";
                return Make_1((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = (format("{0}{1}{2}{3}{4}", "item ", sharedClassSet, " ", TopLevelBlockClass, "") + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_11 = ofArray([Option_getOrElse(new RenderResult(0), map((icon) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                    let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list;
                    const __sibI_2 = tupledArg_2[0] | 0;
                    const __sibC_2 = tupledArg_2[1] | 0;
                    const __pFQN_2 = tupledArg_2[2];
                    const __parentFQN_3 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = ("left" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                        let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
                        const __sibI_3 = tupledArg_3[0] | 0;
                        const __sibC_3 = tupledArg_3[1] | 0;
                        const __pFQN_3 = tupledArg_3[2];
                        const __parentFQN_4 = "LibClient.Components.Icon";
                        return Make_2((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, icon)), (__currClass_2 = (format("{0}{1}{2}", "icon-left ", sharedClassSet, "") + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))([]);
                    })), RenderResult_ToRawElements(__parentFQN_3, __list)));
                }))), curry(2, props.LeftIcon))), RenderResult_Make_E25108F((tupledArg_4) => {
                    let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3, __list_2;
                    const __sibI_4 = tupledArg_4[0] | 0;
                    const __sibC_4 = tupledArg_4[1] | 0;
                    const __pFQN_4 = tupledArg_4[2];
                    const __parentFQN_5 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("middle" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_5) => {
                        let text, __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                        const __sibI_5 = tupledArg_5[0] | 0;
                        const __sibC_5 = tupledArg_5[1] | 0;
                        const __pFQN_5 = tupledArg_5[2];
                        const __parentFQN_6 = "LibClient.Components.UiText";
                        let arg10_4;
                        const __list_1 = singleton_1(RenderResult_Make_E25108F((text = format("{0}", props.Label), (tupledArg_6) => makeTextNode(text, tupledArg_6[0], tupledArg_6[1], tupledArg_6[2]))));
                        arg10_4 = RenderResult_ToRawElements(__parentFQN_6, __list_1);
                        return Make_3((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(void 0, true), (__currClass_4 = (format("{0}{1}{2}", "label ", sharedClassSet, "") + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))(arg10_4);
                    })), RenderResult_ToRawElements(__parentFQN_5, __list_2)));
                }), (props.State.tag === 1) ? RenderResult_Make_6E3A73D((__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_7) => {
                    let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5, __list_3;
                    const __sibI_6 = tupledArg_7[0] | 0;
                    const __sibC_6 = tupledArg_7[1] | 0;
                    const __pFQN_6 = tupledArg_7[2];
                    const __parentFQN_7 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_5 = ("right" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_5)]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))((__list_3 = singleton_1(RenderResult_Make_E25108F((tupledArg_8) => {
                        let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                        const __sibI_7 = tupledArg_8[0] | 0;
                        const __sibC_7 = tupledArg_8[1] | 0;
                        const __pFQN_7 = tupledArg_8[2];
                        const __parentFQN_8 = "ReactXP.Components.ActivityIndicator";
                        return Make_4((__currExplicitProps_6 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B("#aaaaaa", "small"), (__currClass_6 = nthChildClasses(__sibI_7, __sibC_7), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ActivityIndicator", __currStyles_6)]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))(empty_1());
                    })), RenderResult_ToRawElements(__parentFQN_7, __list_3)));
                })), RenderResult_ToRawElements(__parentFQN_2, __list_4))) : RenderResult_Make_6E3A73D((__list_10 = singleton_1(Option_getOrElse(new RenderResult(0), map((right) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_9) => {
                    let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7, __list_9, icon_1, __list_8, count, __list_7;
                    const __sibI_8 = tupledArg_9[0] | 0;
                    const __sibC_8 = tupledArg_9[1] | 0;
                    const __pFQN_8 = tupledArg_9[2];
                    const __parentFQN_9 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_7 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_7 = ("right" + nthChildClasses(__sibI_8, __sibC_8)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_7)]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))((__list_9 = singleton_1((right.tag === 1) ? ((icon_1 = right.fields[0], RenderResult_Make_6E3A73D((__list_8 = singleton_1(RenderResult_Make_E25108F((tupledArg_13) => {
                        let __currExplicitProps_10, __currClass_10, __currStyles_10, __implicitProps_10;
                        const __sibI_11 = tupledArg_13[0] | 0;
                        const __sibC_11 = tupledArg_13[1] | 0;
                        const __pFQN_11 = tupledArg_13[2];
                        const __parentFQN_12 = "LibClient.Components.Icon";
                        return Make_2((__currExplicitProps_10 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(icon_1), (__currClass_10 = (format("{0}{1}{2}", "icon-right ", sharedClassSet, "") + nthChildClasses(__sibI_11, __sibC_11)), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append((__currClass_10 !== "") ? singleton(["ClassName", __currClass_10]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["__style", __currStyles_10]) : empty()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))([]);
                    })), RenderResult_ToRawElements(__parentFQN_9, __list_8))))) : ((count = ($007CPositiveInteger$007C(right.fields[0]) | 0), RenderResult_Make_6E3A73D((__list_7 = singleton_1(RenderResult_Make_E25108F((tupledArg_10) => {
                        let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8, __list_6;
                        const __sibI_9 = tupledArg_10[0] | 0;
                        const __sibC_9 = tupledArg_10[1] | 0;
                        const __pFQN_9 = tupledArg_10[2];
                        const __parentFQN_10 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_8 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_8 = (format("{0}{1}{2}", "badge ", sharedClassSet, "") + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))((__list_6 = singleton_1(RenderResult_Make_E25108F((tupledArg_11) => {
                            let text_1, __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9;
                            const __sibI_10 = tupledArg_11[0] | 0;
                            const __sibC_10 = tupledArg_11[1] | 0;
                            const __pFQN_10 = tupledArg_11[2];
                            const __parentFQN_11 = "LibClient.Components.UiText";
                            let arg10_9;
                            const __list_5 = singleton_1(RenderResult_Make_E25108F((text_1 = format("{0}", count), (tupledArg_12) => makeTextNode(text_1, tupledArg_12[0], tupledArg_12[1], tupledArg_12[2]))));
                            arg10_9 = RenderResult_ToRawElements(__parentFQN_11, __list_5);
                            return Make_3((__currExplicitProps_9 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_9 = (format("{0}{1}{2}", "badge-text ", sharedClassSet, "") + nthChildClasses(__sibI_10, __sibC_10)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["__style", __currStyles_9]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))(arg10_9);
                        })), RenderResult_ToRawElements(__parentFQN_10, __list_6)));
                    })), RenderResult_ToRawElements(__parentFQN_9, __list_7)))))), RenderResult_ToRawElements(__parentFQN_9, __list_9)));
                }))), props.Right))), RenderResult_ToRawElements(__parentFQN_2, __list_10))), Option_getOrElse(new RenderResult(0), map((onPress_1) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_14) => {
                    let __currExplicitProps_11, __currClass_11, __currStyles_11, __implicitProps_11;
                    const __sibI_12 = tupledArg_14[0] | 0;
                    const __sibC_12 = tupledArg_14[1] | 0;
                    const __pFQN_12 = tupledArg_14[2];
                    const __parentFQN_13 = "LibClient.Components.TapCapture";
                    return Make_5((__currExplicitProps_11 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6(onPress_1, void 0, void 0, void 0, void 0, pointerState), (__currClass_11 = ("tap-capture" + nthChildClasses(__sibI_12, __sibC_12)), (__currStyles_11 = findApplicableStyles(__mergedStyles, __currClass_11), (__implicitProps_11 = toList(delay(() => append((__currClass_11 !== "") ? singleton(["ClassName", __currClass_11]) : empty(), delay(() => ((!isEmpty(__currStyles_11)) ? singleton(["__style", __currStyles_11]) : empty()))))), injectImplicitProps(__implicitProps_11, __currExplicitProps_11))))))([]);
                }))), (matchValue_2 = props.State, (matchValue_2.tag === 0) ? ((onPress = matchValue_2.fields[0], onPress)) : (void 0))))]), RenderResult_ToRawElements(__parentFQN_2, __list_11)));
            })), RenderResult_ToLeaves_410EF94B(__list_12)))))))))), RenderResult_ToRawElements(__parentFQN_1, __list_13));
            return react.createElement(react.Fragment, {}, ...children);
        }), (__currClass_12 = nthChildClasses(__sibI, __sibC), (__currStyles_12 = findApplicableStyles(__mergedStyles, __currClass_12), (__implicitProps_12 = toList(delay(() => append((__currClass_12 !== "") ? singleton(["ClassName", __currClass_12]) : empty(), delay(() => ((!isEmpty(__currStyles_12)) ? singleton(["__style", __currStyles_12]) : empty()))))), injectImplicitProps(__implicitProps_12, __currExplicitProps_12))))))([]);
    })));
}

