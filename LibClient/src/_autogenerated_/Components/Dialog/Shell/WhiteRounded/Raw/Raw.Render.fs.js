import { defaultArg } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Runtime_isNative, Helpers_extractProp } from "../../../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult, RenderResult_ToRawElements, RenderResult_Make_2B31D457, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../../RenderResult.fs.js";
import { Make } from "../../../../../../Components/With/ScreenSize/ScreenSize.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06 } from "../../../../With/ScreenSize/ScreenSize.TypeExtensions.fs.js";
import { Make as Make_1 } from "../../../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { format } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { empty, singleton, append, delay, toList } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, empty as empty_1, singleton as singleton_1, isEmpty } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { ScreenSize__get_Class } from "../../../../../../Responsive.fs.js";
import { Make as Make_2 } from "../../../../../../ReactXP/Components/ActivityIndicator/ActivityIndicator.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B } from "../../../../../ReactXP/Components/ActivityIndicator/ActivityIndicator.TypeExtensions.fs.js";
import { Color_Grey_Z721C83C5, Color__get_ToReactXPString } from "../../../../../../ColorModule.fs.js";
import { ContentPosition, Make as Make_4, CanClose__OnClose_411CDB90, CanClose__get_ShouldShowCloseButton } from "../../../../../../Components/Dialog/Base/Base.typext.fs.js";
import { Actionable, Make as Make_3 } from "../../../../../../Components/IconButton/IconButton.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeIconButtonProps_Static_Z7C2F413F } from "../../../../IconButton/IconButton.TypeExtensions.fs.js";
import { ButtonHighLevelStateFactory_MakeLowLevel_3536472E } from "../../../../../../Input.fs.js";
import { uncurry } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Icon_get_X } from "../../../../../../Icons.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_BaseProps_Static_7FA5D348 } from "../../../Base/Base.TypeExtensions.fs.js";
import * as react from "react";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, void 0];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.With.ScreenSize";
        return Make((__currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06((screenSize) => {
            let __list_5, onPress;
            const children = (__list_5 = singleton_1((onPress = (Runtime_isNative() ? (undefined) : ((e) => {
                e.stopPropagation();
            })), RenderResult_Make_E25108F((tupledArg_1) => {
                let __currExplicitProps, __currClass, __currStyles, __implicitProps;
                const __sibI_1 = tupledArg_1[0] | 0;
                const __sibC_1 = tupledArg_1[1] | 0;
                const __pFQN_1 = tupledArg_1[2];
                const __parentFQN_2 = "LibClient.Components.Dialog.Base";
                let arg10_1;
                const __list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                    let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_3;
                    const __sibI_2 = tupledArg_2[0] | 0;
                    const __sibC_2 = tupledArg_2[1] | 0;
                    const __pFQN_2 = tupledArg_2[2];
                    const __parentFQN_3 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = (format("{0}{1}{2}", "max-size-limiter position-", props.Position, "") + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_3 = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                        let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list_2;
                        const __sibI_3 = tupledArg_3[0] | 0;
                        const __sibC_3 = tupledArg_3[1] | 0;
                        const __pFQN_3 = tupledArg_3[2];
                        const __parentFQN_4 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, onPress), (__currClass_2 = (format("{0}{1}{2}", "white-rounded-base ", ScreenSize__get_Class(screenSize), "") + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list_2 = ofArray([RenderResult_Make_E25108F((tupledArg_4) => {
                            let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3, __list;
                            const __sibI_4 = tupledArg_4[0] | 0;
                            const __sibC_4 = tupledArg_4[1] | 0;
                            const __pFQN_4 = tupledArg_4[2];
                            const __parentFQN_5 = "ReactXP.Components.View";
                            return Make_1((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("content" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))((__list = singleton_1(RenderResult_Make_2B31D457(props.children)), RenderResult_ToRawElements(__parentFQN_5, __list)));
                        }), props.InProgress ? RenderResult_Make_E25108F((tupledArg_5) => {
                            let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4, __list_1;
                            const __sibI_5 = tupledArg_5[0] | 0;
                            const __sibC_5 = tupledArg_5[1] | 0;
                            const __pFQN_5 = tupledArg_5[2];
                            const __parentFQN_6 = "ReactXP.Components.View";
                            return Make_1((__currExplicitProps_4 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_4 = ("in-progress" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_4)]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_6) => {
                                let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5;
                                const __sibI_6 = tupledArg_6[0] | 0;
                                const __sibC_6 = tupledArg_6[1] | 0;
                                const __pFQN_6 = tupledArg_6[2];
                                const __parentFQN_7 = "ReactXP.Components.ActivityIndicator";
                                return Make_2((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeActivityIndicatorProps_Static_3A26C93B(Color__get_ToReactXPString(Color_Grey_Z721C83C5("cc"))), (__currClass_5 = nthChildClasses(__sibI_6, __sibC_6), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ActivityIndicator", __currStyles_5)]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))(empty_1());
                            })), RenderResult_ToRawElements(__parentFQN_6, __list_1)));
                        }) : (new RenderResult(0)), CanClose__get_ShouldShowCloseButton(props.CanClose) ? RenderResult_Make_E25108F((tupledArg_7) => {
                            let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                            const __sibI_7 = tupledArg_7[0] | 0;
                            const __sibC_7 = tupledArg_7[1] | 0;
                            const __pFQN_7 = tupledArg_7[2];
                            const __parentFQN_8 = "LibClient.Components.IconButton";
                            return Make_3((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconButtonProps_Static_Z7C2F413F(ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Actionable((arg00_9) => {
                                CanClose__OnClose_411CDB90(props.CanClose, arg00_9);
                            })), uncurry(2, Icon_get_X())), (__currClass_6 = ("close-button" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["__style", __currStyles_6]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))([]);
                        }) : (new RenderResult(0))]), RenderResult_ToRawElements(__parentFQN_4, __list_2)));
                    })), RenderResult_ToRawElements(__parentFQN_3, __list_3)));
                }));
                arg10_1 = RenderResult_ToRawElements(__parentFQN_2, __list_4);
                return Make_4((__currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_BaseProps_Static_7FA5D348(props.CanClose, (screenSize.tag === 0) ? (new ContentPosition(0)) : (new ContentPosition(1))), (__currClass = nthChildClasses(__sibI_1, __sibC_1), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))(arg10_1);
            }))), RenderResult_ToRawElements(__parentFQN_1, __list_5));
            return react.createElement(react.Fragment, {}, ...children);
        }), (__currClass_7 = nthChildClasses(__sibI, __sibC), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))([]);
    })));
}

