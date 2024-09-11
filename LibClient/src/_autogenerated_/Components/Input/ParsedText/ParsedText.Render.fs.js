import { some, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../Components/Input/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_TextProps_Static_CF9FCE9 } from "../Text/Text.TypeExtensions.fs.js";
import { Value$1_OfRaw } from "../../../../Components/Input/ParsedText/ParsedText.typext.fs.js";
import { InputValidity } from "../../../../Input.fs.js";
import { format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, matchValue, message, __currClass, __currStyles, __implicitProps;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "LibClient.Components.Input.Text";
        return Make((__currExplicitProps = LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_TextProps_Static_CF9FCE9(props.Value.Raw, (arg) => {
            props.OnChange(Value$1_OfRaw(props.Parse, arg));
        }, (matchValue = props.Value.Result, (matchValue.tag === 1) ? ((message = matchValue.fields[0], new InputValidity(2, message))) : props.Validity), false, props.RequestFocusOnMount, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, void 0, some(props.Label), some(props.OnKeyPress), some(props.OnEnterKeyPress), void 0, void 0, some(props.Placeholder), some(props.Prefix), some(props.Suffix)), (__currClass = (format("{0}{1}{2}", "theme ", TopLevelBlockClass, "") + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["__style", __currStyles]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))([]);
    })));
}

