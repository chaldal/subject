using Orleans.CodeGeneration;

// We need a C# project so we can include the Microsoft.Orleans.CodeGenerator.MSBuild nuget package
// and link the LibLifeCycleCore project, which will make Orleans generate necessary stubs for grains
// and types at compile time instead of runtime, considerably reducing initialization time of Dev Host
// and unit tests. Unfortunately a C# project is needed, as the MSBuild CodeGen uses Rosyln, which only
// supports C#.

// Need a separate project from LibLifeCycleHostBuild.csproj
// to expose client interface only and avoid dependency on AspNetCore

// Make the Orleans CodeGen follow into these projects
[assembly: KnownAssembly(typeof(LibLifeCycleCore.Anchor.AnchorTypeForProject))]
[assembly: KnownAssembly(typeof(LibLifeCycleTypes.SubjectTypes.SubjectReference))]

namespace LibLifeCycleCoreBuild
{
    public static class Build
    {
        // NO-OP, just for passing around assembly reference
    }
}