import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { empty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Set.js";
import { compare } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Props, SelectionMode as SelectionMode_1 } from "../../../../Components/Input/File/File.typext.fs.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_FileProps_Static_Z7898F858(pValue, pValidity, pOnChange, pAcceptedTypes, pSelectionMode, pMaxFileCount, pMaxFileSize, pkey, pMaxFileCountOption, pMaxFileSizeOption, pkeyOption) {
    const AcceptedTypes = defaultArg(pAcceptedTypes, empty({
        Compare: compare,
    }));
    const SelectionMode = defaultArg(pSelectionMode, new SelectionMode_1(0));
    return new Props(pValue, pValidity, pOnChange, defaultArg(pMaxFileCountOption, defaultArg(map((arg0) => arg0, pMaxFileCount), void 0)), defaultArg(pMaxFileSizeOption, defaultArg(map((arg0_1) => arg0_1, pMaxFileSize), void 0)), AcceptedTypes, SelectionMode, defaultArg(pkeyOption, defaultArg(map((arg0_2) => arg0_2, pkey), JsUndefined())));
}

