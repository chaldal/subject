export function validatePascalCase(input: string): true | string {
    return /^[A-Z][a-zA-Z0-9\.]*$/.test(input) || "PascalCase, please";
}
