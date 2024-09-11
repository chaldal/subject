import { some, map, defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_6E3A73D, RenderResult_ToRawElements, RenderResult_ToLeaves_410EF94B, RenderResult_Make_2B31D457, RenderResult_Make_Z1C694E5A, RenderResult_Make_410EF94B, RenderResult, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../RenderResult.fs.js";
import { Make } from "../../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, append as append_1, singleton as singleton_1, isEmpty } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Option_getOrElse } from "../../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { Make as Make_1 } from "../../../../../Components/With/Layout/Layout.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_LayoutProps_Static_Z6CC90BB3 } from "../../../With/Layout/Layout.TypeExtensions.fs.js";
import { uncurry, equals } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { BackButton__get_OnPressOption, Scroll } from "../../../../../Components/Dialog/Shell/FullScreen/FullScreen.typext.fs.js";
import { Make as Make_2 } from "../../../../../ReactXP/Components/ScrollView/ScrollView.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5 } from "../../../../ReactXP/Components/ScrollView/ScrollView.TypeExtensions.fs.js";
import { RulesBasic_height, RulesBasic_minHeight } from "../../../../../ReactXP/Styles/RulesBasic.fs.js";
import { processDynamicStyles } from "../../../../../ReactXP/Styles/Designtime.fs.js";
import * as react from "react";
import { Center, Make as Make_3 } from "../../../../../Components/Legacy/TopNav/Base/Base.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_BaseProps_Static_Z3C7C79CE } from "../../../Legacy/TopNav/Base/Base.TypeExtensions.fs.js";
import { Actionable, Make as Make_4 } from "../../../../../Components/Legacy/TopNav/IconButton/IconButton.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_IconButtonProps_Static_Z57342692 } from "../../../Legacy/TopNav/IconButton/IconButton.TypeExtensions.fs.js";
import { Icon_get_Back } from "../../../../../Icons.fs.js";
import { Make as Make_5 } from "../../../../../Components/Legacy/TopNav/Filler/Filler.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_FillerProps_Static_4019FA9 } from "../../../Legacy/TopNav/Filler/Filler.TypeExtensions.fs.js";
import { ContentPosition, CanClose, Make as Make_6 } from "../../../../../Components/Dialog/Base/Base.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_BaseProps_Static_7FA5D348 } from "../../Base/Base.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, actions];
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
        const __list_10 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_9;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = ("wrapper" + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_9 = ofArray([Option_getOrElse(new RenderResult(0), map((bottomSection) => {
                let __list;
                return RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_Z1C694E5A((__list = singleton_1(RenderResult_Make_2B31D457(bottomSection)), RenderResult_ToLeaves_410EF94B(__list)))));
            }, props.BottomSection)), RenderResult_Make_E25108F((tupledArg_2) => {
                let __currExplicitProps_4, __currClass_4, __currStyles_5, __implicitProps_4;
                const __sibI_2 = tupledArg_2[0] | 0;
                const __sibC_2 = tupledArg_2[1] | 0;
                const __pFQN_2 = tupledArg_2[2];
                const __parentFQN_3 = "LibClient.Components.With.Layout";
                return Make_1((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_LayoutProps_Static_Z6CC90BB3((tupledArg_3) => {
                    let __list_3;
                    const onLayoutOption = tupledArg_3[0];
                    const maybeLayout = tupledArg_3[1];
                    const children = (__list_3 = singleton_1((!equals(props.Scroll, new Scroll(0))) ? RenderResult_Make_E25108F((tupledArg_4) => {
                        let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list_2;
                        const __sibI_3 = tupledArg_4[0] | 0;
                        const __sibC_3 = tupledArg_4[1] | 0;
                        const __pFQN_3 = tupledArg_4[2];
                        const __parentFQN_4 = "ReactXP.Components.ScrollView";
                        return Make_2((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5(void 0, equals(props.Scroll, new Scroll(3)) ? true : equals(props.Scroll, new Scroll(2)), equals(props.Scroll, new Scroll(3)) ? true : equals(props.Scroll, new Scroll(1)), void 0, uncurry(2, void 0), uncurry(2, void 0), void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, some(onLayoutOption)), (__currClass_2 = nthChildClasses(__sibI_3, __sibC_3), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ScrollView", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_5) => {
                            let __currExplicitProps_3, __currClass_3, __currStyles_3, __dynamicStyles, o_1, __currStyles_4, __implicitProps_3, __list_1;
                            const __sibI_4 = tupledArg_5[0] | 0;
                            const __sibC_4 = tupledArg_5[1] | 0;
                            const __pFQN_4 = tupledArg_5[2];
                            const __parentFQN_5 = "ReactXP.Components.View";
                            return Make((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("scroll-view-children" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__dynamicStyles = ((o_1 = map((l) => singleton_1(RulesBasic_minHeight(l.Height)), maybeLayout), Option_getOrElse(singleton_1(RulesBasic_height(0)), o_1))), (__currStyles_4 = append_1(__currStyles_3, processDynamicStyles(__dynamicStyles)), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_4)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))))((__list_1 = singleton_1(RenderResult_Make_2B31D457(props.children)), RenderResult_ToRawElements(__parentFQN_5, __list_1)));
                        })), RenderResult_ToRawElements(__parentFQN_4, __list_2)));
                    }) : (new RenderResult(0))), RenderResult_ToRawElements(__parentFQN_3, __list_3));
                    return react.createElement(react.Fragment, {}, ...children);
                }), (__currClass_4 = nthChildClasses(__sibI_2, __sibC_2), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))([]);
            }), equals(props.Scroll, new Scroll(0)) ? RenderResult_Make_E25108F((tupledArg_6) => {
                let __currExplicitProps_5, __currClass_5, __currStyles_6, __implicitProps_5, __list_4;
                const __sibI_5 = tupledArg_6[0] | 0;
                const __sibC_5 = tupledArg_6[1] | 0;
                const __pFQN_5 = tupledArg_6[2];
                const __parentFQN_6 = "ReactXP.Components.View";
                return Make((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_5 = ("children" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_6)]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))((__list_4 = singleton_1(RenderResult_Make_2B31D457(props.children)), RenderResult_ToRawElements(__parentFQN_6, __list_4)));
            }) : (new RenderResult(0)), ((props.Heading != null) ? true : (props.BackButton.tag === 1)) ? RenderResult_Make_E25108F((tupledArg_7) => {
                let __currExplicitProps_9, children_1, __list_7, matchValue_1, onPress, __list_6, __list_5, children_2, __list_8, __currClass_9, __currStyles_10, __implicitProps_9;
                const __sibI_6 = tupledArg_7[0] | 0;
                const __sibC_6 = tupledArg_7[1] | 0;
                const __pFQN_6 = tupledArg_7[2];
                const __parentFQN_7 = "LibClient.Components.Legacy.TopNav.Base";
                return Make_3((__currExplicitProps_9 = LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_BaseProps_Static_Z3C7C79CE(new Center(1, Option_getOrElse("", props.Heading)), (children_1 = ((__list_7 = singleton_1((matchValue_1 = BackButton__get_OnPressOption(props.BackButton), (matchValue_1 != null) ? ((onPress = matchValue_1, RenderResult_Make_6E3A73D((__list_6 = singleton_1(RenderResult_Make_E25108F((tupledArg_9) => {
                    let __currExplicitProps_7, __currClass_7, __currStyles_8, __implicitProps_7;
                    const __sibI_8 = tupledArg_9[0] | 0;
                    const __sibC_8 = tupledArg_9[1] | 0;
                    const __pFQN_8 = tupledArg_9[2];
                    const __parentFQN_9 = "LibClient.Components.Legacy.TopNav.IconButton";
                    return Make_4((__currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_IconButtonProps_Static_Z57342692(Actionable(onPress), uncurry(2, Icon_get_Back())), (__currClass_7 = ("icon-button" + nthChildClasses(__sibI_8, __sibC_8)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["__style", __currStyles_8]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))([]);
                })), RenderResult_ToRawElements(__parentFQN_7, __list_6))))) : RenderResult_Make_6E3A73D((__list_5 = singleton_1(RenderResult_Make_E25108F((tupledArg_8) => {
                    let __currExplicitProps_6, __currClass_6, __currStyles_7, __implicitProps_6;
                    const __sibI_7 = tupledArg_8[0] | 0;
                    const __sibC_7 = tupledArg_8[1] | 0;
                    const __pFQN_7 = tupledArg_8[2];
                    const __parentFQN_8 = "LibClient.Components.Legacy.TopNav.Filler";
                    return Make_5((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_FillerProps_Static_4019FA9(), (__currClass_6 = nthChildClasses(__sibI_7, __sibC_7), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))([]);
                })), RenderResult_ToRawElements(__parentFQN_7, __list_5))))), RenderResult_ToRawElements(__parentFQN_7, __list_7))), react.createElement(react.Fragment, {}, ...children_1)), void 0, (children_2 = ((__list_8 = singleton_1(RenderResult_Make_E25108F((tupledArg_10) => {
                    let __currExplicitProps_8, __currClass_8, __currStyles_9, __implicitProps_8;
                    const __sibI_9 = tupledArg_10[0] | 0;
                    const __sibC_9 = tupledArg_10[1] | 0;
                    const __pFQN_9 = tupledArg_10[2];
                    const __parentFQN_10 = "LibClient.Components.Legacy.TopNav.Filler";
                    return Make_5((__currExplicitProps_8 = LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_FillerProps_Static_4019FA9(), (__currClass_8 = nthChildClasses(__sibI_9, __sibC_9), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["__style", __currStyles_9]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))([]);
                })), RenderResult_ToRawElements(__parentFQN_7, __list_8))), react.createElement(react.Fragment, {}, ...children_2))), (__currClass_9 = ("top-nav" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["__style", __currStyles_10]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))([]);
            }) : (new RenderResult(0))]), RenderResult_ToRawElements(__parentFQN_2, __list_9)));
        }));
        arg10 = RenderResult_ToRawElements(__parentFQN_1, __list_10);
        return Make_6((__currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_BaseProps_Static_7FA5D348(new CanClose(1), new ContentPosition(0)), (__currClass = nthChildClasses(__sibI, __sibC), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))(arg10);
    })));
}

