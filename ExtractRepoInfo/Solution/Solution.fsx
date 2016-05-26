#load "../packages/FsLab/FsLab.fsx"
#load "../GitLogParser.fs"

open System
open System.IO

open FSharp.Data
open Git.LogParser
open XPlot.GoogleCharts

let filePath = Path.Combine(__SOURCE_DIRECTORY__, "..\Data\\nancy.log")

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
    commits
    |> Array.collect(fun c -> c.Files)
    |> Array.distinctBy(fun f -> f.FileName)
    |> Array.length

// 1.3 FILES BY TYPE
// Calculate the number of files that have the same extension (.cs, .js, etc).
// Time to use groupBy, map and sortByDescending
// The result should be: [|(".cs", 1963); (".dll", 401); (".xml", 143); ...|]

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
// The result should be: [|"Andreas H├Ñkansson"; "Kristian Hellang"; "Julien Roncaglia"; ... |]

let authors =
    commits
    |> Array.groupBy(fun c -> c.CommitInfo.Author)
    |> Array.map(fun (a, _) -> a)
    |> Array.distinct

// ------------
// 2. HOTSPOTS
// ------------ 

// 2.1 NUMBER OF REVISIONS BY FILE
// Calculate how many commits have been made to each file
// Something similar to files by type...


let numberOfRevisionsByFile =
    commits
    |> Array.collect(fun c -> c.Files)
    |> Array.groupBy(fun f -> f.FileName)
    |> Array.map ( fun c -> fst c, snd c |> Array.length)
    |> Array.sortByDescending snd

// 2.2 BARCHART OF NUMBER OF REVISIONS
// Take the first 10
// You can use the following helper function to get the file name from a full path for a better display
let getFileNameFromPath (p:string) = p.Split('/') |> Array.last

numberOfRevisionsByFile
|> Array.take 10
|> Array.map(fun c -> fst c |> getFileNameFromPath, snd c)
|> Chart.Bar
|> Chart.WithLabels ["Number of revisions"]

// 2.3 CALCULATE COMPLEXITY
// Calculate the number of lines of each file
// You can use Git.Client.numberOfLinesOf or develop your own version
// Use the 10 files with more revisions
let gitHubRawContentBaseAddress = "https://raw.githubusercontent.com/NancyFx/Nancy/"

let numberOfLinesByFile =
    numberOfRevisionsByFile
    |> Array.take 10
    |> Array.choose (fst >> Git.Client.numberOfLinesOf (sprintf "%smaster/" gitHubRawContentBaseAddress))
    |> Array.sortByDescending snd

// 2.4 BARCHART OF NUMBER OF LINES
numberOfLinesByFile
|> Array.map(fun c -> fst c |> getFileNameFromPath, snd c)
|> Chart.Bar
|> Chart.WithLabels ["Number of lines"]

// 2.5 CORRELATE NUMBER OF REVISIONS AND COMPLEXITY
// Draw both charts at the same time

[|numberOfRevisionsByFile
  |> Array.take 10
  |> Array.map(fun c -> fst c |> getFileNameFromPath, snd c); 
 numberOfLinesByFile |> Array.map(fun c -> fst c |> getFileNameFromPath, snd c)|]
|>Chart.Bar
|> Chart.WithLabels ["Number of revisions"; "Number of lines"]

// 2.6 NUMBER OF AUTHORS PER FILE
// Calculate the number of authors that have commited a given file
// Use the 10 files with more revisions

let numberOfAuthorsByFile =
    commits
    |> Array.map (fun c -> c.Files |> Array.map(fun f -> c.CommitInfo.Author, f.FileName ))
    |> Array.collect id
    |> Array.filter(fun f -> numberOfRevisionsByFile 
                             |> Array.take 10 
                             |> Array.map fst 
                             |> Array.contains (snd f))
    |> Array.groupBy snd
    |> Array.map(fun f -> fst f, snd f |> Array.distinct |> Array.length)
    |> Array.sortByDescending snd

// 2.7 BARCHAR OF NUMBER OF AUTHORS
numberOfAuthorsByFile
|> Array.map(fun c -> fst c |> getFileNameFromPath, snd c)
|> Chart.Bar
|> Chart.WithLabels ["Number of authors"]

// 2.8 CORRELATE NUMBER OF REVISIONS AND NUMBER OF AUTHORS
// Draw both charts at the same time
[|numberOfRevisionsByFile
  |> Array.take 10
  |> Array.map(fun c -> fst c |> getFileNameFromPath, snd c); 
 numberOfAuthorsByFile |> Array.map(fun c -> fst c |> getFileNameFromPath, snd c)|]
|> Chart.Bar
|> Chart.WithLabels ["Number of revisions"; "Number of authors"]

// -------------------------
// 3. COMPLEXITY OVER TIME
// -------------------------

// Steps:
//  1.- Create a function to get the history of a file
//      The output of the function should be a list (ordered by date) of tuples (or a type) composed by:
//          - the timestamp of the commit
//          - a string composed by the commit hash and the path of the file
//  2.- Create a function that calculates the number of lines, max number of tabs
//      and average number of tabs. (you can skip this step if you want)
//  3.- Create a function that for each element in the history gets the file from github and calculates the file
//      statistics (the previous function). You have three options here:
//          3.1.- write all the functions by yourself
//          3.2.- use Git.Client.getFileStatistics and pass the previous function as its first parameter
//          3.3.- use Git.Client.getFullFileStatistics to have all the job done. 
//  4.- Create a line chart to show the results

