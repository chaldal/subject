import { map, defaultArg } from "../../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { noElement } from "../../../../../../EggShellReact.fs.js";
import { Props, Mode as Mode_1 } from "../../../../../../Components/Dialog/Shell/WhiteRounded/Standard/Standard.typext.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_Shell_WhiteRounded_StandardProps_Static_61C15D4D(pCanClose, pBody, pButtons, pMode, pHeading, pHeadingOption) {
    const Body = defaultArg(pBody, noElement);
    const Buttons = defaultArg(pButtons, noElement);
    const Mode = defaultArg(pMode, new Mode_1(0));
    return new Props(defaultArg(pHeadingOption, defaultArg(map((arg0) => arg0, pHeading), void 0)), Body, Buttons, Mode, pCanClose);
}

