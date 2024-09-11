import { defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToLeaves_410EF94B, RenderResult_ToRawElements, makeTextNode, RenderResult_Make_6E3A73D, RenderResult_Make_Z1C694E5A, RenderResult_Make_410EF94B, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../ReactXP/Components/ScrollView/ScrollView.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5 } from "../../../ReactXP/Components/ScrollView/ScrollView.TypeExtensions.fs.js";
import { mapIndexed, empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { empty as empty_1, ofArray, singleton as singleton_1, ofSeq, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_1 } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { Make as Make_2 } from "../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../Text/Text.TypeExtensions.fs.js";
import { Make as Make_3 } from "../../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../../UiText/UiText.TypeExtensions.fs.js";
import { Make as Make_4 } from "../../../../Components/TapCapture/TapCapture.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6 } from "../../TapCapture/TapCapture.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, void 0];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_12;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.ScrollView";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5(void 0, true), (__currClass = ("scroll-view" + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ScrollView", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_12 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_11;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make_1((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = ("view" + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_11 = singleton_1(RenderResult_Make_410EF94B(ofSeq(mapIndexed((index, item) => {
                let __list_10, text, __list_3, onPress, label, isSelected, __list_6, onPress_1, label_1, __list_9, __list;
                return RenderResult_Make_Z1C694E5A((__list_10 = singleton_1((item.tag === 1) ? ((text = item.fields[0], RenderResult_Make_6E3A73D((__list_3 = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                    let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3, __list_2;
                    const __sibI_3 = tupledArg_3[0] | 0;
                    const __sibC_3 = tupledArg_3[1] | 0;
                    const __pFQN_3 = tupledArg_3[2];
                    const __parentFQN_4 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("heading" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
                        let text_1, __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                        const __sibI_4 = tupledArg_4[0] | 0;
                        const __sibC_4 = tupledArg_4[1] | 0;
                        const __pFQN_4 = tupledArg_4[2];
                        const __parentFQN_5 = "LibClient.Components.Text";
                        let arg10_1;
                        const __list_1 = singleton_1(RenderResult_Make_E25108F((text_1 = format("{0}", text), (tupledArg_5) => makeTextNode(text_1, tupledArg_5[0], tupledArg_5[1], tupledArg_5[2]))));
                        arg10_1 = RenderResult_ToRawElements(__parentFQN_5, __list_1);
                        return Make_2((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_4 = nthChildClasses(__sibI_4, __sibC_4), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))(arg10_1);
                    })), RenderResult_ToRawElements(__parentFQN_4, __list_2)));
                })), RenderResult_ToRawElements(__parentFQN_2, __list_3))))) : ((item.tag === 2) ? ((onPress = item.fields[2], (label = item.fields[0], (isSelected = item.fields[1], RenderResult_Make_6E3A73D((__list_6 = singleton_1(RenderResult_Make_E25108F((tupledArg_6) => {
                    let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5, __list_5;
                    const __sibI_5 = tupledArg_6[0] | 0;
                    const __sibC_5 = tupledArg_6[1] | 0;
                    const __pFQN_5 = tupledArg_6[2];
                    const __parentFQN_6 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_5 = (("button" + format(" {0} {1}", (index === 0) ? "first" : "", isSelected ? "selected" : "")) + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_5)]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))((__list_5 = ofArray([RenderResult_Make_E25108F((tupledArg_7) => {
                        let text_2, __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                        const __sibI_6 = tupledArg_7[0] | 0;
                        const __sibC_6 = tupledArg_7[1] | 0;
                        const __pFQN_6 = tupledArg_7[2];
                        const __parentFQN_7 = "LibClient.Components.UiText";
                        let arg10_5;
                        const __list_4 = singleton_1(RenderResult_Make_E25108F((text_2 = format("{0}", label), (tupledArg_8) => makeTextNode(text_2, tupledArg_8[0], tupledArg_8[1], tupledArg_8[2]))));
                        arg10_5 = RenderResult_ToRawElements(__parentFQN_7, __list_4);
                        return Make_3((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_6 = (("button-text" + format(" {0}", isSelected ? "selected" : "")) + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["__style", __currStyles_6]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))(arg10_5);
                    }), RenderResult_Make_E25108F((tupledArg_9) => {
                        let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
                        const __sibI_7 = tupledArg_9[0] | 0;
                        const __sibC_7 = tupledArg_9[1] | 0;
                        const __pFQN_7 = tupledArg_9[2];
                        const __parentFQN_8 = "LibClient.Components.TapCapture";
                        return Make_4((__currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6((_arg1) => {
                            props.Hide();
                            onPress(props.OpeningEvent);
                        }), (__currClass_7 = nthChildClasses(__sibI_7, __sibC_7), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))([]);
                    })]), RenderResult_ToRawElements(__parentFQN_6, __list_5)));
                })), RenderResult_ToRawElements(__parentFQN_2, __list_6))))))) : ((item.tag === 3) ? ((onPress_1 = item.fields[1], (label_1 = item.fields[0], RenderResult_Make_6E3A73D((__list_9 = singleton_1(RenderResult_Make_E25108F((tupledArg_10) => {
                    let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8, __list_8;
                    const __sibI_8 = tupledArg_10[0] | 0;
                    const __sibC_8 = tupledArg_10[1] | 0;
                    const __pFQN_8 = tupledArg_10[2];
                    const __parentFQN_9 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_8 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_8 = (("button button-cautionary" + format(" {0}", (index === 0) ? "first" : "")) + nthChildClasses(__sibI_8, __sibC_8)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))((__list_8 = ofArray([RenderResult_Make_E25108F((tupledArg_11) => {
                        let text_3, __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9;
                        const __sibI_9 = tupledArg_11[0] | 0;
                        const __sibC_9 = tupledArg_11[1] | 0;
                        const __pFQN_9 = tupledArg_11[2];
                        const __parentFQN_10 = "LibClient.Components.UiText";
                        let arg10_10;
                        const __list_7 = singleton_1(RenderResult_Make_E25108F((text_3 = format("{0}", label_1), (tupledArg_12) => makeTextNode(text_3, tupledArg_12[0], tupledArg_12[1], tupledArg_12[2]))));
                        arg10_10 = RenderResult_ToRawElements(__parentFQN_10, __list_7);
                        return Make_3((__currExplicitProps_9 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_9 = ("button-text button-cautionary" + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["__style", __currStyles_9]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))(arg10_10);
                    }), RenderResult_Make_E25108F((tupledArg_13) => {
                        let __currExplicitProps_10, __currClass_10, __currStyles_10, __implicitProps_10;
                        const __sibI_10 = tupledArg_13[0] | 0;
                        const __sibC_10 = tupledArg_13[1] | 0;
                        const __pFQN_10 = tupledArg_13[2];
                        const __parentFQN_11 = "LibClient.Components.TapCapture";
                        return Make_4((__currExplicitProps_10 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6((_arg2) => {
                            props.Hide();
                            onPress_1(props.OpeningEvent);
                        }), (__currClass_10 = nthChildClasses(__sibI_10, __sibC_10), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append((__currClass_10 !== "") ? singleton(["ClassName", __currClass_10]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["__style", __currStyles_10]) : empty()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))([]);
                    })]), RenderResult_ToRawElements(__parentFQN_9, __list_8)));
                })), RenderResult_ToRawElements(__parentFQN_2, __list_9)))))) : RenderResult_Make_6E3A73D((__list = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                    let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
                    const __sibI_2 = tupledArg_2[0] | 0;
                    const __sibC_2 = tupledArg_2[1] | 0;
                    const __pFQN_2 = tupledArg_2[2];
                    const __parentFQN_3 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_2 = ("divider" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))(empty_1());
                })), RenderResult_ToRawElements(__parentFQN_2, __list)))))), RenderResult_ToLeaves_410EF94B(__list_10)));
            }, props.Items)))), RenderResult_ToRawElements(__parentFQN_2, __list_11)));
        })), RenderResult_ToRawElements(__parentFQN_1, __list_12)));
    })));
}

