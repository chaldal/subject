export class Option<T> {
	constructor(private _isDefined: boolean = false, private value: T = undefined) {
	}

	public static isNully(possiblyUndefinedOrNullValue: any): boolean {
		return (possiblyUndefinedOrNullValue === undefined || possiblyUndefinedOrNullValue === null);
	}

	public static fromNullable<T>(possiblyUndefinedOrNullValue: T): Option<T> {
		if (Option.isNully(possiblyUndefinedOrNullValue)) {
			return None;
		}
		return Option.some<T>(possiblyUndefinedOrNullValue);
	}

	public static some<T>(value: T): Option<T> {
		return new Option<T>(true, value);
	}

	public toString(): string {
		if (this._isDefined) {
			return `Some(${this.value})`;
		}
		else {
			return "None";
		}
	}

	public isDefined(): boolean {
		return this._isDefined;
	}

	public isEmpty(): boolean {
		return !this._isDefined;
	}

	public get(): T {
		if (!this._isDefined) {
			throw new Error("Option is absent");
		}
		return this.value;
	}

	public orElse(orOption: Option<T>): Option<T> {
		if (this._isDefined) return this;
		return orOption;
	}

	public orElseLazy(orOptionGenerator: () => Option<T>): Option<T> {
		if (this._isDefined) return this;
		return orOptionGenerator();
	}

	public getOrElse(orValue: T): T {
		if (this._isDefined) return this.value;
		return orValue;
	}

	public getOrElseLazy(orGenerator: () => T): T {
		if (this._isDefined) return this.value;
		return orGenerator();
	}

	public orGet<U extends T>(orOption: Option<U>): T {
		if (this._isDefined) return this.value;
		return orOption.get();
	}

	public map<V>(mapper: (value: T) => V): Option<V> {
		if (this._isDefined) {
			return Option.some(mapper(this.value));
		}
		return None;
	}

	public flatMap<V>(fn: (value: T) => Option<V>): Option<V> {
		if (this._isDefined) {
			return fn(this.value);
		}
		return None;
	}

	public match<V>(someMapper: (value: T) => V, noneMapper: () => V): V {
		if (this._isDefined) {
			return someMapper(this.value);
		}
		else {
			return noneMapper();
		}
	}

	public filter(predicate: (value: T) => boolean): Option<T> {
		if (!this._isDefined || predicate(this.value)) {
			return this;
		}
		return None;
	}

	public filterNot(predicate: (value: T) => boolean): Option<T> {
		if (this._isDefined && !predicate(this.value)) {
			return this;
		}

		return None;
	}

	public sideEffect(fn: (value: T) => void): void {
		if (this._isDefined) {
			fn(this.value);
		}
	}
}

export const None = new Option<never>();

export function Some<V>(v: V): Option<V> {
	return Option.some(v);
}
