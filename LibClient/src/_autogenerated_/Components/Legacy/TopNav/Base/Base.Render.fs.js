import { defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../../ReactXP/Styles/Runtime.fs.js";
import { makeTextNode, RenderResult_Make_6E3A73D, RenderResult_ToRawElements, RenderResult, RenderResult_ToLeaves_410EF94B, RenderResult_Make_2B31D457, RenderResult_Make_Z1C694E5A, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../RenderResult.fs.js";
import { Make } from "../../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, singleton as singleton_1, isEmpty } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { equals } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { noElement } from "../../../../../EggShellReact.fs.js";
import { Make as Make_1 } from "../../../../../Components/Legacy/TopNav/Filler/Filler.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_FillerProps_Static_4019FA9 } from "../Filler/Filler.TypeExtensions.fs.js";
import { format } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { Make as Make_2 } from "../../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../../Text/Text.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, void 0];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_8;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = ("view" + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_8 = ofArray([RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_1, __list;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = ("left" + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_1 = ofArray([(!equals(props.Left, noElement)) ? RenderResult_Make_Z1C694E5A((__list = singleton_1(RenderResult_Make_2B31D457(props.Left)), RenderResult_ToLeaves_410EF94B(__list))) : (new RenderResult(0)), equals(props.Left, noElement) ? RenderResult_Make_E25108F((tupledArg_2) => {
                let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
                const __sibI_2 = tupledArg_2[0] | 0;
                const __sibC_2 = tupledArg_2[1] | 0;
                const __pFQN_2 = tupledArg_2[2];
                const __parentFQN_3 = "LibClient.Components.Legacy.TopNav.Filler";
                return Make_1((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_FillerProps_Static_4019FA9(), (__currClass_2 = nthChildClasses(__sibI_2, __sibC_2), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))([]);
            }) : (new RenderResult(0))]), RenderResult_ToRawElements(__parentFQN_2, __list_1)));
        }), RenderResult_Make_E25108F((tupledArg_3) => {
            let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3, __list_5, matchValue, text, __list_4, __list_2;
            const __sibI_3 = tupledArg_3[0] | 0;
            const __sibC_3 = tupledArg_3[1] | 0;
            const __pFQN_3 = tupledArg_3[2];
            const __parentFQN_4 = "ReactXP.Components.View";
            return Make((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("center" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))((__list_5 = singleton_1((matchValue = props.Center, (matchValue.tag === 1) ? ((text = matchValue.fields[0], RenderResult_Make_6E3A73D((__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
                let text_1, __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                const __sibI_4 = tupledArg_4[0] | 0;
                const __sibC_4 = tupledArg_4[1] | 0;
                const __pFQN_4 = tupledArg_4[2];
                const __parentFQN_5 = "LibClient.Components.Text";
                let arg10_3;
                const __list_3 = singleton_1(RenderResult_Make_E25108F((text_1 = format("{0}", text), (tupledArg_5) => makeTextNode(text_1, tupledArg_5[0], tupledArg_5[1], tupledArg_5[2]))));
                arg10_3 = RenderResult_ToRawElements(__parentFQN_5, __list_3);
                return Make_2((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_4 = ("heading" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))(arg10_3);
            })), RenderResult_ToRawElements(__parentFQN_4, __list_4))))) : RenderResult_Make_6E3A73D((__list_2 = singleton_1(RenderResult_Make_2B31D457(props.children)), RenderResult_ToRawElements(__parentFQN_4, __list_2))))), RenderResult_ToRawElements(__parentFQN_4, __list_5)));
        }), RenderResult_Make_E25108F((tupledArg_6) => {
            let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5, __list_7, __list_6;
            const __sibI_5 = tupledArg_6[0] | 0;
            const __sibC_5 = tupledArg_6[1] | 0;
            const __pFQN_5 = tupledArg_6[2];
            const __parentFQN_6 = "ReactXP.Components.View";
            return Make((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_5 = ("right" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_5)]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))((__list_7 = ofArray([(!equals(props.Right, noElement)) ? RenderResult_Make_Z1C694E5A((__list_6 = singleton_1(RenderResult_Make_2B31D457(props.Right)), RenderResult_ToLeaves_410EF94B(__list_6))) : (new RenderResult(0)), equals(props.Right, noElement) ? RenderResult_Make_E25108F((tupledArg_7) => {
                let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                const __sibI_6 = tupledArg_7[0] | 0;
                const __sibC_6 = tupledArg_7[1] | 0;
                const __pFQN_6 = tupledArg_7[2];
                const __parentFQN_7 = "LibClient.Components.Legacy.TopNav.Filler";
                return Make_1((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_FillerProps_Static_4019FA9(), (__currClass_6 = nthChildClasses(__sibI_6, __sibC_6), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["__style", __currStyles_6]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))([]);
            }) : (new RenderResult(0))]), RenderResult_ToRawElements(__parentFQN_6, __list_7)));
        })]), RenderResult_ToRawElements(__parentFQN_1, __list_8)));
    })));
}

