import { Seq } from "./collections";
import { None, Option, Some } from "./lang";

export function min(s1: string, s2: string): string {
	if (s1 < s2) return s1; else return s2;
}

export function extremes(strings: Seq<string>): Option<{min: string, max: string}> {
	if (strings.length === 0) return None;

	let min = strings[0];
	let max = strings[0];

	for (let i = 1; i < strings.length; i++) {
		const curr = strings[i];
		if (curr < min) min = curr;
		if (curr > max) max = curr;
	}

	return Some({
		min: min,
		max: max,
	});
}

export function toNonEmptyOption(s: string | undefined | null): Option<string> {
	if (!s) return None;
	return Some(s);
}

export function comparer(s1: string, s2: string): number {
	if (s1 < s2) return -1;
	else if (s1 > s2) return 1;
	else return 0;
}
