import { map, defaultArg } from "../../../../../AppSample2/src/fable_modules/fable-library.3.7.6/Option.js";
import { Props$1, OnSubscriptionKeyChange as OnSubscriptionKeyChange_1 } from "../../../Components/Subscribe/Subscribe.typext.fs.js";
import { JsUndefined } from "../../../AdditionalAutoOpens.fs.js";

export function LibClient_Components_TypeExtensions_TEs__TEs_MakeSubscribeProps_Static_3CB2E840(pSubscription, pWith, pOnSubscriptionKeyChange, pSubscriptionKey, pkey, pSubscriptionKeyOption, pkeyOption) {
    const OnSubscriptionKeyChange = defaultArg(pOnSubscriptionKeyChange, new OnSubscriptionKeyChange_1(0));
    return new Props$1(pSubscription, defaultArg(pSubscriptionKeyOption, defaultArg(map((arg0) => arg0, pSubscriptionKey), void 0)), OnSubscriptionKeyChange, pWith, defaultArg(pkeyOption, defaultArg(map((arg0_1) => arg0_1, pkey), JsUndefined())));
}

