import { some, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToRawElements, RenderResult_Make_2B31D457, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../RenderResult.fs.js";
import { Make } from "../../../ReactXP/Components/ScrollView/ScrollView.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5 } from "../../ReactXP/Components/ScrollView/ScrollView.TypeExtensions.fs.js";
import { uncurry, equals } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Actions__get_OnContentLayout, Actions__get_OnScroll, Actions__RefScrollView_Z42C170BB, Scroll } from "../../../Components/ScrollView/ScrollView.typext.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { singleton as singleton_1, isEmpty } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_1 } from "../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../ReactXP/Components/View/View.TypeExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_1;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.ScrollView";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeScrollViewProps_Static_342010B5(void 0, equals(props.Scroll, new Scroll(3)) ? true : equals(props.Scroll, new Scroll(2)), equals(props.Scroll, new Scroll(3)) ? true : equals(props.Scroll, new Scroll(1)), void 0, uncurry(2, void 0), uncurry(2, void 0), void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, (arg00) => {
            Actions__RefScrollView_Z42C170BB(actions, arg00);
        }, void 0, void 0, void 0, some(props.OnLayout), void 0, some((props.RestoreScroll.tag === 0) ? (void 0) : Actions__get_OnScroll(actions))), (__currClass = nthChildClasses(__sibI, __sibC), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.ScrollView", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1, __list;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "ReactXP.Components.View";
            return Make_1((__currExplicitProps_1 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, some((props.RestoreScroll.tag === 0) ? (void 0) : Actions__get_OnContentLayout(actions))), (__currClass_1 = nthChildClasses(__sibI_1, __sibC_1), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_1)]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))((__list = singleton_1(RenderResult_Make_2B31D457(props.children)), RenderResult_ToRawElements(__parentFQN_2, __list)));
        })), RenderResult_ToRawElements(__parentFQN_1, __list_1)));
    })));
}

