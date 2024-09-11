import { map, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props, Size } from "../../../../ReactXP/Components/Image/Image.typext.fs.js";

export function ReactXP_Components_TypeExtensions_TEs__TEs_MakeImageProps_Static_331EB6A7(pSource, pSize, pHeaders, pAccessibilityLabel, pResizeMode, pAndroidResizeMethod, pTitle, pOnLoad, pOnError, pHeadersOption, pAccessibilityLabelOption, pResizeModeOption, pAndroidResizeMethodOption, pTitleOption, pOnLoadOption, pOnErrorOption) {
    return new Props(pSource, defaultArg(pSize, new Size(3)), defaultArg(pHeadersOption, defaultArg(map((arg0) => arg0, pHeaders), undefined)), defaultArg(pAccessibilityLabelOption, defaultArg(map((arg0_1) => arg0_1, pAccessibilityLabel), undefined)), defaultArg(pResizeModeOption, defaultArg(map((arg0_2) => arg0_2, pResizeMode), undefined)), defaultArg(pAndroidResizeMethodOption, defaultArg(map((arg0_3) => arg0_3, pAndroidResizeMethod), undefined)), defaultArg(pTitleOption, defaultArg(map((arg0_4) => arg0_4, pTitle), undefined)), defaultArg(pOnLoadOption, defaultArg(map((arg0_5) => arg0_5, pOnLoad), undefined)), defaultArg(pOnErrorOption, defaultArg(map((arg0_6) => arg0_6, pOnError), undefined)));
}

