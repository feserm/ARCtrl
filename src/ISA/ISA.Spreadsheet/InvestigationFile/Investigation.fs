namespace ISA.Spreadsheet

open ISA
open FsSpreadsheet
open Comment
open Remark
open System.Collections.Generic

module ArcInvestigation = 

    let identifierLabel = "Investigation Identifier"
    let titleLabel = "Investigation Title"
    let descriptionLabel = "Investigation Description"
    let submissionDateLabel = "Investigation Submission Date"
    let publicReleaseDateLabel = "Investigation Public Release Date"

    let investigationLabel = "INVESTIGATION"
    let ontologySourceReferenceLabel = "ONTOLOGY SOURCE REFERENCE"
    let publicationsLabel = "INVESTIGATION PUBLICATIONS"
    let contactsLabel = "INVESTIGATION CONTACTS"
    let studyLabel = "STUDY"

    let publicationsLabelPrefix = "Investigation Publication"
    let contactsLabelPrefix = "Investigation Person"

    
    type InvestigationInfo =
        {
        Identifier : string
        Title : string
        Description : string
        SubmissionDate : string
        PublicReleaseDate : string
        Comments : Comment list
        }

        static member create identifier title description submissionDate publicReleaseDate comments =
            {
            Identifier = identifier
            Title = title
            Description = description
            SubmissionDate = submissionDate
            PublicReleaseDate = publicReleaseDate
            Comments = comments
            }
  
        static member Labels = [identifierLabel;titleLabel;descriptionLabel;submissionDateLabel;publicReleaseDateLabel]
    
        static member FromSparseTable (matrix : SparseTable) =
        
            let i = 0

            let comments = 
                matrix.CommentKeys 
                |> List.map (fun k -> 
                    Comment.fromString k (matrix.TryGetValueDefault("",(k,i))))

            InvestigationInfo.create
                (matrix.TryGetValueDefault("",(identifierLabel,i)))  
                (matrix.TryGetValueDefault("",(titleLabel,i)))  
                (matrix.TryGetValueDefault("",(descriptionLabel,i)))  
                (matrix.TryGetValueDefault("",(submissionDateLabel,i)))  
                (matrix.TryGetValueDefault("",(publicReleaseDateLabel,i)))  
                comments


        static member ToSparseTable (investigation: ArcInvestigation) =
            let i = 1
            let matrix = SparseTable.Create (keys = InvestigationInfo.Labels,length=2)
            let mutable commentKeys = []

            do matrix.Matrix.Add ((identifierLabel,i),          (Option.defaultValue "" investigation.Identifier))
            do matrix.Matrix.Add ((titleLabel,i),               (Option.defaultValue "" investigation.Title))
            do matrix.Matrix.Add ((descriptionLabel,i),         (Option.defaultValue "" investigation.Description))
            do matrix.Matrix.Add ((submissionDateLabel,i),      (Option.defaultValue "" investigation.SubmissionDate))
            do matrix.Matrix.Add ((publicReleaseDateLabel,i),   (Option.defaultValue "" investigation.PublicReleaseDate))

            match investigation.Comments with 
            | None -> ()
            | Some c ->
                c
                |> List.iter (fun comment -> 
                    let n,v = comment |> Comment.toString
                    commentKeys <- n :: commentKeys
                    matrix.Matrix.Add((n,i),v)
                )   

            {matrix with CommentKeys = commentKeys |> List.distinct |> List.rev}

      
        static member fromRows lineNumber (rows : IEnumerator<SparseRow>) =
            SparseTable.FromRows(rows,InvestigationInfo.Labels,lineNumber)
            |> fun (s,ln,rs,sm) -> (s,ln,rs, InvestigationInfo.FromSparseTable sm)    
    
        static member toRows (investigation : ArcInvestigation) =  
            investigation
            |> InvestigationInfo.ToSparseTable
            |> SparseTable.ToRows
 
    let fromParts (investigationInfo:InvestigationInfo) (ontologySourceReference:OntologySourceReference list) publications contacts studies remarks =
        ArcInvestigation.make 
            None 
            None 
            (Option.fromValueWithDefault "" investigationInfo.Identifier)
            (Option.fromValueWithDefault "" investigationInfo.Title)
            (Option.fromValueWithDefault "" investigationInfo.Description) 
            (Option.fromValueWithDefault "" investigationInfo.SubmissionDate) 
            (Option.fromValueWithDefault "" investigationInfo.PublicReleaseDate)
            (Option.fromValueWithDefault [] ontologySourceReference) 
            (Option.fromValueWithDefault [] publications)  
            (Option.fromValueWithDefault [] contacts)  
            (Option.fromValueWithDefault [] studies)  
            (Option.fromValueWithDefault [] investigationInfo.Comments)  
            remarks


    let fromRows (rows:seq<SparseRow>) =
        let en = rows.GetEnumerator()              
        
        let emptyInvestigationInfo = InvestigationInfo.create "" "" "" "" "" []

        let rec loop lastLine ontologySourceReferences investigationInfo publications contacts studies remarks lineNumber =
            match lastLine with

            | Some k when k = ontologySourceReferenceLabel -> 
                let currentLine,lineNumber,newRemarks,ontologySourceReferences = OntologySourceReference.fromRows (lineNumber + 1) en         
                loop currentLine ontologySourceReferences investigationInfo publications contacts studies (List.append remarks newRemarks) lineNumber

            | Some k when k = investigationLabel -> 
                let currentLine,lineNumber,newRemarks,investigationInfo = InvestigationInfo.fromRows (lineNumber + 1) en       
                loop currentLine ontologySourceReferences investigationInfo publications contacts studies (List.append remarks newRemarks) lineNumber

            | Some k when k = publicationsLabel -> 
                let currentLine,lineNumber,newRemarks,publications = Publications.fromRows (Some publicationsLabelPrefix) (lineNumber + 1) en       
                loop currentLine ontologySourceReferences investigationInfo publications contacts studies (List.append remarks newRemarks) lineNumber

            | Some k when k = contactsLabel -> 
                let currentLine,lineNumber,newRemarks,contacts = Contacts.fromRows (Some contactsLabelPrefix) (lineNumber + 1) en       
                loop currentLine ontologySourceReferences investigationInfo publications contacts studies (List.append remarks newRemarks) lineNumber

            | Some k when k = studyLabel -> 
                let currentLine,lineNumber,newRemarks,study = Studies.fromRows (lineNumber + 1) en  
                if study.isEmpty then
                    loop currentLine ontologySourceReferences investigationInfo publications contacts studies (List.append remarks newRemarks) lineNumber
                else 
                    loop currentLine ontologySourceReferences investigationInfo publications contacts (study::studies) (List.append remarks newRemarks) lineNumber

            | k -> 
                fromParts investigationInfo ontologySourceReferences publications contacts (List.rev studies) remarks

        if en.MoveNext () then
            let currentLine = en.Current |> SparseRow.tryGetValueAt 0
            loop currentLine [] emptyInvestigationInfo [] [] [] [] 1
            
        else
            failwith "emptyInvestigationFile"
 
   
    let toRows (investigation:ArcInvestigation) : seq<SparseRow> =
        let insertRemarks (remarks:Remark list) (rows:seq<SparseRow>) = 
            try 
                let rm = remarks |> List.map Remark.toTuple |> Map.ofList            
                let rec loop i l nl =
                    match Map.tryFind i rm with
                    | Some remark ->
                         SparseRow.fromValues [wrapRemark remark] :: nl
                        |> loop (i+1) l
                    | None -> 
                        match l with
                        | [] -> nl
                        | h :: t -> 
                            loop (i+1) t (h::nl)
                loop 1 (rows |> List.ofSeq) []
                |> List.rev
            with | _ -> rows |> Seq.toList
        seq {
            yield  SparseRow.fromValues [ontologySourceReferenceLabel]
            yield! OntologySourceReference.toRows (Option.defaultValue [] investigation.OntologySourceReferences)

            yield  SparseRow.fromValues [investigationLabel]
            yield! InvestigationInfo.toRows investigation

            yield  SparseRow.fromValues [publicationsLabel]
            yield! Publications.toRows (Some publicationsLabelPrefix) (Option.defaultValue [] investigation.Publications)

            yield  SparseRow.fromValues [contactsLabel]
            yield! Contacts.toRows (Some contactsLabelPrefix) (Option.defaultValue [] investigation.Contacts)

            for study in (Option.defaultValue [ArcStudy.create()] investigation.Studies) do
                yield  SparseRow.fromValues [studyLabel]
                yield! Studies.toRows study
        }
        |> insertRemarks investigation.Remarks        
        |> seq

    let fromFsWorkbook (doc:FsWorkbook) =  
        try
            doc.GetWorksheets()
            |> List.head
            |> FsWorksheet.getRows
            |> Seq.map SparseRow.fromFsRow
            |> fromRows 
        with
        | err -> failwithf "Could not read investigation from spreadsheet: %s" err.Message

    let toFsWorkbook (investigation:ArcInvestigation) : FsWorkbook =           
        try
            let wb = new FsWorkbook()
            let sheet = FsWorksheet("Investigation")
            investigation
            |> toRows
            |> Seq.iteri (fun rowI r -> SparseRow.writeToSheet (rowI + 1) r sheet)                     
            wb.AddWorksheet(sheet)
            wb
        with
        | err -> failwithf "Could not write investigation to spreadsheet: %s" err.Message
