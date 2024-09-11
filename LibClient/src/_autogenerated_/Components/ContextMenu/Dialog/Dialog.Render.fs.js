import { defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToLeaves_410EF94B, RenderResult_ToRawElements, makeTextNode, RenderResult_Make_6E3A73D, RenderResult_Make_Z1C694E5A, RenderResult_Make_410EF94B, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { Actions__TryCancel_411CDB90 } from "../../../../Components/ContextMenu/Dialog/Dialog.typext.fs.js";
import { ButtonHighLevelStateFactory_MakeLowLevel_3536472E, ReactEvent_Action_OfBrowserEvent_3F8EC5D5 } from "../../../../Input.fs.js";
import { map, empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, empty as empty_1, singleton as singleton_1, ofSeq, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_1 } from "../../../../ReactXP/Components/ScrollView/ScrollView.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5 } from "../../../ReactXP/Components/ScrollView/ScrollView.TypeExtensions.fs.js";
import { format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { Make as Make_2 } from "../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../Text/Text.TypeExtensions.fs.js";
import { Level, Actionable, Make as Make_3 } from "../../../../Components/Button/Button.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52 } from "../../Button/Button.TypeExtensions.fs.js";
import { ContentPosition, CanClose, CloseAction, Make as Make_4 } from "../../../../Components/Dialog/Base/Base.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_BaseProps_Static_7FA5D348 } from "../../Dialog/Base/Base.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, pstate, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.Dialog.Base";
        let arg10;
        const __list_9 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_8;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (e) => {
                e.stopPropagation();
                Actions__TryCancel_411CDB90(actions, ReactEvent_Action_OfBrowserEvent_3F8EC5D5(e));
            }), (__currClass_1 = ("dialog-contents" + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_8 = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list_7;
                const __sibI_2 = tupledArg_2[0] | 0;
                const __sibC_2 = tupledArg_2[1] | 0;
                const __pFQN_2 = tupledArg_2[2];
                const __parentFQN_3 = "ReactXP.Components.ScrollView";
                return Make_1((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5(void 0, true), (__currClass_2 = ("scroll-view" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ScrollView", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list_7 = singleton_1(RenderResult_Make_410EF94B(ofSeq(map((item) => {
                    let __list_6, text, __list_3, onPress, label, isSelected, __list_4, onPress_1, label_1, __list_5, __list;
                    return RenderResult_Make_Z1C694E5A((__list_6 = singleton_1((item.tag === 1) ? ((text = item.fields[0], RenderResult_Make_6E3A73D((__list_3 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
                        let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4, __list_2;
                        const __sibI_4 = tupledArg_4[0] | 0;
                        const __sibC_4 = tupledArg_4[1] | 0;
                        const __pFQN_4 = tupledArg_4[2];
                        const __parentFQN_5 = "ReactXP.Components.View";
                        return Make((__currExplicitProps_4 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_4 = nthChildClasses(__sibI_4, __sibC_4), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_4)]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_5) => {
                            let text_1, __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5;
                            const __sibI_5 = tupledArg_5[0] | 0;
                            const __sibC_5 = tupledArg_5[1] | 0;
                            const __pFQN_5 = tupledArg_5[2];
                            const __parentFQN_6 = "LibClient.Components.Text";
                            let arg10_2;
                            const __list_1 = singleton_1(RenderResult_Make_E25108F((text_1 = format("{0}", text), (tupledArg_6) => makeTextNode(text_1, tupledArg_6[0], tupledArg_6[1], tupledArg_6[2]))));
                            arg10_2 = RenderResult_ToRawElements(__parentFQN_6, __list_1);
                            return Make_2((__currExplicitProps_5 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_5 = ("heading" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))(arg10_2);
                        })), RenderResult_ToRawElements(__parentFQN_5, __list_2)));
                    })), RenderResult_ToRawElements(__parentFQN_3, __list_3))))) : ((item.tag === 2) ? ((onPress = item.fields[2], (label = item.fields[0], (isSelected = item.fields[1], RenderResult_Make_6E3A73D((__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_7) => {
                        let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                        const __sibI_6 = tupledArg_7[0] | 0;
                        const __sibC_6 = tupledArg_7[1] | 0;
                        const __pFQN_6 = tupledArg_7[2];
                        const __parentFQN_7 = "LibClient.Components.Button";
                        return Make_3((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52(label, ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Actionable((e_1) => {
                            Actions__TryCancel_411CDB90(actions, e_1);
                            onPress(e_1);
                        })), new Level(0)), (__currClass_6 = (("button button-normal" + format(" {0}", isSelected ? "selected" : "")) + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["__style", __currStyles_6]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))([]);
                    })), RenderResult_ToRawElements(__parentFQN_3, __list_4))))))) : ((item.tag === 3) ? ((onPress_1 = item.fields[1], (label_1 = item.fields[0], RenderResult_Make_6E3A73D((__list_5 = singleton_1(RenderResult_Make_E25108F((tupledArg_8) => {
                        let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
                        const __sibI_7 = tupledArg_8[0] | 0;
                        const __sibC_7 = tupledArg_8[1] | 0;
                        const __pFQN_7 = tupledArg_8[2];
                        const __parentFQN_8 = "LibClient.Components.Button";
                        return Make_3((__currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52(label_1, ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Actionable((e_2) => {
                            Actions__TryCancel_411CDB90(actions, e_2);
                            onPress_1(e_2);
                        })), new Level(5)), (__currClass_7 = ("button button-cautionary" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))([]);
                    })), RenderResult_ToRawElements(__parentFQN_3, __list_5)))))) : RenderResult_Make_6E3A73D((__list = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                        let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                        const __sibI_3 = tupledArg_3[0] | 0;
                        const __sibC_3 = tupledArg_3[1] | 0;
                        const __pFQN_3 = tupledArg_3[2];
                        const __parentFQN_4 = "ReactXP.Components.View";
                        return Make((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("divider" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))(empty_1());
                    })), RenderResult_ToRawElements(__parentFQN_3, __list)))))), RenderResult_ToLeaves_410EF94B(__list_6)));
                }, props.Parameters.Items)))), RenderResult_ToRawElements(__parentFQN_3, __list_7)));
            })), RenderResult_ToRawElements(__parentFQN_2, __list_8)));
        }));
        arg10 = RenderResult_ToRawElements(__parentFQN_1, __list_9);
        return Make_4((__currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_BaseProps_Static_7FA5D348(new CanClose(0, ofArray([new CloseAction(0), new CloseAction(1)]), (arg00_1) => {
            Actions__TryCancel_411CDB90(actions, arg00_1);
        }), new ContentPosition(0)), (__currClass = nthChildClasses(__sibI, __sibC), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))(arg10);
    })));
}

