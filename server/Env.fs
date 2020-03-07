module Env

let isDevelopment = 
    #if DEBUG 
    true
    #else 
    false 
    #endif 

open System.IO
open System.Linq

/// Recursively tries to find the parent of a file starting from a directory
let rec findParent (directory: string) (fileToFind: string) = 
    let path = if Directory.Exists(directory) then directory else Directory.GetParent(directory).FullName
    let files = Directory.GetFiles(path)
    if files.Any(fun file -> Path.GetFileName(file).ToLower() = fileToFind.ToLower()) 
    then path 
    else findParent (DirectoryInfo(path).Parent.FullName) fileToFind

let solutionRoot = findParent __SOURCE_DIRECTORY__ "root.txt"