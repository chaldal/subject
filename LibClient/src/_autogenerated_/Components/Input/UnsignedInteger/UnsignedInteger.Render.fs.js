import { some, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_ParsedTextProps_Static_Z4D906D6A } from "../ParsedText/ParsedText.TypeExtensions.fs.js";
import { parseProp } from "../../../../Components/Input/UnsignedInteger/UnsignedInteger.typext.fs.js";
import { format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make } from "../../../../Components/Input/ParsedText/ParsedText.typext.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, void 0, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.Input.ParsedText";
        let arg00;
        const __currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_ParsedTextProps_Static_Z4D906D6A(parseProp, props.Value, props.Validity, props.RequestFocusOnMount, props.OnChange, void 0, void 0, void 0, void 0, void 0, void 0, void 0, some(props.Label), some(props.Placeholder), some(props.Prefix), some(props.Suffix), some(props.OnKeyPress), some(props.OnEnterKeyPress));
        const __currClass = format("{0}", TopLevelBlockClass) + nthChildClasses(__sibI, __sibC);
        const __currStyles = findApplicableStyles(__mergedStyles, __currClass);
        const __implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty())))));
        arg00 = injectImplicitProps(__implicitProps, __currExplicitProps);
        return Make()(arg00)([]);
    })));
}

