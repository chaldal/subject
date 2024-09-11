import { None, Option } from "./Option";

export class Either<A, B> {
	constructor(private _isLeft: boolean, private _left: A, private _right: B) {
	}

	public isLeft(): boolean {
		return this._isLeft;
	}

	public isRight(): boolean {
		return !this._isLeft;
	}

	public left(): Option<A> {
		if (this._isLeft) return Option.some(this._left);
		return None;
	}

	public right(): Option<B> {
		if (!this._isLeft) return Option.some(this._right);
		return None;
	}
}

export function Left<A, B>(left: A): Either<A, B> {
	return new Either<A, B>(true, left, undefined);
}

export function Right<A, B>(right: B): Either<A, B> {
	return new Either<A, B>(false, undefined, right);
}

export interface WireEither<A, B> {
	left?:  A;
	right?: B;
}

export function eitherFromWire<A, B>(wireEither: WireEither<A, B>): Either<A, B> {
	if (wireEither.left !== undefined) {
		return Left<A, B>(wireEither.left);
	}
	else {
		return Right<A, B>(wireEither.right);
	}
}
