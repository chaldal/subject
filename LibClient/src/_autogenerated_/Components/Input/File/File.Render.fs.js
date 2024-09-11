import { map as map_1, defaultArg } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Helpers_extractProp } from "../../../../ReactXP/ReactXPBindings.fs.js";
import { injectImplicitProps, prepareStylesForPassingToReactXpComponent, findApplicableStyles, mergeComponentAndPropsStyles } from "../../../../ReactXP/Styles/Runtime.fs.js";
import { RenderResult_Make_410EF94B, RenderResult, makeTextNode, RenderResult_Make_6E3A73D, RenderResult_ToRawElements, RenderResult_Make_2B31D457, nthChildClasses, RenderResult_Make_E25108F, RenderResult_ToRawElementsSingle, RenderResult_ToSingleElement_6E3A73D } from "../../../../RenderResult.fs.js";
import { Make } from "../../../../ReactXP/Components/View/View.typext.fs.js";
import { ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755 } from "../../../ReactXP/Components/View/View.TypeExtensions.fs.js";
import { join, format } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/String.js";
import { TopLevelBlockClass } from "../../../../RenderHelpers.fs.js";
import { empty, singleton, append, delay, toList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Seq.js";
import { length, singleton as singleton_1, ofArray, isEmpty } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/List.js";
import { Make as Make_1 } from "../../../../Components/With/RefDom/RefDom.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_RefDomProps_Static_Z2FB817EF } from "../../With/RefDom/RefDom.TypeExtensions.fs.js";
import { map } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Set.js";
import { Actions__OnDropZoneInitialize_Z5966C024, Actions__OnInputInitialize_Z5966C024, Actions__OnSelectPress, AcceptedType__get_Value } from "../../../../Components/Input/File/File.typext.fs.js";
import { equals, comparePrimitives } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Util.js";
import { Prop, HTMLAttr } from "../../../../../../AppSample2/src/fable_modules/Fable.React.5.3.6/Fable.React.Props.fs.js";
import * as react from "react";
import { keyValueList } from "../../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/MapUtil.js";
import { Actionable, Make as Make_2 } from "../../../../Components/Button/Button.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52 } from "../../Button/Button.TypeExtensions.fs.js";
import { InputValidity, InputValidity__get_InvalidReason, ButtonHighLevelStateFactory_MakeLowLevel_3536472E } from "../../../../Input.fs.js";
import { PositiveInteger__get_Value } from "../../../../../../LibLangFsharp/src/Positive.fs.js";
import { Make as Make_3 } from "../../../../Components/Text/Text.typext.fs.js";
import { LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7 } from "../../Text/Text.TypeExtensions.fs.js";
import { kBToMB } from "../../../../../../LibLifeCycleTypes/src/File.fs.js";
import { noElement } from "../../../../EggShellReact.fs.js";
import { Option_getOrElse } from "../../../../../../LibLangFsharp/src/OptionExtensions.fs.js";

export function render(props, estate, pstate, actions, __componentStyles) {
    [props, estate, void 0, actions];
    const __class = defaultArg(Helpers_extractProp("ClassName", props), "");
    const __mergedStyles = mergeComponentAndPropsStyles(__componentStyles, props);
    const __parentFQN = void 0;
    return RenderResult_ToSingleElement_6E3A73D(RenderResult_ToRawElementsSingle(__parentFQN, RenderResult_Make_E25108F((tupledArg) => {
        let __currExplicitProps, __currClass, __currStyles, __implicitProps, __list_20;
        const __sibI = tupledArg[0] | 0;
        const __sibC = tupledArg[1] | 0;
        const __pFQN = tupledArg[2];
        const __parentFQN_1 = "ReactXP.Components.View";
        return Make((__currExplicitProps = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass = (format("{0}{1}{2}", "view ", TopLevelBlockClass, "") + nthChildClasses(__sibI, __sibC)), (__currStyles = findApplicableStyles(__mergedStyles, __currClass), (__implicitProps = toList(delay(() => append((__currClass !== "") ? singleton(["ClassName", __currClass]) : empty(), delay(() => ((!isEmpty(__currStyles)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles)]) : empty()))))), injectImplicitProps(__implicitProps, __currExplicitProps))))))((__list_20 = singleton_1(RenderResult_Make_E25108F((tupledArg_1) => {
            let __currExplicitProps_14, __currClass_14, __currStyles_14, __implicitProps_14;
            const __sibI_1 = tupledArg_1[0] | 0;
            const __sibC_1 = tupledArg_1[1] | 0;
            const __pFQN_1 = tupledArg_1[2];
            const __parentFQN_2 = "LibClient.Components.With.RefDom";
            return Make_1((__currExplicitProps_14 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_RefDomProps_Static_Z2FB817EF((tupledArg_2) => {
                let __list_19, children_1, __list_18;
                const bindDivRef = tupledArg_2[0];
                const children_3 = (__list_19 = singleton_1(RenderResult_Make_2B31D457((children_1 = ((__list_18 = ofArray([RenderResult_Make_E25108F((tupledArg_3) => {
                    let __currExplicitProps_2, __currClass_2, __currStyles_2, __implicitProps_2;
                    const __sibI_2 = tupledArg_3[0] | 0;
                    const __sibC_2 = tupledArg_3[1] | 0;
                    const __pFQN_2 = tupledArg_3[2];
                    const __parentFQN_3 = "LibClient.Components.With.RefDom";
                    return Make_1((__currExplicitProps_2 = LibClient_Components_TypeExtensions_TEs__TEs_MakeWith_RefDomProps_Static_Z2FB817EF((tupledArg_4) => {
                        let __list, props_1;
                        const bindRef = tupledArg_4[0];
                        const input = tupledArg_4[1];
                        const children = (__list = ofArray([RenderResult_Make_2B31D457((props_1 = [new HTMLAttr(2, join(", ", map(AcceptedType__get_Value, props.AcceptedTypes, {
                            Compare: comparePrimitives,
                        }))), new HTMLAttr(121, true), new HTMLAttr(92, true), new Prop(1, bindRef), new HTMLAttr(159, "file")], react.createElement("input", keyValueList(props_1, 1)))), RenderResult_Make_E25108F((tupledArg_5) => {
                            let __currExplicitProps_1, __currClass_1, __currStyles_1, __implicitProps_1;
                            const __sibI_3 = tupledArg_5[0] | 0;
                            const __sibC_3 = tupledArg_5[1] | 0;
                            const __pFQN_3 = tupledArg_5[2];
                            const __parentFQN_4 = "LibClient.Components.Button";
                            return Make_2((__currExplicitProps_1 = LibClient_Components_TypeExtensions_TEs__TEs_MakeButtonProps_Static_7E50FB52("Select File", ButtonHighLevelStateFactory_MakeLowLevel_3536472E(Actionable((arg10_3) => {
                                Actions__OnSelectPress(actions, input, arg10_3);
                            }))), (__currClass_1 = nthChildClasses(__sibI_3, __sibC_3), (__currStyles_1 = findApplicableStyles(__mergedStyles, __currClass_1), (__implicitProps_1 = toList(delay(() => append((__currClass_1 !== "") ? singleton(["ClassName", __currClass_1]) : empty(), delay(() => ((!isEmpty(__currStyles_1)) ? singleton(["__style", __currStyles_1]) : empty()))))), injectImplicitProps(__implicitProps_1, __currExplicitProps_1))))))([]);
                        })]), RenderResult_ToRawElements(__parentFQN_3, __list));
                        return react.createElement(react.Fragment, {}, ...children);
                    }, (arg00_7) => {
                        Actions__OnInputInitialize_Z5966C024(actions, arg00_7);
                    }), (__currClass_2 = nthChildClasses(__sibI_2, __sibC_2), (__currStyles_2 = findApplicableStyles(__mergedStyles, __currClass_2), (__implicitProps_2 = toList(delay(() => append((__currClass_2 !== "") ? singleton(["ClassName", __currClass_2]) : empty(), delay(() => ((!isEmpty(__currStyles_2)) ? singleton(["__style", __currStyles_2]) : empty()))))), injectImplicitProps(__implicitProps_2, __currExplicitProps_2))))))([]);
                }), RenderResult_Make_E25108F((tupledArg_6) => {
                    let __currExplicitProps_3, __currClass_3, __currStyles_3, __implicitProps_3, __list_4;
                    const __sibI_4 = tupledArg_6[0] | 0;
                    const __sibC_4 = tupledArg_6[1] | 0;
                    const __pFQN_4 = tupledArg_6[2];
                    const __parentFQN_5 = "ReactXP.Components.View";
                    return Make((__currExplicitProps_3 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_3 = ("drag-and-drop-message" + nthChildClasses(__sibI_4, __sibC_4)), (__currStyles_3 = findApplicableStyles(__mergedStyles, __currClass_3), (__implicitProps_3 = toList(delay(() => append((__currClass_3 !== "") ? singleton(["ClassName", __currClass_3]) : empty(), delay(() => ((!isEmpty(__currStyles_3)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_3)]) : empty()))))), injectImplicitProps(__implicitProps_3, __currExplicitProps_3))))))((__list_4 = singleton_1(RenderResult_Make_E25108F((tupledArg_7) => {
                        let matchValue, maxFileCount, maxFileCount_1, __list_1, __list_2, __currExplicitProps_4, __currClass_4, __currStyles_4, __implicitProps_4;
                        const __sibI_5 = tupledArg_7[0] | 0;
                        const __sibC_5 = tupledArg_7[1] | 0;
                        const __pFQN_5 = tupledArg_7[2];
                        const __parentFQN_6 = "LibClient.Components.Text";
                        let arg10_5;
                        const __list_3 = singleton_1((matchValue = props.MaxFileCount, (matchValue != null) ? (((maxFileCount = matchValue, PositiveInteger__get_Value(maxFileCount) === 1)) ? ((maxFileCount_1 = matchValue, RenderResult_Make_6E3A73D((__list_1 = singleton_1(RenderResult_Make_E25108F((tupledArg_8) => makeTextNode("or drag and drop file here", tupledArg_8[0], tupledArg_8[1], tupledArg_8[2]))), RenderResult_ToRawElements(__parentFQN_6, __list_1))))) : RenderResult_Make_6E3A73D((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_9) => makeTextNode("or drag and drop files here", tupledArg_9[0], tupledArg_9[1], tupledArg_9[2]))), RenderResult_ToRawElements(__parentFQN_6, __list_2)))) : RenderResult_Make_6E3A73D((__list_2 = singleton_1(RenderResult_Make_E25108F((tupledArg_9) => makeTextNode("or drag and drop files here", tupledArg_9[0], tupledArg_9[1], tupledArg_9[2]))), RenderResult_ToRawElements(__parentFQN_6, __list_2)))));
                        arg10_5 = RenderResult_ToRawElements(__parentFQN_6, __list_3);
                        return Make_3((__currExplicitProps_4 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_4 = ("text-center" + nthChildClasses(__sibI_5, __sibC_5)), (__currStyles_4 = findApplicableStyles(__mergedStyles, __currClass_4), (__implicitProps_4 = toList(delay(() => append((__currClass_4 !== "") ? singleton(["ClassName", __currClass_4]) : empty(), delay(() => ((!isEmpty(__currStyles_4)) ? singleton(["__style", __currStyles_4]) : empty()))))), injectImplicitProps(__implicitProps_4, __currExplicitProps_4))))))(arg10_5);
                    })), RenderResult_ToRawElements(__parentFQN_5, __list_4)));
                }), RenderResult_Make_E25108F((tupledArg_10) => {
                    let __currExplicitProps_5, __currClass_5, __currStyles_5, __implicitProps_5, __list_10;
                    const __sibI_6 = tupledArg_10[0] | 0;
                    const __sibC_6 = tupledArg_10[1] | 0;
                    const __pFQN_6 = tupledArg_10[2];
                    const __parentFQN_7 = "ReactXP.Components.View";
                    return Make((__currExplicitProps_5 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_5 = ("constrain-message" + nthChildClasses(__sibI_6, __sibC_6)), (__currStyles_5 = findApplicableStyles(__mergedStyles, __currClass_5), (__implicitProps_5 = toList(delay(() => append((__currClass_5 !== "") ? singleton(["ClassName", __currClass_5]) : empty(), delay(() => ((!isEmpty(__currStyles_5)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_5)]) : empty()))))), injectImplicitProps(__implicitProps_5, __currExplicitProps_5))))))((__list_10 = singleton_1(RenderResult_Make_E25108F((tupledArg_11) => {
                        let matchValue_1, maxFileSize, maxFileCount_2, maxFileCount_3, maxFileSize_1, __list_5, text_2, maxFileCount_4, maxFileCount_5, __list_6, text_3, maxFileSize_2, __list_7, text_4, __list_8, __currExplicitProps_6, __currClass_6, __currStyles_6, __implicitProps_6;
                        const __sibI_7 = tupledArg_11[0] | 0;
                        const __sibC_7 = tupledArg_11[1] | 0;
                        const __pFQN_7 = tupledArg_11[2];
                        const __parentFQN_8 = "LibClient.Components.Text";
                        let arg10_10;
                        const __list_9 = singleton_1((matchValue_1 = [props.MaxFileCount, props.MaxFileSize], (matchValue_1[0] != null) ? ((matchValue_1[1] != null) ? (((maxFileSize = (matchValue_1[1] | 0), (maxFileCount_2 = matchValue_1[0], PositiveInteger__get_Value(maxFileCount_2) > 1))) ? ((maxFileCount_3 = matchValue_1[0], (maxFileSize_1 = (matchValue_1[1] | 0), RenderResult_Make_6E3A73D((__list_5 = singleton_1(RenderResult_Make_E25108F((text_2 = format("{0}{1}{2}{3}{4}", "Maximum ", PositiveInteger__get_Value(maxFileCount_3), " files each below ", kBToMB(maxFileSize_1), " MB"), (tupledArg_12) => makeTextNode(text_2, tupledArg_12[0], tupledArg_12[1], tupledArg_12[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_5)))))) : ((matchValue_1[0] != null) ? ((matchValue_1[1] == null) ? (((maxFileCount_4 = matchValue_1[0], PositiveInteger__get_Value(maxFileCount_4) > 1)) ? ((maxFileCount_5 = matchValue_1[0], RenderResult_Make_6E3A73D((__list_6 = singleton_1(RenderResult_Make_E25108F((text_3 = format("{0}{1}{2}", "Maximum ", PositiveInteger__get_Value(maxFileCount_5), " files"), (tupledArg_13) => makeTextNode(text_3, tupledArg_13[0], tupledArg_13[1], tupledArg_13[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_6))))) : ((matchValue_1[1] != null) ? ((maxFileSize_2 = (matchValue_1[1] | 0), RenderResult_Make_6E3A73D((__list_7 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}{1}{2}", "Size below ", kBToMB(maxFileSize_2), " MB"), (tupledArg_14) => makeTextNode(text_4, tupledArg_14[0], tupledArg_14[1], tupledArg_14[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_7))))) : RenderResult_Make_6E3A73D((__list_8 = singleton_1(RenderResult_Make_2B31D457(noElement)), RenderResult_ToRawElements(__parentFQN_8, __list_8))))) : ((matchValue_1[1] != null) ? ((maxFileSize_2 = (matchValue_1[1] | 0), RenderResult_Make_6E3A73D((__list_7 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}{1}{2}", "Size below ", kBToMB(maxFileSize_2), " MB"), (tupledArg_14) => makeTextNode(text_4, tupledArg_14[0], tupledArg_14[1], tupledArg_14[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_7))))) : RenderResult_Make_6E3A73D((__list_8 = singleton_1(RenderResult_Make_2B31D457(noElement)), RenderResult_ToRawElements(__parentFQN_8, __list_8))))) : ((matchValue_1[1] != null) ? ((maxFileSize_2 = (matchValue_1[1] | 0), RenderResult_Make_6E3A73D((__list_7 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}{1}{2}", "Size below ", kBToMB(maxFileSize_2), " MB"), (tupledArg_14) => makeTextNode(text_4, tupledArg_14[0], tupledArg_14[1], tupledArg_14[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_7))))) : RenderResult_Make_6E3A73D((__list_8 = singleton_1(RenderResult_Make_2B31D457(noElement)), RenderResult_ToRawElements(__parentFQN_8, __list_8)))))) : ((matchValue_1[0] != null) ? ((matchValue_1[1] == null) ? (((maxFileCount_4 = matchValue_1[0], PositiveInteger__get_Value(maxFileCount_4) > 1)) ? ((maxFileCount_5 = matchValue_1[0], RenderResult_Make_6E3A73D((__list_6 = singleton_1(RenderResult_Make_E25108F((text_3 = format("{0}{1}{2}", "Maximum ", PositiveInteger__get_Value(maxFileCount_5), " files"), (tupledArg_13) => makeTextNode(text_3, tupledArg_13[0], tupledArg_13[1], tupledArg_13[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_6))))) : ((matchValue_1[1] != null) ? ((maxFileSize_2 = (matchValue_1[1] | 0), RenderResult_Make_6E3A73D((__list_7 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}{1}{2}", "Size below ", kBToMB(maxFileSize_2), " MB"), (tupledArg_14) => makeTextNode(text_4, tupledArg_14[0], tupledArg_14[1], tupledArg_14[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_7))))) : RenderResult_Make_6E3A73D((__list_8 = singleton_1(RenderResult_Make_2B31D457(noElement)), RenderResult_ToRawElements(__parentFQN_8, __list_8))))) : ((matchValue_1[1] != null) ? ((maxFileSize_2 = (matchValue_1[1] | 0), RenderResult_Make_6E3A73D((__list_7 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}{1}{2}", "Size below ", kBToMB(maxFileSize_2), " MB"), (tupledArg_14) => makeTextNode(text_4, tupledArg_14[0], tupledArg_14[1], tupledArg_14[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_7))))) : RenderResult_Make_6E3A73D((__list_8 = singleton_1(RenderResult_Make_2B31D457(noElement)), RenderResult_ToRawElements(__parentFQN_8, __list_8))))) : ((matchValue_1[1] != null) ? ((maxFileSize_2 = (matchValue_1[1] | 0), RenderResult_Make_6E3A73D((__list_7 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}{1}{2}", "Size below ", kBToMB(maxFileSize_2), " MB"), (tupledArg_14) => makeTextNode(text_4, tupledArg_14[0], tupledArg_14[1], tupledArg_14[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_7))))) : RenderResult_Make_6E3A73D((__list_8 = singleton_1(RenderResult_Make_2B31D457(noElement)), RenderResult_ToRawElements(__parentFQN_8, __list_8)))))) : ((matchValue_1[0] != null) ? ((matchValue_1[1] == null) ? (((maxFileCount_4 = matchValue_1[0], PositiveInteger__get_Value(maxFileCount_4) > 1)) ? ((maxFileCount_5 = matchValue_1[0], RenderResult_Make_6E3A73D((__list_6 = singleton_1(RenderResult_Make_E25108F((text_3 = format("{0}{1}{2}", "Maximum ", PositiveInteger__get_Value(maxFileCount_5), " files"), (tupledArg_13) => makeTextNode(text_3, tupledArg_13[0], tupledArg_13[1], tupledArg_13[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_6))))) : ((matchValue_1[1] != null) ? ((maxFileSize_2 = (matchValue_1[1] | 0), RenderResult_Make_6E3A73D((__list_7 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}{1}{2}", "Size below ", kBToMB(maxFileSize_2), " MB"), (tupledArg_14) => makeTextNode(text_4, tupledArg_14[0], tupledArg_14[1], tupledArg_14[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_7))))) : RenderResult_Make_6E3A73D((__list_8 = singleton_1(RenderResult_Make_2B31D457(noElement)), RenderResult_ToRawElements(__parentFQN_8, __list_8))))) : ((matchValue_1[1] != null) ? ((maxFileSize_2 = (matchValue_1[1] | 0), RenderResult_Make_6E3A73D((__list_7 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}{1}{2}", "Size below ", kBToMB(maxFileSize_2), " MB"), (tupledArg_14) => makeTextNode(text_4, tupledArg_14[0], tupledArg_14[1], tupledArg_14[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_7))))) : RenderResult_Make_6E3A73D((__list_8 = singleton_1(RenderResult_Make_2B31D457(noElement)), RenderResult_ToRawElements(__parentFQN_8, __list_8))))) : ((matchValue_1[1] != null) ? ((maxFileSize_2 = (matchValue_1[1] | 0), RenderResult_Make_6E3A73D((__list_7 = singleton_1(RenderResult_Make_E25108F((text_4 = format("{0}{1}{2}", "Size below ", kBToMB(maxFileSize_2), " MB"), (tupledArg_14) => makeTextNode(text_4, tupledArg_14[0], tupledArg_14[1], tupledArg_14[2])))), RenderResult_ToRawElements(__parentFQN_8, __list_7))))) : RenderResult_Make_6E3A73D((__list_8 = singleton_1(RenderResult_Make_2B31D457(noElement)), RenderResult_ToRawElements(__parentFQN_8, __list_8)))))));
                        arg10_10 = RenderResult_ToRawElements(__parentFQN_8, __list_9);
                        return Make_3((__currExplicitProps_6 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_6 = ("text-center" + nthChildClasses(__sibI_7, __sibC_7)), (__currStyles_6 = findApplicableStyles(__mergedStyles, __currClass_6), (__implicitProps_6 = toList(delay(() => append((__currClass_6 !== "") ? singleton(["ClassName", __currClass_6]) : empty(), delay(() => ((!isEmpty(__currStyles_6)) ? singleton(["__style", __currStyles_6]) : empty()))))), injectImplicitProps(__implicitProps_6, __currExplicitProps_6))))))(arg10_10);
                    })), RenderResult_ToRawElements(__parentFQN_7, __list_10)));
                }), RenderResult_Make_E25108F((tupledArg_15) => {
                    let __currExplicitProps_7, __currClass_7, __currStyles_7, __implicitProps_7, __list_13;
                    const __sibI_8 = tupledArg_15[0] | 0;
                    const __sibC_8 = tupledArg_15[1] | 0;
                    const __pFQN_8 = tupledArg_15[2];
                    const __parentFQN_9 = "ReactXP.Components.View";
                    return Make((__currExplicitProps_7 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_7 = nthChildClasses(__sibI_8, __sibC_8), (__currStyles_7 = findApplicableStyles(__mergedStyles, __currClass_7), (__implicitProps_7 = toList(delay(() => append((__currClass_7 !== "") ? singleton(["ClassName", __currClass_7]) : empty(), delay(() => ((!isEmpty(__currStyles_7)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_7)]) : empty()))))), injectImplicitProps(__implicitProps_7, __currExplicitProps_7))))))((__list_13 = ofArray([(length(props.Value) === 1) ? RenderResult_Make_E25108F((tupledArg_16) => {
                        let text_5, __currExplicitProps_8, __currClass_8, __currStyles_8, __implicitProps_8;
                        const __sibI_9 = tupledArg_16[0] | 0;
                        const __sibC_9 = tupledArg_16[1] | 0;
                        const __pFQN_9 = tupledArg_16[2];
                        const __parentFQN_10 = "LibClient.Components.Text";
                        let arg10_17;
                        const __list_11 = singleton_1(RenderResult_Make_E25108F((text_5 = format("{0}{1}{2}", "", length(props.Value), " file selected"), (tupledArg_17) => makeTextNode(text_5, tupledArg_17[0], tupledArg_17[1], tupledArg_17[2]))));
                        arg10_17 = RenderResult_ToRawElements(__parentFQN_10, __list_11);
                        return Make_3((__currExplicitProps_8 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_8 = ("text-center" + nthChildClasses(__sibI_9, __sibC_9)), (__currStyles_8 = findApplicableStyles(__mergedStyles, __currClass_8), (__implicitProps_8 = toList(delay(() => append((__currClass_8 !== "") ? singleton(["ClassName", __currClass_8]) : empty(), delay(() => ((!isEmpty(__currStyles_8)) ? singleton(["__style", __currStyles_8]) : empty()))))), injectImplicitProps(__implicitProps_8, __currExplicitProps_8))))))(arg10_17);
                    }) : (new RenderResult(0)), (length(props.Value) > 1) ? RenderResult_Make_E25108F((tupledArg_18) => {
                        let text_6, __currExplicitProps_9, __currClass_9, __currStyles_9, __implicitProps_9;
                        const __sibI_10 = tupledArg_18[0] | 0;
                        const __sibC_10 = tupledArg_18[1] | 0;
                        const __pFQN_10 = tupledArg_18[2];
                        const __parentFQN_11 = "LibClient.Components.Text";
                        let arg10_19;
                        const __list_12 = singleton_1(RenderResult_Make_E25108F((text_6 = format("{0}{1}{2}", "", length(props.Value), " files selected"), (tupledArg_19) => makeTextNode(text_6, tupledArg_19[0], tupledArg_19[1], tupledArg_19[2]))));
                        arg10_19 = RenderResult_ToRawElements(__parentFQN_11, __list_12);
                        return Make_3((__currExplicitProps_9 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_9 = ("text-center" + nthChildClasses(__sibI_10, __sibC_10)), (__currStyles_9 = findApplicableStyles(__mergedStyles, __currClass_9), (__implicitProps_9 = toList(delay(() => append((__currClass_9 !== "") ? singleton(["ClassName", __currClass_9]) : empty(), delay(() => ((!isEmpty(__currStyles_9)) ? singleton(["__style", __currStyles_9]) : empty()))))), injectImplicitProps(__implicitProps_9, __currExplicitProps_9))))))(arg10_19);
                    }) : (new RenderResult(0))]), RenderResult_ToRawElements(__parentFQN_9, __list_13)));
                }), Option_getOrElse(new RenderResult(0), map_1((reason) => RenderResult_Make_410EF94B(singleton_1(RenderResult_Make_E25108F((tupledArg_20) => {
                    let __currExplicitProps_10, __currClass_10, __currStyles_10, __implicitProps_10, __list_15;
                    const __sibI_11 = tupledArg_20[0] | 0;
                    const __sibC_11 = tupledArg_20[1] | 0;
                    const __pFQN_11 = tupledArg_20[2];
                    const __parentFQN_12 = "ReactXP.Components.View";
                    return Make((__currExplicitProps_10 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_10 = nthChildClasses(__sibI_11, __sibC_11), (__currStyles_10 = findApplicableStyles(__mergedStyles, __currClass_10), (__implicitProps_10 = toList(delay(() => append((__currClass_10 !== "") ? singleton(["ClassName", __currClass_10]) : empty(), delay(() => ((!isEmpty(__currStyles_10)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_10)]) : empty()))))), injectImplicitProps(__implicitProps_10, __currExplicitProps_10))))))((__list_15 = singleton_1(RenderResult_Make_E25108F((tupledArg_21) => {
                        let text_7, __currExplicitProps_11, __currClass_11, __currStyles_11, __implicitProps_11;
                        const __sibI_12 = tupledArg_21[0] | 0;
                        const __sibC_12 = tupledArg_21[1] | 0;
                        const __pFQN_12 = tupledArg_21[2];
                        const __parentFQN_13 = "LibClient.Components.Text";
                        let arg10_22;
                        const __list_14 = singleton_1(RenderResult_Make_E25108F((text_7 = format("{0}", reason), (tupledArg_22) => makeTextNode(text_7, tupledArg_22[0], tupledArg_22[1], tupledArg_22[2]))));
                        arg10_22 = RenderResult_ToRawElements(__parentFQN_13, __list_14);
                        return Make_3((__currExplicitProps_11 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_11 = ("invalid-reason" + nthChildClasses(__sibI_12, __sibC_12)), (__currStyles_11 = findApplicableStyles(__mergedStyles, __currClass_11), (__implicitProps_11 = toList(delay(() => append((__currClass_11 !== "") ? singleton(["ClassName", __currClass_11]) : empty(), delay(() => ((!isEmpty(__currStyles_11)) ? singleton(["__style", __currStyles_11]) : empty()))))), injectImplicitProps(__implicitProps_11, __currExplicitProps_11))))))(arg10_22);
                    })), RenderResult_ToRawElements(__parentFQN_12, __list_15)));
                }))), (InputValidity__get_InvalidReason(estate.InternalValidity) != null) ? InputValidity__get_InvalidReason(estate.InternalValidity) : InputValidity__get_InvalidReason(props.Validity))), equals(props.Validity, new InputValidity(1)) ? RenderResult_Make_E25108F((tupledArg_23) => {
                    let __currExplicitProps_12, __currClass_12, __currStyles_12, __implicitProps_12, __list_17;
                    const __sibI_13 = tupledArg_23[0] | 0;
                    const __sibC_13 = tupledArg_23[1] | 0;
                    const __pFQN_13 = tupledArg_23[2];
                    const __parentFQN_14 = "ReactXP.Components.View";
                    return Make((__currExplicitProps_12 = ReactXP_Components_TypeExtensions_TEs__TEs_MakeViewProps_Static_1BFB6755(), (__currClass_12 = nthChildClasses(__sibI_13, __sibC_13), (__currStyles_12 = findApplicableStyles(__mergedStyles, __currClass_12), (__implicitProps_12 = toList(delay(() => append((__currClass_12 !== "") ? singleton(["ClassName", __currClass_12]) : empty(), delay(() => ((!isEmpty(__currStyles_12)) ? singleton(["style", prepareStylesForPassingToReactXpComponent("ReactXP.Components.View", __currStyles_12)]) : empty()))))), injectImplicitProps(__implicitProps_12, __currExplicitProps_12))))))((__list_17 = singleton_1(RenderResult_Make_E25108F((tupledArg_24) => {
                        let __currExplicitProps_13, __currClass_13, __currStyles_13, __implicitProps_13;
                        const __sibI_14 = tupledArg_24[0] | 0;
                        const __sibC_14 = tupledArg_24[1] | 0;
                        const __pFQN_14 = tupledArg_24[2];
                        const __parentFQN_15 = "LibClient.Components.Text";
                        let arg10_25;
                        const __list_16 = singleton_1(RenderResult_Make_E25108F((tupledArg_25) => makeTextNode("This field is required", tupledArg_25[0], tupledArg_25[1], tupledArg_25[2])));
                        arg10_25 = RenderResult_ToRawElements(__parentFQN_15, __list_16);
                        return Make_3((__currExplicitProps_13 = LibClient_Components_TypeExtensions_TEs__TEs_MakeTextProps_Static_373F7BF7(), (__currClass_13 = ("invalid-reason" + nthChildClasses(__sibI_14, __sibC_14)), (__currStyles_13 = findApplicableStyles(__mergedStyles, __currClass_13), (__implicitProps_13 = toList(delay(() => append((__currClass_13 !== "") ? singleton(["ClassName", __currClass_13]) : empty(), delay(() => ((!isEmpty(__currStyles_13)) ? singleton(["__style", __currStyles_13]) : empty()))))), injectImplicitProps(__implicitProps_13, __currExplicitProps_13))))))(arg10_25);
                    })), RenderResult_ToRawElements(__parentFQN_14, __list_17)));
                }) : (new RenderResult(0))]), RenderResult_ToRawElements(__parentFQN_2, __list_18))), react.createElement("div", {
                    ref: bindDivRef,
                }, ...children_1)))), RenderResult_ToRawElements(__parentFQN_2, __list_19));
                return react.createElement(react.Fragment, {}, ...children_3);
            }, (arg00_62) => {
                Actions__OnDropZoneInitialize_Z5966C024(actions, arg00_62);
            }), (__currClass_14 = nthChildClasses(__sibI_1, __sibC_1), (__currStyles_14 = findApplicableStyles(__mergedStyles, __currClass_14), (__implicitProps_14 = toList(delay(() => append((__currClass_14 !== "") ? singleton(["ClassName", __currClass_14]) : empty(), delay(() => ((!isEmpty(__currStyles_14)) ? singleton(["__style", __currStyles_14]) : empty()))))), injectImplicitProps(__implicitProps_14, __currExplicitProps_14))))))([]);
        })), RenderResult_ToRawElements(__parentFQN_1, __list_20)));
    })));
}

