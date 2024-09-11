module Egg.Shell.Fake.FileSystemHelper

open System.IO

/// Create a symlink that is either a directory or file depending on the type of `path` provided. In either case,
/// it will link to `pathToTarget`.
let CreateSymbolicLink (path: string) (pathToTarget: string) =
    // We need to know whether the target is a file or directory, so the correct type of symlink is created
    // at the location specified by path.
    let (pathFileSystemInfo: FileSystemInfo, targetFileSystemInfo: FileSystemInfo) =
        let fullPathToTarget =
            if Path.IsPathFullyQualified(pathToTarget) then
                pathToTarget
            else
                Path.Combine(path, pathToTarget)

        if Directory.Exists(fullPathToTarget) then
            (DirectoryInfo(path) :> FileSystemInfo, DirectoryInfo(fullPathToTarget) :> FileSystemInfo)
        else if File.Exists(fullPathToTarget) then
            (FileInfo(path) :> FileSystemInfo, FileInfo(fullPathToTarget) :> FileSystemInfo)
        else
            failwith $"pathToTarget {pathToTarget} has full path of {fullPathToTarget}, which does not exist"

    if pathFileSystemInfo.Exists then
        let isSymlink = pathFileSystemInfo.Attributes &&& FileAttributes.ReparsePoint = FileAttributes.ReparsePoint

        if isSymlink then
            // Recreate the symlink just in case the target differs from what is in the existing symlink.
            pathFileSystemInfo.Delete() |> ignore
        else
            failwith $"path already exists and it is not a symlink: {path}"

    // Always create the link with a relative path to the target, even if we're given a fully qualified path.
    let relativePathToTarget = System.IO.Path.GetRelativePath(System.IO.Path.GetDirectoryName(pathFileSystemInfo.FullName), targetFileSystemInfo.FullName)

    pathFileSystemInfo.CreateAsSymbolicLink(relativePathToTarget)

let Delete (path: string) =
    try
        let attr = File.GetAttributes (path)

        if (attr.HasFlag(FileAttributes.Directory))
        then
            System.IO.Directory.Delete (path) |> ignore
        else
            System.IO.File.Delete (path) |> ignore
    with
    | :? System.IO.FileNotFoundException
    | :? System.IO.DirectoryNotFoundException -> sprintf "Delete: File/Directory does not exist" |> ignore
    | e -> raise e