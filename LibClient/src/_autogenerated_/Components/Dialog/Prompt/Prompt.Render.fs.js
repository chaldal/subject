import { some, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToRawElements, makeTextNode, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeForm_BaseProps_Static_Z556E86D6 } from "../../Form/Base/Base.TypeExtensions.fs.js";
import { Actions__TryCancel_411CDB90, Field, Acc, Actions__Submit, Acc_get_Initial } from "../../../../Components/Dialog/Prompt/Prompt.typext.fs.js";
import { Make as Make_5, Types_FormHandle$3__get_IsSubmitInProgress, Types_FormHandle$3__FieldValidity_2B595, Accumulator$1 } from "../../../../Components/Form/Base/Base.typext.fs.js";
import { Never, Make } from "../../../../Components/Dialog/Shell/WhiteRounded/Standard/Standard.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_Shell_WhiteRounded_StandardProps_Static_61C15D4D } from "../Shell/WhiteRounded/Standard/Standard.TypeExtensions.fs.js";
import { Make as Make_1 } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, singleton as singleton_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { Make as Make_2 } from "../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../Text/Text.TypeExtensions.fs.js";
import { Make as Make_3 } from "../../../../Components/Input/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_TextProps_Static_CF9FCE9 } from "../../Input/Text/Text.TypeExtensions.fs.js";
import * as react from "react";
import { InProgress, Level, Actionable, Make as Make_4 } from "../../../../Components/Button/Button.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52 } from "../../Button/Button.TypeExtensions.fs.js";
import { ButtonHighLevelStateFactory_MakeLowLevel_3536472E } from "../../../../Input.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, pstate, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.Form.Base";
        let arg00;
        const __currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeForm_BaseProps_Static_Z556E86D6(new Accumulator$1(0, Acc_get_Initial()), (arg00_1, arg10_1, arg20) => Actions__Submit(actions, arg00_1, arg10_1, void 0), (form) => {
            let __list_4;
            const children_2 = (__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
                let __currExplicitProps_5, children, __list_2, children_1, __list_3, __currClass_5, __currStyles_5, __implicitProps_5;
                const __sibI_1 = tupledArg_1[0] | 0;
                const __sibC_1 = tupledArg_1[1] | 0;
                const __pFQN_1 = tupledArg_1[2];
                const __parentFQN_2 = "LibClient.Components.Dialog.Shell.WhiteRounded.Standard";
                return Make((__currExplicitProps_5 = LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_Shell_WhiteRounded_StandardProps_Static_61C15D4D(Never, (children = ((__list_2 = ofArray([RenderResult_Make_E25108F((tupledArg_2) => {
                    let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_1;
                    const __sibI_2 = tupledArg_2[0] | 0;
                    const __sibC_2 = tupledArg_2[1] | 0;
                    const __pFQN_2 = tupledArg_2[2];
                    const __parentFQN_3 = "ReactXP.Components.View";
                    return Make_1((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = ("details" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_3) => {
                        let text, __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1;
                        const __sibI_3 = tupledArg_3[0] | 0;
                        const __sibC_3 = tupledArg_3[1] | 0;
                        const __pFQN_3 = tupledArg_3[2];
                        const __parentFQN_4 = "LibClient.Components.Text";
                        let arg10_3;
                        const __list = singleton_1(RenderResult_Make_E25108F((text = format("{0}", props.Parameters.Details), (tupledArg_4) => makeTextNode(text, tupledArg_4[0], tupledArg_4[1], tupledArg_4[2]))));
                        arg10_3 = RenderResult_ToRawElements(__parentFQN_4, __list);
                        return Make_2((__currExplicitProps_1 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_1 = ("details-text" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["__style", __currStyles_1]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))(arg10_3);
                    })), RenderResult_ToRawElements(__parentFQN_3, __list_1)));
                }), RenderResult_Make_E25108F((tupledArg_5) => {
                    let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
                    const __sibI_4 = tupledArg_5[0] | 0;
                    const __sibC_4 = tupledArg_5[1] | 0;
                    const __pFQN_4 = tupledArg_5[2];
                    const __parentFQN_5 = "LibClient.Components.Input.Text";
                    return Make_3((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_TextProps_Static_CF9FCE9(form.Acc.Value, (value_2) => {
                        form.UpdateAcc((acc) => (new Acc(value_2)));
                    }, Types_FormHandle$3__FieldValidity_2B595(form, new Field(0)), void 0, void 0, void 0, "Value"), (__currClass_2 = nthChildClasses(__sibI_4, __sibC_4), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))([]);
                })]), RenderResult_ToRawElements(__parentFQN_2, __list_2))), react.createElement(react.Fragment, {}, ...children)), (children_1 = ((__list_3 = ofArray([RenderResult_Make_E25108F((tupledArg_6) => {
                    let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
                    const __sibI_5 = tupledArg_6[0] | 0;
                    const __sibC_5 = tupledArg_6[1] | 0;
                    const __pFQN_5 = tupledArg_6[2];
                    const __parentFQN_6 = "LibClient.Components.Button";
                    return Make_4((__currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52("Cancel", ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Actionable((arg00_13) => {
                        Actions__TryCancel_411CDB90(actions, arg00_13);
                    })), new Level(1)), (__currClass_3 = nthChildClasses(__sibI_5, __sibC_5), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["__style", __currStyles_3]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))([]);
                }), RenderResult_Make_E25108F((tupledArg_7) => {
                    let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                    const __sibI_6 = tupledArg_7[0] | 0;
                    const __sibC_6 = tupledArg_7[1] | 0;
                    const __pFQN_6 = tupledArg_7[2];
                    const __parentFQN_7 = "LibClient.Components.Button";
                    return Make_4((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52("Submit", ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Types_FormHandle$3__get_IsSubmitInProgress(form) ? InProgress : Actionable(form.TrySubmitLowLevel))), (__currClass_4 = nthChildClasses(__sibI_6, __sibC_6), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))([]);
                })]), RenderResult_ToRawElements(__parentFQN_2, __list_3))), react.createElement(react.Fragment, {}, ...children_1)), void 0, void 0, some(props.Parameters.MaybeHeading)), (__currClass_5 = nthChildClasses(__sibI_1, __sibC_1), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["__style", __currStyles_5]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))([]);
            })), RenderResult_ToRawElements(__parentFQN_1, __list_4));
            return react.createElement(react.Fragment, {}, ...children_2);
        });
        const __currClass_6 = nthChildClasses(__sibI, __sibC);
        const __currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6);
        const __implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["__style", __currStyles_6]) : empty())))));
        arg00 = injectImplicitProps(__implicitProps_6, __currExplicitProps_6);
        return Make_5()(arg00)([]);
    })));
}

