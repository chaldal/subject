import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props, NavigatorRoute } from "../../../Components/RXNavigator/RXNavigator.typext.fs.js";

export function LibRouter_Components_TypeExtensions_TEs__TEs_MakeRXNavigatorNavigatorRoute_Static_72384821(prouteId, psceneConfigType, pgestureResponseDistance, pcustomSceneConfig, pgestureResponseDistanceOption, pcustomSceneConfigOption) {
    return new NavigatorRoute(prouteId, psceneConfigType, defaultArg(pgestureResponseDistanceOption, defaultArg(map((arg0) => arg0, pgestureResponseDistance), undefined)), defaultArg(pcustomSceneConfigOption, defaultArg(map((arg0_1) => arg0_1, pcustomSceneConfig), undefined)));
}

export function LibRouter_Components_TypeExtensions_TEs__TEs_MakeRXNavigatorProps_Static_Z2EC7E068(pref, prenderScene) {
    return new Props(pref, prenderScene);
}

