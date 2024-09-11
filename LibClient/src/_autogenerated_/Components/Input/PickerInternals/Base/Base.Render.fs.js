import { some, defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../../ReactXP/Styles/Runtime.fs.js";
import { nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../../RenderResult.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_PickerInternals_FieldProps_Static_Z52658B7F } from "../Field/Field.TypeExtensions.fs.js";
import { Actions$1__GetModel } from "../../../../../Components/Input/PickerInternals/Base/Base.typext.fs.js";
import { format } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { isEmpty } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make } from "../../../../../Components/Input/PickerInternals/Field/Field.typext.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.Input.PickerInternals.Field";
        let arg00;
        const __currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_PickerInternals_FieldProps_Static_Z52658B7F(Actions$1__GetModel(actions), props.Value, props.Validity, props.ItemView, void 0, void 0, void 0, some(props.Label), some(props.Placeholder));
        const __currClass = format("{0}", TopLevelBlockClass) + nthChildClasses(__sibI, __sibC);
        const __currStyles = findApplicableStyles(__mergedStyles, __currClass);
        const __implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty())))));
        arg00 = injectImplicitProps(__implicitProps, __currExplicitProps);
        return Make()(arg00)([]);
    })));
}

