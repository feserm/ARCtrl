﻿module ISA.Spreadsheet.ArcAssay

open ISA
open FsSpreadsheet

let [<Literal>] obsoloteAssaysLabel = "ASSAY METADATA"
let [<Literal>] assaysLabel = "ASSAY"
let [<Literal>] contactsLabel = "ASSAY PERFORMERS"

let [<Literal>] metaDataSheetName = "Assay"

let toMetadataSheet (assay : ArcAssay) : FsWorksheet =
    let toRows (assay:ArcAssay) =
        seq {          
            yield  SparseRow.fromValues [assaysLabel]
            yield! Assays.toRows (None) [assay]

            yield SparseRow.fromValues [contactsLabel]
            yield! Contacts.toRows (None) (assay.Performers)
        }
    let sheet = FsWorksheet(metaDataSheetName)
    assay
    |> toRows
    |> Seq.iteri (fun rowI r -> SparseRow.writeToSheet (rowI + 1) r sheet)    
    sheet

let fromMetadataSheet (sheet : FsWorksheet) : ArcAssay =
    let fromRows (rows: seq<SparseRow>) =
        let en = rows.GetEnumerator()
        let rec loop lastLine assays contacts lineNumber =
               
            match lastLine with

            | Some k when k = assaysLabel || k = obsoloteAssaysLabel -> 
                let currentLine,lineNumber,_,assays = Assays.fromRows None (lineNumber + 1) en       
                loop currentLine assays contacts lineNumber

            | Some k when k = contactsLabel -> 
                let currentLine,lineNumber,_,contacts = Contacts.fromRows None (lineNumber + 1) en  
                loop currentLine assays contacts lineNumber

            | k -> 
                match assays, contacts with
                | [], [] -> ArcAssay.create(Identifier.createMissingIdentifier())
                | assays, contacts ->
                    assays
                    |> Seq.tryHead 
                    |> Option.defaultValue (ArcAssay.create(Identifier.createMissingIdentifier()))
                    |> ArcAssay.setPerformers contacts
        
        if en.MoveNext () then
            let currentLine = en.Current |> SparseRow.tryGetValueAt 0
            loop currentLine [] [] 1
            
        else
            failwith "empty assay metadata sheet"
    sheet.Rows 
    |> Seq.map SparseRow.fromFsRow
    |> fromRows


/// Reads an assay from a spreadsheet
let fromFsWorkbook (doc:FsWorkbook) = 
    // Reading the "Assay" metadata sheet. Here metadata 
    let assayMetaData = 
        
        match doc.TryGetWorksheetByName metaDataSheetName with 
        | Option.Some sheet ->
            fromMetadataSheet sheet
        | None -> 
            printfn "Cannot retrieve metadata: Assay file does not contain \"%s\" sheet." metaDataSheetName
            ArcAssay.create(Identifier.createMissingIdentifier())
    let sheets = 
        doc.GetWorksheets()
        |> List.choose ArcTable.tryFromFsWorksheet
    if sheets.IsEmpty then
        assayMetaData
    else 
        assayMetaData.Tables <- ResizeArray(sheets)
        assayMetaData

let toFsWorkbook (assay : ArcAssay) =
    let doc = new FsWorkbook()
    let metaDataSheet = toMetadataSheet (assay)
    doc.AddWorksheet metaDataSheet

    assay.Tables
    |> Seq.iter (ArcTable.toFsWorksheet >> doc.AddWorksheet)

    doc