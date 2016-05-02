module Git

    module LogParser =

        open FSharp.Data
        open System
        open System.Globalization
        open System.IO

        // git log --pretty=format:'[%H],%aN,%ad,%s' --date=local --numstat > sfa-log.log
    
        type CommitInfo = {Hash : string; Author : string; TimeStamp : DateTime; Message : string}
        type CommittedFile = {LinesAdded: int option; LinesDeleted: int option; FileName: string}
        type Commit = {CommitInfo: CommitInfo; Files: CommittedFile[]}   
        type CommitInfoCsv = CsvProvider<"Hash,Author,Date,Message", HasHeaders = false, 
                                            Schema = "Hash,Author,Date(string),Message">
        type CommitLineCsv = CsvProvider<"LinesAdded\tLinesDeleted\tFile", HasHeaders = false, 
                                            Schema = "LinesAdded(int option),LinesDeleted(int option),FileName">

        let getAllCommits filePath = 
            let lineBreak = "\r\n"

            let file = File.ReadAllText(filePath)
            let commits = file.Split([|lineBreak + lineBreak|], StringSplitOptions.RemoveEmptyEntries)

            let extractCommitInfo (commit:string) =
                let isCommitInfoLine (line: string) = line.StartsWith("[")

                let extractCommitedFilesInfo c = 
                    let commitFileLine = CommitLineCsv.Parse(c).Rows |> Seq.head

                    {LinesAdded = commitFileLine.LinesAdded
                     LinesDeleted = commitFileLine.LinesDeleted
                     FileName = commitFileLine.FileName}

                let commitLines = commit.Split([|lineBreak|], StringSplitOptions.RemoveEmptyEntries)    
                let commitInfoLine = commitLines |> Array.takeWhile isCommitInfoLine |> Array.last
                let fileLines = commitLines |> Array.skipWhile isCommitInfoLine 
                let infoRow = CommitInfoCsv.Parse(commitInfoLine).Rows |> Seq.head
                let commitInfo = {
                        Hash = infoRow.Hash
                        Author = infoRow.Author 
                        TimeStamp = DateTime.ParseExact(infoRow.Date,"ddd MMM d HH:mm:ss yyyy", CultureInfo.InvariantCulture) 
                        Message = infoRow.Message
                    }
        
                let fileRows = fileLines |> Array.map extractCommitedFilesInfo
    
                {CommitInfo = commitInfo; Files = fileRows}

            let totalCommits = 
                commits
                |> Array.map extractCommitInfo

            let extensionBlacklist = [|".css"; ".feature.cs"; ".generated.cs"; ".config"; ".scss"; ".csproj"; ".cshtml"; ".min.js"; ".sln"|]

            totalCommits
            |> Array.map(fun c -> {CommitInfo = c.CommitInfo; 
                                    Files = c.Files 
                                    |> Array.filter(fun f -> extensionBlacklist 
                                                            |> Array.forall(fun e -> not (f.FileName.EndsWith(e))))})

    module Client =
        open System.IO
        open FSharp.Data
                
        let numberOfLinesOf file =
            let gitHubRawContentBaseAddress = "https://raw.githubusercontent.com/SkillsFundingAgency/FindApprenticeship/master/"
            let getFileFromGitHub fileUrl =
                let request = Http.Request(fileUrl, silentHttpErrors = true)
                if ( request.StatusCode = 200 ) then
                    match request.Body with
                    | Text(t) -> Some(t)
                    | Binary(_) -> None             
                else None

            let gitHubPath = Path.Combine ( gitHubRawContentBaseAddress, file)
            let fileContent = getFileFromGitHub gitHubPath
            match fileContent with
            | Some(content) -> Some (file, content.Split([|'\n'|]) |> Array.length)
            | None -> None
