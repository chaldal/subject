import { map, defaultArg } from "../../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props, Scroll as Scroll_1, BackButton as BackButton_1 } from "../../../../../Components/Dialog/Shell/FullScreen/FullScreen.typext.fs.js";
import { noElement } from "../../../../../EggShellReact.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeDialog_Shell_FullScreenProps_Static_Z1C29A00E(pBackButton, pScroll, pchildren, pHeading, pBottomSection, pHeadingOption, pBottomSectionOption) {
    const BackButton = defaultArg(pBackButton, new BackButton_1(0));
    const Scroll = defaultArg(pScroll, new Scroll_1(2));
    const children = defaultArg(pchildren, noElement);
    return new Props(defaultArg(pHeadingOption, defaultArg(map((arg0) => arg0, pHeading), void 0)), BackButton, defaultArg(pBottomSectionOption, defaultArg(map((arg0_1) => arg0_1, pBottomSection), void 0)), Scroll, children);
}

