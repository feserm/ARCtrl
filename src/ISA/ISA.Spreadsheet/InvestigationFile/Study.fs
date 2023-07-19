namespace ISA.Spreadsheet

open ISA
open Comment
open Remark
open System.Collections.Generic

module Studies = 

    let [<Literal>] identifierLabel = "Study Identifier"
    let [<Literal>] titleLabel = "Study Title"
    let [<Literal>] descriptionLabel = "Study Description"
    let [<Literal>] submissionDateLabel = "Study Submission Date"
    let [<Literal>] publicReleaseDateLabel = "Study Public Release Date"
    let [<Literal>] fileNameLabel = "Study File Name"

    let [<Literal>] designDescriptorsLabelPrefix = "Study Design"
    let [<Literal>] publicationsLabelPrefix = "Study Publication"
    let [<Literal>] factorsLabelPrefix = "Study Factor"
    let [<Literal>] assaysLabelPrefix = "Study Assay"
    let [<Literal>] protocolsLabelPrefix = "Study Protocol"
    let [<Literal>] contactsLabelPrefix = "Study Person"

    let [<Literal>] designDescriptorsLabel = "STUDY DESIGN DESCRIPTORS"
    let [<Literal>] publicationsLabel = "STUDY PUBLICATIONS"
    let [<Literal>] factorsLabel = "STUDY FACTORS"
    let [<Literal>] assaysLabel = "STUDY ASSAYS"
    let [<Literal>] protocolsLabel = "STUDY PROTOCOLS"
    let [<Literal>] contactsLabel = "STUDY CONTACTS"

    type StudyInfo =
        {
        Identifier : string
        Title : string
        Description : string
        SubmissionDate : string
        PublicReleaseDate : string
        FileName : string
        Comments : Comment list
        }

        static member create identifier title description submissionDate publicReleaseDate fileName comments =
            {
            Identifier = identifier
            Title = title
            Description = description
            SubmissionDate = submissionDate
            PublicReleaseDate = publicReleaseDate
            FileName = fileName
            Comments = comments
            }
  
        static member Labels = [identifierLabel;titleLabel;descriptionLabel;submissionDateLabel;publicReleaseDateLabel;fileNameLabel]
    
        static member FromSparseTable (matrix : SparseTable) =
        
            let i = 0

            let comments = 
                matrix.CommentKeys 
                |> List.map (fun k -> 
                    Comment.fromString k (matrix.TryGetValueDefault("",(k,i))))

            StudyInfo.create
                (matrix.TryGetValueDefault("",(identifierLabel,i)))  
                (matrix.TryGetValueDefault("",(titleLabel,i)))  
                (matrix.TryGetValueDefault("",(descriptionLabel,i)))  
                (matrix.TryGetValueDefault("",(submissionDateLabel,i)))  
                (matrix.TryGetValueDefault("",(publicReleaseDateLabel,i)))  
                (matrix.TryGetValueDefault("",(fileNameLabel,i)))                    
                comments


        static member ToSparseTable (study: ArcStudy) =
            let i = 1
            let matrix = SparseTable.Create (keys = StudyInfo.Labels,length = 2)
            let mutable commentKeys = []

            do matrix.Matrix.Add ((identifierLabel,i),          study.Identifier)
            do matrix.Matrix.Add ((titleLabel,i),               (Option.defaultValue "" study.Title))
            do matrix.Matrix.Add ((descriptionLabel,i),         (Option.defaultValue "" study.Description))
            do matrix.Matrix.Add ((submissionDateLabel,i),      (Option.defaultValue "" study.SubmissionDate))
            do matrix.Matrix.Add ((publicReleaseDateLabel,i),   (Option.defaultValue "" study.PublicReleaseDate))
            do matrix.Matrix.Add ((fileNameLabel,i),            ArcStudy.FileName)

            if study.Comments.IsEmpty |> not then
                study.Comments
                |> List.iter (fun comment -> 
                    let n,v = comment |> Comment.toString
                    commentKeys <- n :: commentKeys
                    matrix.Matrix.Add((n,i),v)
                )    

            {matrix with CommentKeys = commentKeys |> List.distinct |> List.rev}

        static member fromRows lineNumber (rows : IEnumerator<SparseRow>) =
            SparseTable.FromRows(rows,StudyInfo.Labels,lineNumber)
            |> fun (s,ln,rs,sm) -> (s,ln,rs, StudyInfo.FromSparseTable sm)
    
        static member toRows (study : ArcStudy) =  
            study
            |> StudyInfo.ToSparseTable
            |> SparseTable.ToRows
    
    let fromParts (studyInfo:StudyInfo) (designDescriptors:OntologyAnnotation list) publications factors (assays: ArcAssay list) (protocols : Protocol list) contacts =
        ArcStudy.make 
            (studyInfo.Identifier)
            (Option.fromValueWithDefault "" studyInfo.Title)
            (Option.fromValueWithDefault "" studyInfo.Description) 
            (Option.fromValueWithDefault "" studyInfo.SubmissionDate)
            (Option.fromValueWithDefault "" studyInfo.PublicReleaseDate)
            (publications)
            (contacts)
            (designDescriptors)  
            (protocols |> List.map ArcTable.fromProtocol |> ResizeArray)
            (ResizeArray(assays))
            (factors) 
            (studyInfo.Comments)
        |> fun arcstudy -> if arcstudy.isEmpty && arcstudy.Identifier = "" then None else Some arcstudy

    let fromRows lineNumber (en:IEnumerator<SparseRow>) = 

        let rec loop lastLine (studyInfo : StudyInfo) designDescriptors publications factors assays protocols contacts remarks lineNumber =
           
            match lastLine with

            | Some k when k = designDescriptorsLabel -> 
                let currentLine,lineNumber,newRemarks,designDescriptors = DesignDescriptors.fromRows (Some designDescriptorsLabelPrefix) (lineNumber + 1) en         
                loop currentLine studyInfo designDescriptors publications factors assays protocols contacts (List.append remarks newRemarks) lineNumber

            | Some k when k = publicationsLabel -> 
                let currentLine,lineNumber,newRemarks,publications = Publications.fromRows (Some publicationsLabelPrefix) (lineNumber + 1) en       
                loop currentLine studyInfo designDescriptors publications factors assays protocols contacts (List.append remarks newRemarks) lineNumber

            | Some k when k = factorsLabel -> 
                let currentLine,lineNumber,newRemarks,factors = Factors.fromRows (Some factorsLabelPrefix) (lineNumber + 1) en       
                loop currentLine studyInfo designDescriptors publications factors assays protocols contacts (List.append remarks newRemarks) lineNumber

            | Some k when k = assaysLabel -> 
                let currentLine,lineNumber,newRemarks,assays = Assays.fromRows (Some assaysLabelPrefix) (lineNumber + 1) en       
                loop currentLine studyInfo designDescriptors publications factors assays protocols contacts (List.append remarks newRemarks) lineNumber

            | Some k when k = protocolsLabel -> 
                let currentLine,lineNumber,newRemarks,protocols = Protocols.fromRows (Some protocolsLabelPrefix) (lineNumber + 1) en  
                loop currentLine studyInfo designDescriptors publications factors assays protocols contacts (List.append remarks newRemarks) lineNumber

            | Some k when k = contactsLabel -> 
                let currentLine,lineNumber,newRemarks,contacts = Contacts.fromRows (Some contactsLabelPrefix) (lineNumber + 1) en  
                loop currentLine studyInfo designDescriptors publications factors assays protocols contacts (List.append remarks newRemarks) lineNumber

            | k -> 
                k,lineNumber,remarks, fromParts studyInfo designDescriptors publications factors assays protocols contacts
    
        let currentLine,lineNumber,remarks,item = StudyInfo.fromRows lineNumber en  
        loop currentLine item [] [] [] [] [] [] remarks lineNumber

    
    let toRows (study : ArcStudy) =
        let protocols = study.Tables |> Seq.collect (fun p -> p.GetProtocols()) |> List.ofSeq
        seq {          
            yield! StudyInfo.toRows study

            yield  SparseRow.fromValues [designDescriptorsLabel]
            yield! DesignDescriptors.toRows (Some designDescriptorsLabelPrefix) (study.StudyDesignDescriptors)

            yield  SparseRow.fromValues [publicationsLabel]
            yield! Publications.toRows (Some publicationsLabelPrefix) (study.Publications)

            yield  SparseRow.fromValues [factorsLabel]
            yield! Factors.toRows (Some factorsLabelPrefix) (study.Factors)

            yield  SparseRow.fromValues [assaysLabel]
            yield! Assays.toRows (Some assaysLabelPrefix) (List.ofSeq study.Assays)

            yield  SparseRow.fromValues [protocolsLabel]
            yield! Protocols.toRows (Some protocolsLabelPrefix) protocols

            yield  SparseRow.fromValues [contactsLabel]
            yield! Contacts.toRows (Some contactsLabelPrefix) (study.Contacts)
        }