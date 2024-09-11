import { map, some, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../ReactXP/Styles/Runtime.fs.js";
import { makeTextNode, RenderResult_ToRawElements, RenderResult_Make_2B31D457, RenderResult_Make_6E3A73D, RenderResult_Make_410EF94B, RenderResult, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../RenderResult.fs.js";
import { Make } from "../../../Components/With/Layout/Layout.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_LayoutProps_Static_Z6CC90BB3 } from "../With/Layout/Layout.TypeExtensions.fs.js";
import { Make as Make_1 } from "../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { format } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, singleton as singleton_1, empty as empty_1, isEmpty } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Size, Make as Make_2 } from "../../../ReactXP/Components/Image/Image.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeImageProps_Static_331EB6A7 } from "../../ReactXP/Components/Image/Image.TypeExtensions.fs.js";
import { Option_getOrElse } from "../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { Label__get_UseScrim } from "../../../Components/ImageCard/ImageCard.typext.fs.js";
import { Make as Make_3 } from "../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../UiText/UiText.TypeExtensions.fs.js";
import { Make as Make_4 } from "../../../Components/TapCapture/TapCapture.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6 } from "../TapCapture/TapCapture.TypeExtensions.fs.js";
import * as react from "react";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.With.Layout";
        return Make((__currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_LayoutProps_Static_Z6CC90BB3((tupledArg_1) => {
            let __list_6;
            const onLayoutOption = tupledArg_1[0];
            const maybeLayout = tupledArg_1[1];
            const children = (__list_6 = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_5;
                const __sibI_1 = tupledArg_2[0] | 0;
                const __sibC_1 = tupledArg_2[1] | 0;
                const __pFQN_1 = tupledArg_2[2];
                const __parentFQN_2 = "ReactXP.Components.View";
                return Make_1((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, some(onLayoutOption)), (__currClass = (format("{0}{1}{2}", "view ", TopLevelBlockClass, "") + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_5 = ofArray([RenderResult_Make_E25108F((tupledArg_3) => {
                    let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1;
                    const __sibI_2 = tupledArg_3[0] | 0;
                    const __sibC_2 = tupledArg_3[1] | 0;
                    const __pFQN_2 = tupledArg_3[2];
                    const __parentFQN_3 = "ReactXP.Components.Image";
                    return Make_2((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeImageProps_Static_331EB6A7(props.Source, new Size(0, maybeLayout), void 0, void 0, "cover"), (__currClass_1 = ("image" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.Image", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))(empty_1());
                }), Option_getOrElse(new RenderResult(0), map((label) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
                    let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list_4, __list_3, text, __list_2;
                    const __sibI_3 = tupledArg_4[0] | 0;
                    const __sibC_3 = tupledArg_4[1] | 0;
                    const __pFQN_3 = tupledArg_4[2];
                    const __parentFQN_4 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_2 = ("label-block" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list_4 = ofArray([Label__get_UseScrim(label) ? RenderResult_Make_E25108F((tupledArg_5) => {
                        let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                        const __sibI_4 = tupledArg_5[0] | 0;
                        const __sibC_4 = tupledArg_5[1] | 0;
                        const __pFQN_4 = tupledArg_5[2];
                        const __parentFQN_5 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("scrim" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))(empty_1());
                    }) : (new RenderResult(0)), (label.tag === 1) ? RenderResult_Make_6E3A73D((__list_3 = singleton_1(RenderResult_Make_2B31D457(props.children)), RenderResult_ToRawElements(__parentFQN_4, __list_3))) : ((text = label.fields[0], RenderResult_Make_6E3A73D((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_6) => {
                        let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4, __list_1;
                        const __sibI_5 = tupledArg_6[0] | 0;
                        const __sibC_5 = tupledArg_6[1] | 0;
                        const __pFQN_5 = tupledArg_6[2];
                        const __parentFQN_6 = "ReactXP.Components.View";
                        return Make_1((__currExplicitProps_4 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_4 = ("label-text-wrapper" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_4)]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_7) => {
                            let text_1, __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5;
                            const __sibI_6 = tupledArg_7[0] | 0;
                            const __sibC_6 = tupledArg_7[1] | 0;
                            const __pFQN_6 = tupledArg_7[2];
                            const __parentFQN_7 = "LibClient.Components.UiText";
                            let arg10_1;
                            const __list = singleton_1(RenderResult_Make_E25108F((text_1 = format("{0}", text), (tupledArg_8) => makeTextNode(text_1, tupledArg_8[0], tupledArg_8[1], tupledArg_8[2]))));
                            arg10_1 = RenderResult_ToRawElements(__parentFQN_7, __list);
                            return Make_3((__currExplicitProps_5 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_5 = ("label-text" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))(arg10_1);
                        })), RenderResult_ToRawElements(__parentFQN_6, __list_1)));
                    })), RenderResult_ToRawElements(__parentFQN_4, __list_2)))))]), RenderResult_ToRawElements(__parentFQN_4, __list_4)));
                }))), props.Label)), Option_getOrElse(new RenderResult(0), map((onPress) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_9) => {
                    let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                    const __sibI_7 = tupledArg_9[0] | 0;
                    const __sibC_7 = tupledArg_9[1] | 0;
                    const __pFQN_7 = tupledArg_9[2];
                    const __parentFQN_8 = "LibClient.Components.TapCapture";
                    return Make_4((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6(onPress), (__currClass_6 = nthChildClasses(__sibI_7, __sibC_7), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["__style", __currStyles_6]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))([]);
                }))), props.OnPress))]), RenderResult_ToRawElements(__parentFQN_2, __list_5)));
            })), RenderResult_ToRawElements(__parentFQN_1, __list_6));
            return react.createElement(react.Fragment, {}, ...children);
        }, true), (__currClass_7 = nthChildClasses(__sibI, __sibC), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))([]);
    })));
}

