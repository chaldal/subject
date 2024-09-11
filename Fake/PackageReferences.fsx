#if !FAKE_BUILD_FSPROJ

#r "nuget: Fake.IO.FileSystem, 6.0.0"
#r "nuget: Fake.DotNet.Cli, 6.0.0"
#r "nuget: Fake.Core.Target, 6.0.0"
#r "nuget: Fake.JavaScript.Npm, 6.0.0"
#r "nuget: System.Security.Cryptography.Cng, 5.0.0"
#r "nuget: MSBuild.StructuredLogger, 2.1.820"
// fsi goes nuts and can't see naked System.Security.Cryptography.MD5 but has no problem with the wrapper:  https://github.com/dotnet/roslyn/issues/66249
#r "nuget: CryptHash.Net, 3.6.1"
#r "nuget: NuGet.Packaging, 6.9.1"
#r "nuget: Azure.Storage.Blobs, 12.20.0"

#endif