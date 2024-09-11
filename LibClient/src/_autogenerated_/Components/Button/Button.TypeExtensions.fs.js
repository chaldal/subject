import { defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props, Icon, Level } from "../../../Components/Button/Button.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52(pLabel, pState, pLevel, pIcon) {
    return new Props(defaultArg(pLevel, new Level(0)), pLabel, defaultArg(pIcon, new Icon(0)), pState);
}

