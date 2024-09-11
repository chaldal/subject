import { some, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_ToRawElements, RenderResult, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { ofArray, map, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeThumbsProps_Static_Z70D2C236 } from "../../Thumbs/Thumbs.TypeExtensions.fs.js";
import { Make as Make_1, PropForFactory_Make_Z328837CF } from "../../../../Components/Thumbs/Thumbs.typext.fs.js";
import { ImageSourceModule_ofUrl } from "../../../../Services/ImageService/ImageService.fs.js";
import { File__get_ToDataUri } from "../../../../../../LibLifeCycleTypes/src/File.fs.js";
import { Actions__RemoveSelected_411CDB90, Actions__ToggleSelectedFilesForRemoval_Z6EF827D7 } from "../../../../Components/Input/Image/Image.typext.fs.js";
import { Microsoft_FSharp_Collections_FSharpSet$1__Set$1_get_IsNonempty } from "../../../../../../LibLangFsharp/src/SetExtensions.fs.js";
import { Actionable, Make as Make_2 } from "../../../../Components/TextButton/TextButton.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextButtonProps_Static_Z17FE0D32 } from "../../TextButton/TextButton.TypeExtensions.fs.js";
import { ButtonHighLevelStateFactory_MakeLowLevel_3536472E } from "../../../../Input.fs.js";
import { SelectionMode, AcceptedType, Make as Make_3 } from "../../../../Components/Input/File/File.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_FileProps_Static_Z7898F858 } from "../File/File.TypeExtensions.fs.js";
import { ofSeq } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Set.js";
import { compare } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = ("view" + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list = ofArray([props.ShowPreview ? RenderResult_Make_E25108F((tupledArg_1) => {
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "LibClient.Components.Thumbs";
            let arg00;
            const __currExplicitProps_1 = LibClient_Components_TypeExtensions_TEs__TEs_MakeThumbsProps_Static_Z70D2C236(PropForFactory_Make_Z328837CF(map((file) => ImageSourceModule_ofUrl(File__get_ToDataUri(file)), props.Value)), estate.SelectedFiles, (_arg2, index, _arg1) => {
                Actions__ToggleSelectedFilesForRemoval_Z6EF827D7(actions, index);
            });
            const __currClass_1 = "image-thumbs" + nthChildClasses(__sibI_1, __sibC_1);
            const __currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1);
            const __implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["__style", __currStyles_1]) : empty())))));
            arg00 = injectImplicitProps(__implicitProps_1, __currExplicitProps_1);
            return Make_1()(arg00)([]);
        }) : (new RenderResult(0)), Microsoft_FSharp_Collections_FSharpSet$1__Set$1_get_IsNonempty(estate.SelectedIndicesForRemoval) ? RenderResult_Make_E25108F((tupledArg_2) => {
            let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
            const __sibI_2 = tupledArg_2[0] | 0;
            const __sibC_2 = tupledArg_2[1] | 0;
            const __pFQN_2 = tupledArg_2[2];
            const __parentFQN_3 = "LibClient.Components.TextButton";
            return Make_2((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextButtonProps_Static_Z17FE0D32(ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Actionable((arg00_3) => {
                Actions__RemoveSelected_411CDB90(actions, arg00_3);
            })), "Remove Selected"), (__currClass_2 = nthChildClasses(__sibI_2, __sibC_2), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))([]);
        }) : (new RenderResult(0)), RenderResult_Make_E25108F((tupledArg_3) => {
            let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3;
            const __sibI_3 = tupledArg_3[0] | 0;
            const __sibC_3 = tupledArg_3[1] | 0;
            const __pFQN_3 = tupledArg_3[2];
            const __parentFQN_4 = "LibClient.Components.Input.File";
            return Make_3((__currExplicitProps_3 = LibClient_Components_TypeExtensions_TEs__TEs_MakeInput_FileProps_Static_Z7898F858(props.Value, props.Validity, props.OnChange, ofSeq([new AcceptedType(4)], {
                Compare: compare,
            }), new SelectionMode(1), void 0, void 0, void 0, some(props.MaxFileCount), some(props.MaxFileSize)), (__currClass_3 = ("image-input" + nthChildClasses(__sibI_3, __sibC_3)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["__style", __currStyles_3]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))([]);
        })]), RenderResult_ToRawElements(__parentFQN_1, __list)));
    })));
}

