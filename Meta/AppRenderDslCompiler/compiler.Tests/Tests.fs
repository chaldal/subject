module AppRenderDslCompiler.Tests

open Xunit

open AppRenderDslCompiler.Render.CodeGeneration
open AppRenderDslCompiler.Render.Types
open AppRenderDslCompiler.Render.Main
open System.IO
open System.Reflection

let private testGenerateForExpression (expression: string) (expectedResult: string) =
    Assert.Equal(expectedResult, (generateForExpression (SingleLineExpression (NonemptyString.ofStringUnsafe expression))).Value)

let private testGenerateForStringExpression (expression: string) (expectedResult: string) =
    Assert.Equal(expectedResult, (generateForStringExpression (StringExpression (NonemptyString.ofStringUnsafe expression))).Value)

[<Fact>]
let ``Record literal`` () =
    testGenerateForExpression """{name = "anton"}""" """({name = "anton"})"""

[<Fact>]
let ``Newlines encoded`` () =
    testGenerateForStringExpression "This is\nsome text" "\"This is\\nsome text\""

type FileBasedTest = {
    name:           string
    input:          string
    expectedOutput: string
} with
    override this.ToString () : string =
        this.name

let private readTests () : seq<FileBasedTest> =
    // likely not to work cross-platform, and maybe only works with debug builds, but good enough for now
    let assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    let testsDirectory = (Directory.GetParent assemblyDirectory).Parent.Parent.FullName + "/tests"

    System.IO.Directory.GetFiles(testsDirectory, "*.render")
    |> Seq.ofArray
    |> Seq.map
        (fun rtFilename ->
            let fsFilename = rtFilename.[0..rtFilename.Length - 7] + "xfs"
            let rtFileContents = System.IO.File.ReadAllText rtFilename
            let fsFileContents = System.IO.File.ReadAllText fsFilename
            let testName = System.IO.Path.GetFileNameWithoutExtension rtFilename
            {
                name           = testName
                input          = rtFileContents
                expectedOutput = fsFileContents
            }
        )

let values = readTests() |> Seq.map (fun (z) -> Array.ofList [z :> obj])

[<Theory>]
[<MemberData("values")>]
let FileBasedTests (test: FileBasedTest) : unit =
    let actualOutput =
        match generateRenderFunctionFile "Test" "LibAlias" [] DefaultComponentLibraryAliases Map.empty true test.input with
        | Error e -> e.ToString()
        | Ok code -> code

    Assert.Equal(test.expectedOutput, actualOutput)
