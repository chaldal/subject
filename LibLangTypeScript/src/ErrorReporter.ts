export interface ErrorReporter {
	reportError(errorOrMessage: string | Error, extraData?: any): void
	reportWarning(errorOrMessage: string | Error, extraData?: any): void
}

let errorReporter: ErrorReporter;

export function provide(value: ErrorReporter): void {
	errorReporter = value;
}

export function reportError(errorOrMessage: string | Error, extraData: any = undefined): void {
	errorReporter.reportError(errorOrMessage, extraData);
}

export function reportWarning(errorOrMessage: string | Error, extraData: any = undefined): void {
	errorReporter.reportWarning(errorOrMessage, extraData);
}
