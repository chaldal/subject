export type T2<V1, V2> = Readonly<{__type: 'T2', _1: V1, _2: V2, array: [V1, V2]}>;
export function T2<V1, V2>(_1: V1, _2: V2): T2<V1, V2> {
	return { __type: 'T2', _1, _2, array: <[V1, V2]>[_1, _2] };
}
export function isT2(value: any): value is T2<any, any> {
	return value !== undefined && value.__type === 'T2';
}

export type T3<V1, V2, V3> = Readonly<{__type: 'T3', _1: V1, _2: V2, _3: V3, array: [V1, V2, V3]}>;
export function T3<V1, V2, V3>(_1: V1, _2: V2, _3: V3): T3<V1, V2, V3> {
	return { __type: 'T3', _1, _2, _3, array: <[V1, V2, V3]>[_1, _2, _3] };
}
export function isT3(value: any): value is T3<any, any, any> {
	return value !== undefined && value.__type === 'T3';
}

export type T4<V1, V2, V3, V4> = Readonly<{__type: 'T4', _1: V1, _2: V2, _3: V3, _4: V4, array: [V1, V2, V3, V4]}>;
export function T4<V1, V2, V3, V4>(_1: V1, _2: V2, _3: V3, _4: V4): T4<V1, V2, V3, V4> {
	return { __type: 'T4', _1, _2, _3, _4, array: <[V1, V2, V3, V4]>[_1, _2, _3, _4] };
}
export function isT4(value: any): value is T4<any, any, any, any> {
	return value !== undefined && value.__type === 'T4';
}
