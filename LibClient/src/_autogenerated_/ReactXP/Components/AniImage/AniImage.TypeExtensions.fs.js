import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props } from "../../../../ReactXP/Components/AniImage/AniImage.typext.fs.js";

export function ReactXP_Components_TypeExtensions_TEs__TEs_MakeAniImageProps_Static_Z45804F12(psource, pheaders, paccessibilityLabel, presizeMode, presizeMethod, ptitle, ponLoad, ponError, pheadersOption, paccessibilityLabelOption, presizeModeOption, presizeMethodOption, ptitleOption, ponLoadOption, ponErrorOption) {
    return new Props(psource, defaultArg(pheadersOption, defaultArg(map((arg0) => arg0, pheaders), undefined)), defaultArg(paccessibilityLabelOption, defaultArg(map((arg0_1) => arg0_1, paccessibilityLabel), undefined)), defaultArg(presizeModeOption, defaultArg(map((arg0_2) => arg0_2, presizeMode), undefined)), defaultArg(presizeMethodOption, defaultArg(map((arg0_3) => arg0_3, presizeMethod), undefined)), defaultArg(ptitleOption, defaultArg(map((arg0_4) => arg0_4, ptitle), undefined)), defaultArg(ponLoadOption, defaultArg(map((arg0_5) => arg0_5, ponLoad), undefined)), defaultArg(ponErrorOption, defaultArg(map((arg0_6) => arg0_6, ponError), undefined)));
}

