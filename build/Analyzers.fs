[<RequireQualifiedAccess>]
module Analyzers

open System
open System.IO
open System.IO.Compression
open System.Net.Http
open Fake.IO

let client = new HttpClient()

type NugetPackage = { Name: string; Version: string }

/// Downlaods nuget package fron nuget registry
let downloadPackage (package: NugetPackage) =
    let download = async {
        try
            let packageId = package.Name.ToLowerInvariant()
            let baseUrl = sprintf "https://api.nuget.org/v3-flatcontainer/%s/%s/%s.%s.nupkg"
            let packageUrl = baseUrl packageId package.Version packageId package.Version
            let! packageBytes = Async.AwaitTask (client.GetByteArrayAsync packageUrl)
            return Ok packageBytes
        with error ->
            return Error error
    }

    Async.RunSynchronously download

/// Downloads a list of nuget packages
let downloadPackages (packages: NugetPackage list) =
    let packageContents = ResizeArray<NugetPackage * byte[]>()
    for package in packages do
        printfn "Downloading package %s v%s" package.Name package.Version
        match downloadPackage package with
        | Ok content ->
            printfn "Downloaded package %s v%s successfully"  package.Name package.Version
            packageContents.Add(package, content)
        | Error error ->
            printfn "Error occured while downloading package %s v%s" package.Name package.Version
            printfn "%A" error
    packageContents

let private installUnsafe (destinationDir: string) (packages: NugetPackage list) =
    // list of supported target frameworks of F# analyzers
    let targetFrameworks = [ "netcoreapp2.0"; "netcoreapp2.1" ]
    if Directory.Exists destinationDir && not (Seq.isEmpty (Directory.GetDirectories(destinationDir))) then
        printfn "Cleaning up analyzers path"
        Shell.deleteDir destinationDir
    if not (Directory.Exists destinationDir) then ignore (Directory.CreateDirectory(destinationDir))
    let downloadedPackages = downloadPackages packages
    for (package, content) in downloadedPackages do
        let packagePath = Path.Combine(destinationDir, sprintf "%s.%s.nupkg" package.Name package.Version)
        let extractedPath = Path.Combine(destinationDir, sprintf "%s.%s" package.Name package.Version)
        let finalDestination = Path.Combine(destinationDir, package.Name)
        ignore (Directory.CreateDirectory(finalDestination))
        File.WriteAllBytes(packagePath, content)
        ZipFile.ExtractToDirectory(packagePath, extractedPath)
        let foundTargetFrameworks = Directory.GetDirectories(Path.Combine(extractedPath, "lib"))
        foundTargetFrameworks
        |> Seq.tryFind (fun framework -> targetFrameworks |> List.exists framework.EndsWith)
        |> function
            | None ->
                let availableFrameworks =
                    foundTargetFrameworks
                    |> Array.map (fun path -> path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
                    |> Array.map (fun segments -> Array.last segments)
                    |> String.concat ", "
                    |> sprintf "[%s]"

                printfn "Package %s.%s does not have a suitable target framework. Available target frameworks are %s" package.Name package.Version availableFrameworks
            | Some target ->
                let foundTargetFramework = Array.last (target.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
                printfn "Found compatible target framework %s for analyzer %s (v%s)" foundTargetFramework package.Name package.Version
                Shell.copyDir finalDestination target (fun file -> true)
                // Clean up
                Shell.deleteDir extractedPath
                File.Delete(packagePath)

/// Installs the configured analyzers into the specified directory
let install (destinationDir: string) (packages: NugetPackage list) =
    try
        installUnsafe destinationDir packages
    with error ->
        printfn "Error occured while installing analyzers in '%s'" destinationDir
        printfn "%A" error