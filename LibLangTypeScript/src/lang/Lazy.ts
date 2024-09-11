export interface InternalConstructorParams<T> {
	value:       T;
	isGenerated: boolean;
}

export class Lazy<T> {
	private generator:   () => T;
	private value:       T;
	private isGenerated: boolean = false;

	constructor (generator: () => T, internal?: InternalConstructorParams<T>) {
		if (internal) {
			this.value       = internal.value;
			this.isGenerated = internal.isGenerated;
		}
		else {
			this.generator = generator;
		}
	}

	public static of<T>(t: T): Lazy<T> {
		return new Lazy<T>(undefined, {
			value:       t,
			isGenerated: true,
		});
	}

	public get(): T {
		if (!this.isGenerated) {
			this.value = this.generator();
			this.isGenerated = true;
		}
		return this.value;
	}

	public map<U>(mapper: (t: T) => U): Lazy<U> {
		return new Lazy(() => mapper(this.get()));
	}
}
