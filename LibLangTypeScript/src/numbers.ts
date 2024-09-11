export function atMost(max: number): (n: number) => number {
	return (n: number) => Math.min(max, n);
}

export function atLeast(min: number): (n: number) => number {
	return (n: number) => Math.max(min, n);
}

export function inRange(min: number, max: number): (n: number) => number {
	return (n: number) => Math.max(min, Math.min(max, n));
}
