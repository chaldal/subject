module LibLifeCycleTypes.AssemblyInfo

open System.Runtime.CompilerServices

[<assembly: InternalsVisibleTo("LibLifeCycle")>]

// Using globally-namespaced modules (such as "module LibLifeCycleTypes_SubjectTypes") results in Orleans generating faulty code.
// See https://github.com/dotnet/orleans/issues/8213. Therefore, we use non-global modules and auto-open at the assembly level
// where relevant. However, the use of assembly-level AutoOpen triggers a compilation bug in Fable when using the --noPrecompile
// switch.
//
// So we need to avoid globally-namespaced modules for Orleans' sake, and avoid assembly-level AutoOpen for Fable's sake.
//
// TODO: HACK: once Fable allows auto-open at assembly level in conjunction with the --noPrecompile switch, we can undo this mess.

#if !FABLE_COMPILER
[<assembly: AutoOpen("LibLifeCycleTypes.AccessControl")>]
[<assembly: AutoOpen("LibLifeCycleTypes.EcosystemTypes")>]
[<assembly: AutoOpen("LibLifeCycleTypes.SubjectTypes")>]
[<assembly: AutoOpen("LibLifeCycleTypes.Exceptions")>]
#endif

do()
