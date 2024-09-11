import * as _ from "lodash";

/**
 * Wherever Immutable.Record feels too heavy-weight,
 * but we want to keep the benefits of immutability,
 * use a normal object but update it using this function
 */
export function update<T>(t: T, u: Partial<T>): T {
	return _.extend({}, t, u);
}

export function updatePicked<T, K extends keyof T, L extends keyof T>(t: Pick<T, K>, u: Pick<T, L>): Pick<T, K & L> {
	return _.extend({}, t, u);
}

/**
 * When you have a generic class, and need to perform an update
 * at that level, without knowing what the actual leaf type is,
 * typescript won't allow you to, with an unhelful error message,
 * but basically it boils down to "you can't have a Partial of a type
 * that is not concrete". Instead, we perform the update using the base
 * type, which is probably what you wanted to do in the first place.
 */
export function updateBase<T extends B, B>(t: T, u: Partial<B>): T {
	return _.extend({}, t, u);
}

export function updateSub<Superclass, Subclass extends Superclass>(superclass: Superclass, subclassPartial: Partial<Subclass>): Partial<Subclass> {
	return _.extend({}, superclass, subclassPartial);
}
