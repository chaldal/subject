export function ifMatch<T>(condition: boolean, trueCase: () => T, falseCase: () => T): T {
	return condition
	     ? trueCase()
	     : falseCase();
}
