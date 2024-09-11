import { map, some, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../ReactXP/Styles/Runtime.fs.js";
import { makeTextNode, RenderResult_Make_6E3A73D, RenderResult_ToRawElements, RenderResult_Make_2B31D457, RenderResult_Make_410EF94B, RenderResult, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../RenderResult.fs.js";
import { Make } from "../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, singleton as singleton_1, isEmpty } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Option_getOrElse } from "../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { format } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { Make as Make_1 } from "../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../Text/Text.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, matchValue, onPress, __currClass, __currStyles, __implicitProps, __list_13;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, some((matchValue = props.State, (matchValue.tag === 0) ? ((onPress = matchValue.fields[0], (e) => {
            e.stopPropagation();
            onPress(e);
        })) : (void 0)))), (__currClass = ("view" + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_13 = ofArray([Option_getOrElse(new RenderResult(0), map((leftIcon) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_1 = ("left-icon" + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list = singleton_1(RenderResult_Make_2B31D457(leftIcon(20))), RenderResult_ToRawElements(__parentFQN_2, __list)));
        }))), props.LeftIcon)), RenderResult_Make_E25108F((tupledArg_2) => {
            let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list_4, matchValue_1, text, __list_3, __list_1;
            const __sibI_2 = tupledArg_2[0] | 0;
            const __sibC_2 = tupledArg_2[1] | 0;
            const __pFQN_2 = tupledArg_2[2];
            const __parentFQN_3 = "ReactXP.Components.View";
            return Make((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_2 = ("label" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list_4 = singleton_1((matchValue_1 = props.Label, (matchValue_1.tag === 1) ? ((text = matchValue_1.fields[0], RenderResult_Make_6E3A73D((__list_3 = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                let text_1, __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                const __sibI_3 = tupledArg_3[0] | 0;
                const __sibC_3 = tupledArg_3[1] | 0;
                const __pFQN_3 = tupledArg_3[2];
                const __parentFQN_4 = "LibClient.Components.Text";
                let arg10_2;
                const __list_2 = singleton_1(RenderResult_Make_E25108F((text_1 = format("{0}", text), (tupledArg_4) => makeTextNode(text_1, tupledArg_4[0], tupledArg_4[1], tupledArg_4[2]))));
                arg10_2 = RenderResult_ToRawElements(__parentFQN_4, __list_2);
                return Make_1((__currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_3 = ("label-text" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["__style", __currStyles_3]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))(arg10_2);
            })), RenderResult_ToRawElements(__parentFQN_3, __list_3))))) : RenderResult_Make_6E3A73D((__list_1 = singleton_1(RenderResult_Make_2B31D457(props.children)), RenderResult_ToRawElements(__parentFQN_3, __list_1))))), RenderResult_ToRawElements(__parentFQN_3, __list_4)));
        }), Option_getOrElse(new RenderResult(0), map((right) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_5) => {
            let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4, __list_12, icon, __list_8, number_1, icon_1, __list_11, number, __list_6;
            const __sibI_4 = tupledArg_5[0] | 0;
            const __sibC_4 = tupledArg_5[1] | 0;
            const __pFQN_4 = tupledArg_5[2];
            const __parentFQN_5 = "ReactXP.Components.View";
            return Make((__currExplicitProps_4 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_4 = ("right" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_4)]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))((__list_12 = singleton_1((right.tag === 0) ? ((icon = right.fields[0], RenderResult_Make_6E3A73D((__list_8 = singleton_1(RenderResult_Make_E25108F((tupledArg_8) => {
                let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6, __list_7;
                const __sibI_6 = tupledArg_8[0] | 0;
                const __sibC_6 = tupledArg_8[1] | 0;
                const __pFQN_6 = tupledArg_8[2];
                const __parentFQN_7 = "ReactXP.Components.View";
                return Make((__currExplicitProps_6 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_6 = ("right-icon" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_6)]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))((__list_7 = singleton_1(RenderResult_Make_2B31D457(icon(32))), RenderResult_ToRawElements(__parentFQN_7, __list_7)));
            })), RenderResult_ToRawElements(__parentFQN_5, __list_8))))) : ((right.tag === 2) ? ((number_1 = (right.fields[0] | 0), (icon_1 = right.fields[1], RenderResult_Make_6E3A73D((__list_11 = ofArray([RenderResult_Make_E25108F((tupledArg_9) => {
                let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7, __list_9, text_3;
                const __sibI_7 = tupledArg_9[0] | 0;
                const __sibC_7 = tupledArg_9[1] | 0;
                const __pFQN_7 = tupledArg_9[2];
                const __parentFQN_8 = "ReactXP.Components.View";
                return Make((__currExplicitProps_7 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_7 = ("number" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_7)]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))((__list_9 = singleton_1(RenderResult_Make_E25108F((text_3 = format("{0}", number_1), (tupledArg_10) => makeTextNode(text_3, tupledArg_10[0], tupledArg_10[1], tupledArg_10[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_9)));
            }), RenderResult_Make_E25108F((tupledArg_11) => {
                let __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8, __list_10;
                const __sibI_8 = tupledArg_11[0] | 0;
                const __sibC_8 = tupledArg_11[1] | 0;
                const __pFQN_8 = tupledArg_11[2];
                const __parentFQN_9 = "ReactXP.Components.View";
                return Make((__currExplicitProps_8 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_8 = ("right-icon" + nthChildClasses(__sibI_8, __sibC_8)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_8)]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))((__list_10 = singleton_1(RenderResult_Make_2B31D457(icon_1(32))), RenderResult_ToRawElements(__parentFQN_9, __list_10)));
            })]), RenderResult_ToRawElements(__parentFQN_5, __list_11)))))) : ((number = (right.fields[0] | 0), RenderResult_Make_6E3A73D((__list_6 = singleton_1(RenderResult_Make_E25108F((tupledArg_6) => {
                let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5, __list_5, text_2;
                const __sibI_5 = tupledArg_6[0] | 0;
                const __sibC_5 = tupledArg_6[1] | 0;
                const __pFQN_5 = tupledArg_6[2];
                const __parentFQN_6 = "ReactXP.Components.View";
                return Make((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_5 = ("number" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_5)]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))((__list_5 = singleton_1(RenderResult_Make_E25108F((text_2 = format("{0}", number), (tupledArg_7) => makeTextNode(text_2, tupledArg_7[0], tupledArg_7[1], tupledArg_7[2])))), RenderResult_ToRawElements(__parentFQN_6, __list_5)));
            })), RenderResult_ToRawElements(__parentFQN_5, __list_6))))))), RenderResult_ToRawElements(__parentFQN_5, __list_12)));
        }))), props.Right))]), RenderResult_ToRawElements(__parentFQN_1, __list_13)));
    })));
}

