import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_410EF94B, RenderResult, RenderResult_Make_2B31D457, makeTextNode, RenderResult_ToRawElements, RenderResult_Make_6E3A73D, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, singleton as singleton_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Option_getOrElse } from "../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";
import { Make as Make_1 } from "../../../../Components/Icon/Icon.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B } from "../../Icon/Icon.TypeExtensions.fs.js";
import { uncurry } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Icon_get_CheckboxEmpty, Icon_get_CheckboxChecked, Icon_get_CheckboxUnknown } from "../../../../Icons.fs.js";
import { InputValidity__get_InvalidReason, InputValidity__get_IsInvalid } from "../../../../Input.fs.js";
import { Make as Make_2 } from "../../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../../UiText/UiText.TypeExtensions.fs.js";
import { Make as Make_3 } from "../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../Text/Text.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, void 0];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_9;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = (format("{0}", TopLevelBlockClass) + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_9 = ofArray([RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list_6, matchValue, __list_2, __list, __list_1, matchValue_1, label, __list_5, __list_3;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (_arg1) => {
                props.OnChange(!Option_getOrElse(false, props.Value));
            }), (__currClass_1 = ("div" + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list_6 = ofArray([(matchValue = props.Value, (matchValue == null) ? RenderResult_Make_6E3A73D((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
                let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                const __sibI_4 = tupledArg_4[0] | 0;
                const __sibC_4 = tupledArg_4[1] | 0;
                const __pFQN_4 = tupledArg_4[2];
                const __parentFQN_5 = "LibClient.Components.Icon";
                return Make_1((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, Icon_get_CheckboxUnknown())), (__currClass_4 = (("icon" + format(" {0} {1}", InputValidity__get_IsInvalid(props.Validity) ? "checkbox-invalid" : "", (!InputValidity__get_IsInvalid(props.Validity)) ? "unchecked" : "")) + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))([]);
            })), RenderResult_ToRawElements(__parentFQN_2, __list_2))) : (matchValue ? RenderResult_Make_6E3A73D((__list = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
                let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
                const __sibI_2 = tupledArg_2[0] | 0;
                const __sibC_2 = tupledArg_2[1] | 0;
                const __pFQN_2 = tupledArg_2[2];
                const __parentFQN_3 = "LibClient.Components.Icon";
                return Make_1((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, Icon_get_CheckboxChecked())), (__currClass_2 = (("icon" + format(" {0} {1}", InputValidity__get_IsInvalid(props.Validity) ? "checkbox-invalid" : "", (!InputValidity__get_IsInvalid(props.Validity)) ? "checked" : "")) + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))([]);
            })), RenderResult_ToRawElements(__parentFQN_2, __list))) : RenderResult_Make_6E3A73D((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                const __sibI_3 = tupledArg_3[0] | 0;
                const __sibC_3 = tupledArg_3[1] | 0;
                const __pFQN_3 = tupledArg_3[2];
                const __parentFQN_4 = "LibClient.Components.Icon";
                return Make_1((__currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, Icon_get_CheckboxEmpty())), (__currClass_3 = (("icon" + format(" {0} {1}", InputValidity__get_IsInvalid(props.Validity) ? "checkbox-invalid" : "", (!InputValidity__get_IsInvalid(props.Validity)) ? "unchecked" : "")) + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["__style", __currStyles_3]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))([]);
            })), RenderResult_ToRawElements(__parentFQN_2, __list_1))))), (matchValue_1 = props.Label, (matchValue_1.tag === 0) ? ((label = matchValue_1.fields[0], RenderResult_Make_6E3A73D((__list_5 = singleton_1(RenderResult_Make_E25108F((tupledArg_5) => {
                let text, __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5;
                const __sibI_5 = tupledArg_5[0] | 0;
                const __sibC_5 = tupledArg_5[1] | 0;
                const __pFQN_5 = tupledArg_5[2];
                const __parentFQN_6 = "LibClient.Components.UiText";
                let arg10_7;
                const __list_4 = singleton_1(RenderResult_Make_E25108F((text = format("{0}", label), (tupledArg_6) => makeTextNode(text, tupledArg_6[0], tupledArg_6[1], tupledArg_6[2]))));
                arg10_7 = RenderResult_ToRawElements(__parentFQN_6, __list_4);
                return Make_2((__currExplicitProps_5 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_5 = ("label" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))(arg10_7);
            })), RenderResult_ToRawElements(__parentFQN_2, __list_5))))) : RenderResult_Make_6E3A73D((__list_3 = singleton_1(RenderResult_Make_2B31D457(props.children)), RenderResult_ToRawElements(__parentFQN_2, __list_3))))]), RenderResult_ToRawElements(__parentFQN_2, __list_6)));
        }), Option_getOrElse(new RenderResult(0), map((reason) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_7) => {
            let __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6, __list_8;
            const __sibI_6 = tupledArg_7[0] | 0;
            const __sibC_6 = tupledArg_7[1] | 0;
            const __pFQN_6 = tupledArg_7[2];
            const __parentFQN_7 = "ReactXP.Components.View";
            return Make((__currExplicitProps_6 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_6 = nthChildClasses(__sibI_6, __sibC_6), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_6)]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))((__list_8 = singleton_1(RenderResult_Make_E25108F((tupledArg_8) => {
                let text_1, __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7;
                const __sibI_7 = tupledArg_8[0] | 0;
                const __sibC_7 = tupledArg_8[1] | 0;
                const __pFQN_7 = tupledArg_8[2];
                const __parentFQN_8 = "LibClient.Components.Text";
                let arg10_11;
                const __list_7 = singleton_1(RenderResult_Make_E25108F((text_1 = format("{0}", reason), (tupledArg_9) => makeTextNode(text_1, tupledArg_9[0], tupledArg_9[1], tupledArg_9[2]))));
                arg10_11 = RenderResult_ToRawElements(__parentFQN_8, __list_7);
                return Make_3((__currExplicitProps_7 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_7 = ("invalid-reason" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["__style", __currStyles_7]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))(arg10_11);
            })), RenderResult_ToRawElements(__parentFQN_7, __list_8)));
        }))), InputValidity__get_InvalidReason(props.Validity)))]), RenderResult_ToRawElements(__parentFQN_1, __list_9)));
    })));
}

