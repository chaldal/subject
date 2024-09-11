export type Seq<T> = ReadonlyArray<T>;
export interface NonemptySeq<T> extends Seq<T> {
	0: T;
}

// trying to modify a frozen object errors only in strict mode, but at the very least
// we're preventing modification of a widely used common object by some rogue _.extends call or the like.
export const emptyObject: any = (typeof Object !== undefined && Object.freeze !== undefined && Object.freeze({})) || {};

export const emptySeq: Seq<any> = [];

import { None, Option, Some } from "../lang";
import * as _ from "lodash";

export function truthyKeys(data: any): Seq<string> {
	return _.reduce(data, (a, v, k) => {if (v) a.push(k); return a;}, []);
}


export function mapToTrue(keys: Seq<any>): {[key: string]: true} {
	return _.reduce(keys, (a: {[key: string]: true}, k) => {a[k] = true; return a;}, {});
}

/**
 * https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/is
 */
export function is(x: any, y: any) {
	// SameValue algorithm
	if (x === y) {
		// Steps 1-5, 7-10
		// Steps 6.b-6.e: +0 != -0
		// Added the nonzero y check to make Flow happy, but it is redundant
		return x !== 0 || y !== 0 || 1 / x === 1 / y;
	} else {
		// Step 6.a: NaN == NaN
		return x !== x && y !== y;
	}
}

// actual return type of _.flatten is RecursiveArray<T | Seq<T>>
export const flatten: <T>(seq: Seq<Seq<T>>) => Seq<T> = _.flatten;

export function last<T>(seq: Seq<T>): T {
	const length = seq.length;
	if (length === 0) throw new Error("Called last on an empty seq");
	return seq[length - 1];
}

export function lastOption<T>(seq: Seq<T>): Option<T> {
	const length = seq.length;
	if (length === 0) return None;
	return Some(seq[length - 1]);
}

export function firstOption<T>(seq: Seq<T>): Option<T> {
	if (seq.length === 0) return None;
	return Some(seq[0]);
}

export function mapWithPrevious<T, U>(array: Seq<T>, mapper: (curr: T, prev: T) => U): Seq<U> {
	const result: Array<U> = [];
	for (let i = 0; i < array.length; i++) {
		result.push(mapper(array[i], i === 0 ? undefined : array[i - 1]));
	}
	return result;
}
