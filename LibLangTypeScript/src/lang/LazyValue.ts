import { Option, Some } from "./Option";

function defaultIsEqual<T>(a: T, b: T): boolean {
	return a === b;
}

export class LazyValue<Input, Output> {
	private input: Input;
	private maybeValue: Option<Output>;

	constructor(
		private factory: (input: Input) => Output,
		private isEqual: (a: Input, b: Input) => boolean = defaultIsEqual
	) {}

	public get(input: Input): Output {
		if (!this.isEqual(this.input, input) || this.maybeValue.isEmpty()) {
			this.input = input;
			this.maybeValue = Some(this.factory(input));
		}

		return this.maybeValue.get();
	}
}
