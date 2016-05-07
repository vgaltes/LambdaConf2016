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
        open System
        open System.IO
        open FSharp.Data
                
        let numberOfLinesOf gitHubRawContentBaseAddress file =
            let getFileFromGitHub fileUrl =
                let request = Http.Request(fileUrl, silentHttpErrors = true)
                if ( request.StatusCode = 200 ) then
                    match request.Body with
                    | Text(t) -> Some(t)
                    | Binary(_) -> None             
                else None

            let gitHubPath = Path.Combine ( gitHubRawContentBaseAddress, file)
            printfn "%s" gitHubPath
            let fileContent = getFileFromGitHub gitHubPath
            match fileContent with
            | Some(content) -> Some (file, content.Split([|'\n'|]) |> Array.length)
            | None -> None

        let calculateFileStatistics (fileContent: string) = 
            let srcLines = fileContent.Split([|"\n"|], StringSplitOptions.RemoveEmptyEntries)

            let numLines = srcLines |> Array.length

            let spaces = srcLines |> Array.map ( fun l -> l.ToCharArray() |> Array.takeWhile(fun c -> c = ' '))
            let numTabs = spaces |> Array.map ( fun l -> float( l |> Array.length ) / 4.0)
            let maxTabs = numTabs|> Array.max
            let averageTabs = numTabs |> Array.average

            numLines, maxTabs, averageTabs

        let processHttpResponse calculateFileStatistics (response: HttpResponseBody) =
            match response with
            | Text(t) -> 
                calculateFileStatistics t
            | Binary(_) -> 0, 0., 0.  
        
        let getFullFileStatistics (gitHubFileInfo: DateTime * string) =
            let request = Http.Request(snd gitHubFileInfo, silentHttpErrors = true)
            if ( request.StatusCode = 200 ) then
                Some (fst gitHubFileInfo, processHttpResponse calculateFileStatistics request.Body)   
            else None

        let getFileStatistics calculateFileStatistics (gitHubFileInfo: DateTime * string) =
            let request = Http.Request(snd gitHubFileInfo, silentHttpErrors = true)
            if ( request.StatusCode = 200 ) then
                Some (fst gitHubFileInfo, processHttpResponse calculateFileStatistics request.Body)   
            else None
