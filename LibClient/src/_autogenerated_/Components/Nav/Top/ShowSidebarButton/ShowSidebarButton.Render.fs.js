import { defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../../ReactXP/Styles/Runtime.fs.js";
import { nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../RenderResult.fs.js";
import { iconOnly, State, Make } from "../../../../../Components/Nav/Top/Item/Item.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeNav_Top_ItemProps_Static_Z3BE3E96B } from "../Item/Item.TypeExtensions.fs.js";
import { setSidebarVisibility } from "../../../../../Components/AppShell/Content/Content.typext.fs.js";
import { uncurry } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Icon_get_Menu } from "../../../../../Icons.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { isEmpty } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";

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
        const __parentFQN_1 = "LibClient.Components.Nav.Top.Item";
        return Make((__currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeNav_Top_ItemProps_Static_Z3BE3E96B(new State(0, (e) => {
            setSidebarVisibility(true, e);
        }), iconOnly(uncurry(2, Icon_get_Menu()))), (__currClass = ("topnav-item" + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))([]);
    })));
}

