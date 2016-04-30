#load "../packages/FsLab/FsLab.fsx"
#load "../GitLogParser.fs"

open System.IO
open XPlot.GoogleCharts

let filePath = Path.Combine(__SOURCE_DIRECTORY__, "..\Data\sfa-log.log")

let commits = GitLogParser.getAllCommits filePath

// --------------------
// 1. BASIC STATISTICS
// --------------------

// 1.1 NUMBER OF FILES CHANGED
// Calculate the number of files (with repetitions) that have been commited
// Maybe it's time to investigate Array.collect and Array.length
// The result should be: 

let numberOfFilesChanged =
    commits
    |> Array.collect(fun c -> c.Files)
    |> Array.length

// 1.2 NUMBER OF FILES
// Calculate the number of files (without repetitions) that have been commited.
// Take a look at the Array documentation and see if there's some function to get the disctict elements.
// The result should be:

let numberOfFiles = 
    commits
    |> Array.collect(fun c -> c.Files)
    |> Array.distinctBy(fun f -> f.FileName)
    |> Array.length

// 1.3 FILES BY TYPE
// Calculate the number of files that have the same extension (.cs, .js, etc).
// Time to use groupBy, map and sortByDescending
// The result should be:

let filesByType =
    commits
    |> Array.collect(fun c -> c.Files)
    |> Array.distinctBy(fun f -> f.FileName)
    |> Array.groupBy(fun f -> Path.GetExtension f.FileName)
    |> Array.map(fun f -> fst f, (snd f) |> Array.length)
    |> Array.sortByDescending snd

// 1.4 CHART OF FILES BY TYPE
// Use XPlot to visualize the previously calculated data.
// Use a Chart.Pie that receives an array of tuples (string * value )

let chartFileByType =
    filesByType
    |> Chart.Pie
    |> Chart.WithLegend true
    |> Chart.WithSize(640, 480)

// 1.6 AUTHORS
// Get the disctinct authors that have commited a file (using the Author field of the CommitInfo record)
// It's possible that you'll have to decompose a tuple: let d (a, b) = a
// The result should be:

let authors =
    commits
    |> Array.groupBy(fun c -> c.CommitInfo.Author)
    |> Array.map(fun (a, _) -> a)
    |> Array.distinct