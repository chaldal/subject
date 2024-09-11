import { defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToRawElements, makeTextNode, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, singleton as singleton_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_1 } from "../../../../Components/Heading/Heading.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeHeadingProps_Static_DDDDD60 } from "../../Heading/Heading.TypeExtensions.fs.js";
import { Level, Actionable, Make as Make_2 } from "../../../../Components/Button/Button.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52 } from "../../Button/Button.TypeExtensions.fs.js";
import { ButtonHighLevelStateFactory_MakeLowLevel_3536472E } from "../../../../Input.fs.js";
import { Make as Make_3 } from "../../../../Components/Buttons/Buttons.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonsProps_Static_519A9F32 } from "../../Buttons/Buttons.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_3;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = ("view" + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_3 = ofArray([RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "LibClient.Components.Heading";
            let arg10;
            const __list = singleton_1(RenderResult_Make_E25108F((tupledArg_2) => makeTextNode("Sorry, an error occurred!", tupledArg_2[0], tupledArg_2[1], tupledArg_2[2])));
            arg10 = RenderResult_ToRawElements(__parentFQN_2, __list);
            return Make_1((__currExplicitProps_1 = LibClient_Components_TypeExtensions_TEs__TEs_MakeHeadingProps_Static_DDDDD60(), (__currClass_1 = nthChildClasses(__sibI_1, __sibC_1), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["__style", __currStyles_1]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))(arg10);
        }), RenderResult_Make_E25108F((tupledArg_3) => {
            let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2, __list_1;
            const __sibI_2 = tupledArg_3[0] | 0;
            const __sibC_2 = tupledArg_3[1] | 0;
            const __pFQN_2 = tupledArg_3[2];
            const __parentFQN_3 = "ReactXP.Components.View";
            return Make((__currExplicitProps_2 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_2 = ("second-line" + nthChildClasses(__sibI_2, __sibC_2)), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_2)]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_4) => makeTextNode("You may try reloading the screen and the error may go away. If not, please try again later.", tupledArg_4[0], tupledArg_4[1], tupledArg_4[2]))), RenderResult_ToRawElements(__parentFQN_3, __list_1)));
        }), RenderResult_Make_E25108F((tupledArg_5) => {
            let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
            const __sibI_3 = tupledArg_5[0] | 0;
            const __sibC_3 = tupledArg_5[1] | 0;
            const __pFQN_3 = tupledArg_5[2];
            const __parentFQN_4 = "LibClient.Components.Buttons";
            let arg10_3;
            const __list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_6) => {
                let __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                const __sibI_4 = tupledArg_6[0] | 0;
                const __sibC_4 = tupledArg_6[1] | 0;
                const __pFQN_4 = tupledArg_6[2];
                const __parentFQN_5 = "LibClient.Components.Button";
                return Make_2((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52("Reload", ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Actionable((_arg1) => {
                    props.Retry();
                })), new Level(1)), (__currClass_4 = nthChildClasses(__sibI_4, __sibC_4), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))([]);
            }));
            arg10_3 = RenderResult_ToRawElements(__parentFQN_4, __list_2);
            return Make_3((__currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonsProps_Static_519A9F32(), (__currClass_3 = nthChildClasses(__sibI_3, __sibC_3), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["__style", __currStyles_3]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))(arg10_3);
        })]), RenderResult_ToRawElements(__parentFQN_1, __list_3)));
    })));
}

