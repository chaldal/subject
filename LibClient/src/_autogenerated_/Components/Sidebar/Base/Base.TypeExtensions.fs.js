import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { noElement } from "../../../../EggShellReact.fs.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../Components/Sidebar/Base/Base.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeSidebar_BaseProps_Static_Z58A16E89(pFixedTop, pFixedBottom, pScrollableMiddle, pkey, pkeyOption) {
    return new Props(defaultArg(pFixedTop, noElement), defaultArg(pFixedBottom, noElement), defaultArg(pScrollableMiddle, noElement), defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

