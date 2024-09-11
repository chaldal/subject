module AppRenderDslCompiler.RecordsWithDefaults.Main

open AppRenderDslCompiler.RecordsWithDefaults.Compiler

let generateRecordConstructors (componentName: string) (componentsAlias: string) (namespaceToAlias: Map<string, string>) (source: string): Result<string, RecordsWithDefaultsCompilerError> =
    let compiler = RecordsWithDefaultsCompiler {
        ComponentName    = componentName
        ComponentsAlias  = componentsAlias
        NamespaceToAlias = namespaceToAlias
    }

    compiler.Compile source