let getHistoryOf file =
    commits
    |> Array.filter(fun cf -> cf.Files |> Array.filter(fun f -> f.FileName.Contains file) |> Array.length > 0)
    |> Array.map(fun cf -> cf.CommitInfo.TimeStamp, cf.CommitInfo.Hash.Substring(1, cf.CommitInfo.Hash.Length - 2))


let getComplexityOf file =
    getHistoryOf file
    |> Array.map(fun f -> fst f, sprintf "%s%s/%s" gitHubRawContentBaseAddress (snd f) file)
    |> Array.choose Git.Client.getFullFileStatistics
    |> Array.sortBy(fst)

let complexity = 
    getComplexityOf "src/Nancy/NancyEngine.cs"

let first (x, _, _) = x
let second (_,x,_) = x
let third (_,_,x) = x

[|complexity |> Array.map(fun f -> fst f, float (first (snd f)));
  complexity |> Array.map(fun f -> fst f, (second (snd f)));
  complexity |> Array.map(fun f -> fst f, (third (snd f)))|]
    |> Chart.Line



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


let commitsByFile fileName =
    commits
    |> Array.collect ( fun c -> c.Files |> Array.map ( fun f -> c.CommitInfo.Author, f))
    |> Array.filter (fun f -> (snd f).FileName.Contains fileName)
    

let calculateFileContributionByAuthor ((authorAndComitedFileArray: 
                                       (string * CommittedFile) [])) =
    let sumLinesModified (committedFile : CommittedFile) =
        let getLines lines =
            match lines with
            | Some(x) -> x
            | None -> 0

        ( getLines committedFile.LinesAdded ) + (getLines committedFile.LinesDeleted)        

    let commitsGroupedByAuthor = authorAndComitedFileArray |> Array.groupBy fst

    commitsGroupedByAuthor 
        |> Array.map ( fun f -> fst f, (snd f) |> Array.sumBy ( sumLinesModified << snd ))  
        |> Array.sortByDescending snd
        

let contributionsByAuthorOn = commitsByFile >> calculateFileContributionByAuthor     


let fileName = "src/Nancy/NancyEngine.cs"

let data =
    (fileName, "", 0)::
    (contributionsByAuthorOn fileName
    |> Array.map(fun u -> fst u, fileName, snd u)
    |> List.ofArray)
    
let options =
    Options(
        minColor = "#f00",
        midColor = "#ddd",
        maxColor = "#0d0",
        headerHeight = 15,
        fontColor = "black",
        showScale = true        
    )
 
let treemap =
    data
    |> Chart.Treemap
    |> Chart.WithOptions options

    
// 3.2 FILES WHERE AN AUTHOR IS THE MAIN CONTRIBUTOR
// List the files where the author (xx) is the main contributor

let filesOf userName =
    commits
    |> Array.collect(fun c -> c.Files)
    |> Array.distinctBy(fun f -> f.FileName)
    |> Array.map(fun f -> contributionsByAuthorOn f.FileName, f.FileName)
    |> Array.filter(fun f -> fst (fst f |> Array.head)  = userName )
    |> Array.map snd


let filesOfStevenRobbins = filesOf "Steven Robbins"


// 3.3 FILES WHERE AN AUTHOR IS THE ONLY CONTRIBUTOR
// List the files where the author (xx) is the only contributor
let uniqueContributorOfFiles userName =
    commits
    |> Array.collect(fun c -> c.Files)
    |> Array.distinctBy(fun f -> f.FileName)
    |> Array.map(fun f -> contributionsByAuthorOn f.FileName, f.FileName)
    |> Array.filter(fun f -> (fst f |> Array.length = 1) && fst (fst f |> Array.head)  = userName )
    |> Array.map snd

let filesWhereStevenRobbinsIsTheOnlyContributor = uniqueContributorOfFiles "Steven Robbins"


//--------------
// 4. COUPLING
//--------------

// Steps:
//  1.- Create a function that, from an array of strings, generates all the possible pairs without repetition.
//      [|"A"; "B"; "C"|] -> [|("A", "B"); ("A", "C"); ("B", "C")|]
//  2.- Create a function that, for each commit, calls the previous function, and sum the times two files are commited together
//  3.- Use a Sankey chart to display the relationship between the first 20 files
let combinator (seq:string[]) = 
    [|
        for i in 0 .. seq.Length - 1 do
            for j in i+1 .. seq.Length - 1  do
                if i <> j then 
                    if seq.[i] < seq.[j] then
                        yield (seq.[i], seq.[j])
                    else yield (seq.[j], seq.[i])
    |]
type CommitedPair = {File1 : string; File2: string; TimesCommitedTogether: int}

let committedTogether =
    commits
    |> Array.map(fun c -> c.Files |> Array.map (fun f -> f.FileName) |> combinator)
    |> Array.collect id
    |> Array.groupBy id
    |> Array.sortByDescending ( snd >> Array.length )
    |> Array.map(fun (k, a) -> {File1 = fst k; 
                                File2 = snd k; 
                                TimesCommitedTogether = (a |> Array.length)})

committedTogether
|> Array.take 20
|> Array.map(fun c -> getFileNameFromPath c.File1, getFileNameFromPath c.File2, c.TimesCommitedTogether)
|> Chart.Sankey
|> Chart.WithLabels["File1"; "File2"; "Number of times committed together"]