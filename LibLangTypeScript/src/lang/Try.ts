import { reportError } from "../ErrorReporter";
import { None, Option, Some } from "./Option";


export class Try<T> {
	/* private */ constructor(
		private readonly isSuccessful: boolean,
		private readonly value:        T,
		private readonly error:        any = undefined
	) {
	}

	public static success<T>(value: T): Try<T> {
		return new Try<T>(true, value);
	}

	public static failure<T>(error: any): Try<T> {
		return new Try<T>(false, undefined, error);
	}

	public toOption(): Option<T> {
		if (this.isSuccess()) return Some(this.get());
		return None;
	}

	public isSuccess(): boolean {
		return this.isSuccessful;
	}

	public isFailure(): boolean {
		return !this.isSuccessful;
	}

	public get(): T {
		if (!this.isSuccessful) {
			throw new Error("Calling get on an unsuccessful Try");
		}
		return this.value;
	}

	public getError(): any {
		if (this.isSuccessful) {
			throw new Error("Calling getError on a successful Try");
		}
		return this.error;
	}

	public getOrElse(orValue: T): T {
		if (this.isSuccessful) return this.value;
		return orValue;
	}

	public map<V>(mapper: (value: T) => V): Try<V> {
		if (this.isSuccessful) {
			const result: V = mapper(this.value);
			return Try.success(result);
		}
		// reusing
		return <any>(this);
	}

	public match<V>(successMapper: (value: T) => V, errorMapper: (error: any) => V): V {
		if (this.isSuccessful) {
			return successMapper(this.value);
		}
		else {
			return errorMapper(this.error);
		}
	}

	public flatMap<V>(fn: (value: T) => Try<V>): Try<V> {
		if (this.isSuccessful) {
			return fn(this.value);
		}
		// reusing
		return <any>(this);
	}

	public sideEffect(fn: (value: T) => void): void {
		if (this.isSuccessful) {
			fn(this.value);
		}
	}

	public recover(recovery: (error: any) => T): Try<T> {
		try {
			if (this.isSuccessful) {
				return this;
			}
			else {
				return Try.success(recovery(this.error));
			}
		}
		catch(error) {
			return Try.failure<T>(error);
		}
	}

	public reportErrors(): Try<T> {
		if (!this.isSuccessful) reportError(this.error, {value: this.value});

		return this;
	}
}
