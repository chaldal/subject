export const Void: void = <void>undefined;
export function Identity<T>(_: T): T { return _; }
export function ReturnTrue(...args: Array<any>): true { return true; }

export { Noop } from "./Noop";
export { Option, Some, None } from "./Option";
export { Either, Left, Right, eitherFromWire } from "./Either";
export { Try } from "./Try";
export { Lazy } from "./Lazy";
export { LazyValue } from "./LazyValue";
export { T2, T3 } from "./tuples";
export { update, updatePicked, updateBase, updateSub } from "./update";
export { QQQ } from "./implement";
export { ifMatch } from "./IfMatch";
export { coalesce } from "./coalesce";
export { ResultCallback, CompletionCallback } from "./callbacks";

import * as Options from "./Options";
export { Options };

import * as Tries from "./Tries";
export { Tries };

import * as Promises from "./Promises";
export { Promises };
