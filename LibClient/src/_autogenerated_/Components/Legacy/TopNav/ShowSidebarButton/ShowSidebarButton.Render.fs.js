import { defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_2B31D457, RenderResult_ToRawElements, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../RenderResult.fs.js";
import { Make } from "../../../../../Components/Responsive/Responsive.typext.fs.js";
import { Actionable, Make as Make_1 } from "../../../../../Components/Legacy/TopNav/IconButton/IconButton.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_IconButtonProps_Static_Z57342692 } from "../IconButton/IconButton.TypeExtensions.fs.js";
import { setSidebarVisibility } from "../../../../../Components/AppShell/Content/Content.typext.fs.js";
import { uncurry } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Icon_get_Menu } from "../../../../../Icons.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { singleton as singleton_1, isEmpty } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import * as react from "react";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeResponsiveProps_Static_502D6660 } from "../../../Responsive/Responsive.TypeExtensions.fs.js";
import { noElement } from "../../../../../EggShellReact.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let button, children, __list, __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.Responsive";
        return Make((button = ((children = ((__list = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps, __currClass, __currStyles, __implicitProps;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "LibClient.Components.Legacy.TopNav.IconButton";
            return Make_1((__currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeLegacy_TopNav_IconButtonProps_Static_Z57342692(Actionable((e) => {
                setSidebarVisibility(true, e);
            }), uncurry(2, Icon_get_Menu())), (__currClass = nthChildClasses(__sibI_1, __sibC_1), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))([]);
        })), RenderResult_ToRawElements(__parentFQN_1, __list))), react.createElement(react.Fragment, {}, ...children))), (__currExplicitProps_1 = LibClient_Components_TypeExtensions_TEs__TEs_MakeResponsiveProps_Static_502D6660((_arg1) => {
            let __list_1;
            const children_1 = (__list_1 = singleton_1(RenderResult_Make_2B31D457(props.OnlyOnHandheld ? noElement : button)), RenderResult_ToRawElements(__parentFQN_1, __list_1));
            return react.createElement(react.Fragment, {}, ...children_1);
        }, (_arg2) => {
            let __list_2;
            const children_2 = (__list_2 = singleton_1(RenderResult_Make_2B31D457(button)), RenderResult_ToRawElements(__parentFQN_1, __list_2));
            return react.createElement(react.Fragment, {}, ...children_2);
        }), (__currClass_1 = nthChildClasses(__sibI, __sibC), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["__style", __currStyles_1]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1)))))))([]);
    })));
}

