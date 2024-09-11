import { Try } from "./Try";
import { None, Some } from "./Option";

function mapMultipleHelper<R>(args: Array<Try<any>>, callback: Function): Try<R> {
	if (args.find(_ => _.isFailure()) === undefined) {
		return Try.success(callback.apply(undefined, args.map(_ => _.get()) as any));
	}
	return Try.failure(args.map(curr => {
        if (curr.isSuccess()) {
            return None;
        }
        else {
            return Some(curr.getError());
        }
    }));
}

export function map2<T1, T2, R>(pt1: Try<T1>, pt2: Try<T2>, callback: (t1: T1, t2: T2) => R): Try<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}

export function map3<T1, T2, T3, R>(pt1: Try<T1>, pt2: Try<T2>, pt3: Try<T3>, callback: (t1: T1, t2: T2, t3: T3) => R): Try<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}

export function map4<T1, T2, T3, T4, R>(pt1: Try<T1>, pt2: Try<T2>, pt3: Try<T3>, pt4: Try<T4>, callback: (t1: T1, t2: T2, t3: T3, t4: T4) => R): Try<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}

export function map5<T1, T2, T3, T4, T5, R>(pt1: Try<T1>, pt2: Try<T2>, pt3: Try<T3>, pt4: Try<T4>, pt5: Try<T5>, callback: (t1: T1, t2: T2, t3: T3, t4: T4, t5: T5) => R): Try<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}

export function map6<T1, T2, T3, T4, T5, T6, R>(pt1: Try<T1>, pt2: Try<T2>, pt3: Try<T3>, pt4: Try<T4>, pt5: Try<T5>, pt6: Try<T6>, callback: (t1: T1, t2: T2, t3: T3, t4: T4, t5: T5, t6: T6) => R): Try<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}

export function map7<T1, T2, T3, T4, T5, T6, T7, R>(pt1: Try<T1>, pt2: Try<T2>, pt3: Try<T3>, pt4: Try<T4>, pt5: Try<T5>, pt6: Try<T6>, pt7: Try<T7>, callback: (t1: T1, t2: T2, t3: T3, t4: T4, t5: T5, t6: T6, t7: T7) => R): Try<R> {
	return mapMultipleHelper<R>(Array.prototype.slice.call(arguments, 0, arguments.length - 1), callback);
}
