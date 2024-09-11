module Suitejobs.CodecGen

open CodecGen

let suiteInputs: SuiteInputs =
    {
        TypesProjectPath = __SOURCE_DIRECTORY__ +-+  @"../../../../Ecosystem/Jobs.Types"
        TypesProjectName = "Jobs.Types.fsproj"
        AbbreviateGenericParamWitness = false

        // types used as type parameters in generic subjects.
        // jobs has no generic subjects
        ShouldIncludeCodecTypeLabel = fun _ -> false

        CrossEcosystemTypeLabelPrefix = "Jobs_"
        TypesProjectSourceShouldCompile = fun _ -> true
    }

[<EntryPoint>]
let main _args =
    generateCodecs suiteInputs
    0
