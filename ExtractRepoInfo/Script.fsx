﻿#load "./packages/FsLab/FsLab.fsx"
#load "GitLogParser.fs"

open System.IO

let filePath = Path.Combine(__SOURCE_DIRECTORY__, "Data", "nancy.log")

let commits = Git.LogParser.getAllCommits filePath

// type CommitInfo = {Hash : string; Author : string; TimeStamp : DateTime; Message : string}
// type CommittedFile = {LinesAdded: int option; LinesDeleted: int option; FileName: string}

// --------------------
// 1. BASIC STATISTICS
// --------------------

// 1.1 NUMBER OF FILES CHANGED
// Calculate the number of files (with repetitions) that have been commited
// Maybe it's time to investigate Array.collect and Array.length
// The result should be: 14720

let numberOfFilesChanged =
    commits
    |> Array.collect(fun c -> c.Files)
    |> Array.length


// 1.2 NUMBER OF FILES
// Calculate the number of files (without repetitions) that have been commited.
// Take a look at the Array documentation and see if there's some function to get the disctict elements.
// The result should be: 3284

let numberOfFiles = 


// 1.3 FILES BY TYPE
// Calculate the number of files that have the same extension (.cs, .js, etc).
// Time to use groupBy, map and sortByDescending
// The result should be: [|(".cs", 1963); (".dll", 401); (".xml", 143); ...|]

let filesByType =

// 1.4 CHART OF FILES BY TYPE
// Use XPlot to visualize the previously calculated data.
// Use a Chart.Pie that receives an array of tuples (string * value )

let chartFileByType =

// 1.6 AUTHORS
// Get the disctinct authors that have commited a file (using the Author field of the CommitInfo record)
// It's possible that you'll have to decompose a tuple: let d (a, b) = a
// The result should be: [|"Andreas H├Ñkansson"; "Kristian Hellang"; "Julien Roncaglia"; ... |]

let authors =

// ------------
// 2. HOTSPOTS
// ------------ 

// 2.1 NUMBER OF REVISIONS BY FILE
// Calculate how many commits have been made to each file
// Something similar to files by type...
// You can use the following helper function to get the file name from a full path
let getFileNameFromPath (p:string) = p.Split('/') |> Array.last

let numberOfRevisionsByFile =

// 2.2 BARCHART OF NUMBER OF REVISIONS
// Take the first 10

// 2.3 CALCULATE COMPLEXITY
// Calculate the number of lines of each file
// You can use Git.Client.numberOfLinesOf or develop your own version
// Use the 10 files with more revisions
let gitHubRawContentBaseAddress = "https://raw.githubusercontent.com/NancyFx/Nancy/"
let numberOfLinesByFile =

// 2.4 BARCHART OF NUMBER OF LINES

// 2.5 CORRELATE NUMBER OF REVISIONS AND COMPLEXITY
// Draw both charts at the same time

// 2.6 NUMBER OF AUTHORS PER FILE
// Calculate the number of authors that have commited a given file
// Use the 10 files with more revisions

let numberOfAuthorsByFile =

// 2.7 BARCHAR OF NUMBER OF AUTHORS

// 2.8 CORRELATE NUMBER OF REVISIONS AND NUMBER OF AUTHORS
// Draw both charts at the same time

// -------------------------
// 3. COMPLEXITY OVER TIME
// -------------------------

// Steps:
//  1.- Create a function to get the history of a file
//      The output of the function should be a list (ordered by date) of strings composed by
//      the commit hash and the path of the file
//  2.- Create a function that given a HttpResponseBody calculates the number of lines, max number of tabs
//      and average number of tabs
//  3.- Create a function that for each element in the history gets the file from github and calculates the file
//      statistics (the previous function)
//  4.- Create a line chart to show the results


//--------------
// 3. AUTHORS
//--------------

// 3.1 CONTRIBUTION BY AUTHOR IN A FILE
// Steps:
//  1.- Create a function to get the different commits (CommitInfo array) of a file. You'll need the author of each
//      Commit, so probably you'll need to return an array of (string * CommitInfo)
//  2.- Create a function that gets the result of the previous function and calculates the contributions (sum of lines
//      added plus lines deleted) of each author to that file
//  3.- Use a treeMap to display the information

// 3.2 FILES WHERE AN AUTHOR IS THE MAIN CONTRIBUTOR
// List the files where the author (xx) is the main contributor


// 3.3 FILES WHERE AN AUTHOR IS THE ONLU CONTRIBUTOR
// List the files where the author (xx) is the only contributor

//--------------
// 4. COUPLING
//--------------

// Steps:
//  1.- Create a function that, from an array of strings, generates all the possible pairs without repetition.
//      [|"A"; "B"; "C"|] -> [|("A", "B"); ("A", "C"); ("B", "C")|]
//  2.- Create a function that, for each commit, calls the previous function, and sum the times two files are commited together
//  3.- Use a Sankey chart to display the relationship between the first 20 files