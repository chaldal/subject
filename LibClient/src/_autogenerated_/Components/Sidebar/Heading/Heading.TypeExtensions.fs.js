import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props, Level } from "../../../../Components/Sidebar/Heading/Heading.typext.fs.js";
import { JsUndefined } from "../../../../AdditionalAutoOpens.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeSidebar_HeadingProps_Static_646361C8(pText, pLevel, pkey, pkeyOption) {
    return new Props(pText, defaultArg(pLevel, new Level(0)), defaultArg(pkeyOption, defaultArg(map((arg0) => arg0, pkey), JsUndefined())));
}

