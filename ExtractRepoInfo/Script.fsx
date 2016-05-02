#load "./packages/FsLab/FsLab.fsx"
#load "GitLogParser.fs"

open System.IO

let filePath = Path.Combine(__SOURCE_DIRECTORY__, ".\Data\sfa-log.log")

let commits = Git.LogParser.getAllCommits filePath

// --------------------
// 1. BASIC STATISTICS
// --------------------

// 1.1 NUMBER OF FILES CHANGED
// Calculate the number of files (with repetitions) that have been commited
// Maybe it's time to investigate Array.collect and Array.length
// The result should be: 

let numberOfFilesChanged =


// 1.2 NUMBER OF FILES
// Calculate the number of files (without repetitions) that have been commited.
// Take a look at the Array documentation and see if there's some function to get the disctict elements.
// The result should be:

let numberOfFiles = 


// 1.3 FILES BY TYPE
// Calculate the number of files that have the same extension (.cs, .js, etc).
// Time to use groupBy, map and sortByDescending
// The result should be:

let filesByType =

// 1.4 CHART OF FILES BY TYPE
// Use XPlot to visualize the previously calculated data.
// Use a Chart.Pie that receives an array of tuples (string * value )

let chartFileByType =

// 1.6 AUTHORS
// Get the disctinct authors that have commited a file (using the Author field of the CommitInfo record)
// It's possible that you'll have to decompose a tuple: let d (a, b) = a
// The result should be:

let authors =

// ------------
// 2. HOTSPOTS
// ------------ 

// 2.1 NUMBER OF REVISIONS BY FILE
// Calculate how many commits have been made to each file
// Something similar to files by type...

let numberOfRevisionsByFile =

// 2.2 BARCHART OF NUMBER OF REVISIONS

// 2.3 CALCULATE COMPLEXITY
// Calculate the number of lines of each file
// You can use Git.Client.numberOfLinesOf or develop your own version
// Use the 10 files with more revisions

let numberOfLinesByFile =

// 2.4 CORRELATE NUMBER OF REVISIONS AND COMPLEXITY
// Draw both charts at the same time

// 2.5 NUMBER OF AUTHORS PER FILE
// Calculate the number of authors that have commited a given file
// Use the 10 files with more revisions

let numberOfAuthorsByFile =

// 2.6 CORRELATE NUMBER OF REVISIONS AND NUMBER OF AUTHORS
// Draw both charts at the same time

