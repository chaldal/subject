import { some, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToRawElements, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../Components/With/ScreenSize/ScreenSize.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06 } from "../../With/ScreenSize/ScreenSize.TypeExtensions.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_PickerInternals_BaseProps_Static_ZF99D2A0 } from "../PickerInternals/Base/Base.TypeExtensions.fs.js";
import { format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { singleton as singleton_1, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_1 } from "../../../../Components/Input/PickerInternals/Base/Base.typext.fs.js";
import * as react from "react";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.With.ScreenSize";
        return Make((__currExplicitProps_1 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_ScreenSizeProps_Static_Z6EE84C06((screenSize) => {
            let __list;
            const children = (__list = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
                const __sibI_1 = tupledArg_1[0] | 0;
                const __sibC_1 = tupledArg_1[1] | 0;
                const __pFQN_1 = tupledArg_1[2];
                const __parentFQN_2 = "LibClient.Components.Input.PickerInternals.Base";
                let arg00_1;
                const __currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_PickerInternals_BaseProps_Static_ZF99D2A0(props.Items, props.ItemView, props.Value, props.Validity, screenSize, void 0, void 0, void 0, some(props.Label), some(props.Placeholder));
                const __currClass = format("{0}", TopLevelBlockClass) + nthChildClasses(__sibI_1, __sibC_1);
                const __currStyles = findApplicableStyles(__mergedStyles, __currClass);
                const __implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty())))));
                arg00_1 = injectImplicitProps(__implicitProps, __currExplicitProps);
                return Make_1()(arg00_1)([]);
            })), RenderResult_ToRawElements(__parentFQN_1, __list));
            return react.createElement(react.Fragment, {}, ...children);
        }), (__currClass_1 = nthChildClasses(__sibI, __sibC), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["__style", __currStyles_1]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))([]);
    })));
}

