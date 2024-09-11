import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { noElement } from "../../../../EggShellReact.fs.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";
import { Props } from "../../../../Components/AppShell/Content/Content.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeAppShell_ContentProps_Static_33F89BDF(pDesktopSidebarStyle, pSidebar, pContent, pDialogs, pOnError, pTopNav, pkey, pkeyOption) {
    return new Props(pDesktopSidebarStyle, pSidebar, defaultArg(pTopNav, noElement), pContent, pDialogs, pOnError, defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

