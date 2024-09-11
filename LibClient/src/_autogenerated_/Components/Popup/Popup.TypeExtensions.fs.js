import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props } from "../../../Components/Popup/Popup.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakePopupProps_Static_Z200491B8(pRender, pConnector, pId, pIdOption) {
    return new Props(pRender, pConnector, defaultArg(pIdOption, defaultArg(map((arg0) => arg0, pId), void 0)));
}

