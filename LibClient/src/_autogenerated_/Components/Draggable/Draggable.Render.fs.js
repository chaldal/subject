import { some, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToRawElements, RenderResult_Make_2B31D457, nthChildClasses, RenderResult_Make_E25108F, RenderResult_Make_6E3A73D, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../RenderResult.fs.js";
import { Make } from "../../../ReactXP/Components/GestureView/GestureView.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeGestureViewProps_Static_18AAFB35 } from "../../ReactXP/Components/GestureView/GestureView.TypeExtensions.fs.js";
import { contentsView, Actions__OnPanHorizontal_Z4AEA1285, Actions__OnPanVertical_Z4AEA1285 } from "../../../Components/Draggable/Draggable.typext.fs.js";
import { format } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { singleton as singleton_1, isEmpty } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import * as react from "react";

export function render(props, estate, pstate, actions, __componentStyles) {
    let gestureView, children, __list_1, __list_2;
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_6E3A73D((gestureView = ((children = ((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.GestureView";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeGestureViewProps_Static_18AAFB35(void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, some(((props.Up != null) ? true : (props.Down != null)) ? ((arg00) => {
            Actions__OnPanVertical_Z4AEA1285(actions, arg00);
        }) : (void 0)), some(((props.Left != null) ? true : (props.Right != null)) ? ((arg00_1) => {
            Actions__OnPanHorizontal_Z4AEA1285(actions, arg00_1);
        }) : (void 0))), (__currClass = (format("{0}{1}{2}", "gesture-view ", TopLevelBlockClass, "") + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.GestureView", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list = singleton_1(RenderResult_Make_2B31D457(props.children)), RenderResult_ToRawElements(__parentFQN_1, __list)));
    })), RenderResult_ToRawElements(__parentFQN, __list_1))), react.createElement(react.Fragment, {}, ...children))), (__list_2 = singleton_1(RenderResult_Make_2B31D457(contentsView(gestureView, props, estate))), RenderResult_ToRawElements(__parentFQN, __list_2))))));
}

