module SuiteT__ECI__T.CodecGen

open CodecGen

let suiteInputs: SuiteInputs =
    {
        TypesProjectPath = __SOURCE_DIRECTORY__ +-+  @"../../../../Ecosystem/T__EC__T.Types"
        TypesProjectName = "T__EC__T.Types.fsproj"
        AbbreviateGenericParamWitness = false

        // types used as type parameters in generic subjects.
        // T__ECI__T has no generic subjects
        ShouldIncludeCodecTypeLabel = fun _ -> false

        CrossEcosystemTypeLabelPrefix = "T__EC__T_"
        TypesProjectSourceShouldCompile = fun _ -> true
    }

[<EntryPoint>]
let main _args =
    generateCodecs suiteInputs
    0
