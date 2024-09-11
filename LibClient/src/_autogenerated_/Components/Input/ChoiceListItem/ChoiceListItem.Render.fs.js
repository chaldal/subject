import { defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_2B31D457, makeTextNode, RenderResult_ToRawElements, RenderResult_Make_6E3A73D, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, singleton as singleton_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_1 } from "../../../../Components/Icon/Icon.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B } from "../../Icon/Icon.TypeExtensions.fs.js";
import { partialApply, uncurry } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Icon_get_RadioButtonEmpty, Icon_get_CheckboxEmpty, Icon_get_RadioButtonFilled, Icon_get_CheckboxChecked } from "../../../../Icons.fs.js";
import { Make as Make_2 } from "../../../../Components/UiText/UiText.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737 } from "../../UiText/UiText.TypeExtensions.fs.js";
import { Make as Make_3 } from "../../../../Components/TapCapture/TapCapture.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6 } from "../../TapCapture/TapCapture.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    let isSelected;
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, (isSelected = props.Group.IsSelected(props.Value), RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_6, __list, __list_1, matchValue_2, label, __list_5, __list_3;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = (format("{0}{1}{2}", "view ", TopLevelBlockClass, "") + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_6 = ofArray([isSelected ? RenderResult_Make_6E3A73D((__list = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, matchValue, __currClass_1, __currStyles_1, __implicitProps_1;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "LibClient.Components.Icon";
            return Make_1((__currExplicitProps_1 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, (matchValue = props.Group.Value, (matchValue.tag === 3) ? Icon_get_CheckboxChecked() : ((matchValue.tag === 0) ? Icon_get_RadioButtonFilled() : ((matchValue.tag === 1) ? Icon_get_RadioButtonFilled() : Icon_get_CheckboxChecked()))))), (__currClass_1 = ("icon checked" + nthChildClasses(__sibI_1, __sibC_1)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["__style", __currStyles_1]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))([]);
        })), RenderResult_ToRawElements(__parentFQN_1, __list))) : RenderResult_Make_6E3A73D((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => {
            let __currExplicitProps_2, matchValue_1, __currClass_2, __currStyles_2, __implicitProps_2;
            const __sibI_2 = tupledArg_2[0] | 0;
            const __sibC_2 = tupledArg_2[1] | 0;
            const __pFQN_2 = tupledArg_2[2];
            const __parentFQN_3 = "LibClient.Components.Icon";
            return Make_1((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeIconProps_Static_Z3C9BD4B(uncurry(2, (matchValue_1 = props.Group.Value, (matchValue_1.tag === 3) ? Icon_get_CheckboxEmpty() : ((matchValue_1.tag === 0) ? Icon_get_RadioButtonEmpty() : ((matchValue_1.tag === 1) ? Icon_get_RadioButtonEmpty() : Icon_get_CheckboxEmpty()))))), (__currClass_2 = ("icon unchecked" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))([]);
        })), RenderResult_ToRawElements(__parentFQN_1, __list_1))), (matchValue_2 = props.Label, (matchValue_2.tag === 0) ? ((label = matchValue_2.fields[0], RenderResult_Make_6E3A73D((__list_5 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => {
            let text, __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
            const __sibI_4 = tupledArg_4[0] | 0;
            const __sibC_4 = tupledArg_4[1] | 0;
            const __pFQN_4 = tupledArg_4[2];
            const __parentFQN_5 = "LibClient.Components.UiText";
            let arg10_6;
            const __list_4 = singleton_1(RenderResult_Make_E25108F((text = format("{0}", label), (tupledArg_5) => makeTextNode(text, tupledArg_5[0], tupledArg_5[1], tupledArg_5[2]))));
            arg10_6 = RenderResult_ToRawElements(__parentFQN_5, __list_4);
            return Make_2((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeUiTextProps_Static_46684737(), (__currClass_4 = ("label" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))(arg10_6);
        })), RenderResult_ToRawElements(__parentFQN_1, __list_5))))) : RenderResult_Make_6E3A73D((__list_3 = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
            let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3, __list_2;
            const __sibI_3 = tupledArg_3[0] | 0;
            const __sibC_3 = tupledArg_3[1] | 0;
            const __pFQN_3 = tupledArg_3[2];
            const __parentFQN_4 = "ReactXP.Components.View";
            return Make((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("label" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))((__list_2 = singleton_1(RenderResult_Make_2B31D457(props.children)), RenderResult_ToRawElements(__parentFQN_4, __list_2)));
        })), RenderResult_ToRawElements(__parentFQN_1, __list_3)))), RenderResult_Make_E25108F((tupledArg_6) => {
            let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5;
            const __sibI_5 = tupledArg_6[0] | 0;
            const __sibC_5 = tupledArg_6[1] | 0;
            const __pFQN_5 = tupledArg_6[2];
            const __parentFQN_6 = "LibClient.Components.TapCapture";
            return Make_3((__currExplicitProps_5 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTapCaptureProps_Static_Z599606E6(partialApply(1, props.Group.Toggle, [props.Value])), (__currClass_5 = nthChildClasses(__sibI_5, __sibC_5), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))([]);
        })]), RenderResult_ToRawElements(__parentFQN_1, __list_6)));
    }))));
}

