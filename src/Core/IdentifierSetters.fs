﻿module ARCtrl.IdentifierSetters

open ARCtrl.Helper 
open Identifier
 

let setArcTableName (newName: string) (table: ArcTable) =
    checkValidCharacters newName
    table.Name <- newName
    table

let setAssayIdentifier (newIdentifier: string) (assay: ArcAssay) =
    checkValidCharacters newIdentifier
    assay.Identifier <- newIdentifier
    assay

let setStudyIdentifier (newIdentifier: string) (study: ArcStudy) =
    checkValidCharacters newIdentifier
    study.Identifier <- newIdentifier
    study

let setInvestigationIdentifier (newIdentifier: string) (investigation: ArcInvestigation) =
    checkValidCharacters newIdentifier
    investigation.Identifier <- newIdentifier
    investigation
