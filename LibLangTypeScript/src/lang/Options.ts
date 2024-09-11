import { None, Option, Some } from "./Option";

export function flatten<T>(o: Option<Option<T>>): Option<T> {
	return o.flatMap(_ => _.orElse(None));
}

function mapMultipleHelper<R>(args: Array<Option<any>>, callback: Function): Option<R> {
	if (args.find(_ => _.isEmpty()) === undefined) {
		return Some(callback.apply(undefined, args.map(_ => _.get()) as any));
	}
	return None;
}

export function map2<T1, T2, R>(pt1: Option<T1>, pt2: Option<T2>, callback: (t1: T1, t2: T2) => R): Option<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}

export function map3<T1, T2, T3, R>(pt1: Option<T1>, pt2: Option<T2>, pt3: Option<T3>, callback: (t1: T1, t2: T2, t3: T3) => R): Option<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}

export function map4<T1, T2, T3, T4, R>(pt1: Option<T1>, pt2: Option<T2>, pt3: Option<T3>, pt4: Option<T4>, callback: (t1: T1, t2: T2, t3: T3, t4: T4) => R): Option<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}

export function map5<T1, T2, T3, T4, T5, R>(pt1: Option<T1>, pt2: Option<T2>, pt3: Option<T3>, pt4: Option<T4>, pt5: Option<T5>, callback: (t1: T1, t2: T2, t3: T3, t4: T4, t5: T5) => R): Option<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}

export function map6<T1, T2, T3, T4, T5, T6, R>(pt1: Option<T1>, pt2: Option<T2>, pt3: Option<T3>, pt4: Option<T4>, pt5: Option<T5>, pt6: Option<T6>, callback: (t1: T1, t2: T2, t3: T3, t4: T4, t5: T5, t6: T6) => R): Option<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}

export function map7<T1, T2, T3, T4, T5, T6, T7, R>(pt1: Option<T1>, pt2: Option<T2>, pt3: Option<T3>, pt4: Option<T4>, pt5: Option<T5>, pt6: Option<T6>, pt7: Option<T7>, callback: (t1: T1, t2: T2, t3: T3, t4: T4, t5: T5, t6: T6, t7: T7) => R): Option<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}
