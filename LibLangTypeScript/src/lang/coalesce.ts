import { None, Option, Some } from "./Option";

export function coalesce<T>(...args: Array<T>): T {
    const len = args.length;
    for (let i = 0; i < len; i++) {
        if (args[i] !== null && args[i] !== undefined) {
            return args[i];
        }
    }

    return undefined;
}

export function coalesceOption<T>(...args: Array<T>): Option<T> {
    const len = args.length;
    for (let i = 0; i < len; i++) {
        if (args[i] !== null && args[i] !== undefined) {
            return Some(args[i]);
        }
    }

    return None;
}
