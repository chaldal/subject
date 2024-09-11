module AppRenderDslCompiler.Render.Main

open LibDsl.Compilers

open AppRenderDslCompiler.Render.Types
open AppRenderDslCompiler.Render.Compiler

let DefaultComponentLibraryAliases = Map.ofList [("default", "Components")]

let generateRenderFunctionFile
    (componentName:           string)
    (libAlias:                string)
    (additionalOpens:         seq<string>)
    (componentLibraryAliases: Map<string, string>)
    (componentAliases:        Map<string, string>)
    (withStyles:              bool)
    (source:                  string)
    : Result<string, RenderCompilerError> =

    let compiler = RenderDslCompiler {
        ComponentName           = componentName
        LibAlias                = libAlias
        AdditionalOpens         = additionalOpens
        ComponentLibraryAliases = componentLibraryAliases
        ComponentAliases        = componentAliases
        WithStyles              = withStyles
    }

    compiler.Compile source

let generateRenderFunction
    (componentName:           string)
    (libAlias:                string)
    (additionalOpens:         seq<string>)
    (componentLibraryAliases: Map<string, string>)
    (componentAliases:        Map<string, string>)
    (withStyles:              bool)
    (source:                  string)
    : Result<string, RenderCompilerError> =

    let compiler = RenderDslConverter {
        ComponentName           = componentName
        LibAlias                = libAlias
        AdditionalOpens         = additionalOpens
        ComponentLibraryAliases = componentLibraryAliases
        ComponentAliases        = componentAliases
        WithStyles              = withStyles
    }

    compiler.Compile source
