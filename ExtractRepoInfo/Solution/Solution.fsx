#load "../packages/FsLab/FsLab.fsx"
#load "../GitLogParser.fs"

open System.IO

let filePath = Path.Combine(__SOURCE_DIRECTORY__, "..\Data\sfa-log.log")

let commits = GitLogParser.getAllCommits filePath


