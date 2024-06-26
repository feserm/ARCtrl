﻿namespace ARCtrl.Json

open Thoth.Json.Core

open ARCtrl   

open JsonHelper

module ArcInvestigation = 
    let encoder (inv:ArcInvestigation) = 
        Encode.object [ 
            "Identifier", Encode.string inv.Identifier
            if inv.Title.IsSome then
                "Title", Encode.string inv.Title.Value
            if inv.Description.IsSome then
                "Description", Encode.string inv.Description.Value
            if inv.SubmissionDate.IsSome then
                "SubmissionDate", Encode.string inv.SubmissionDate.Value
            if inv.PublicReleaseDate.IsSome then
                "PublicReleaseDate", Encode.string inv.PublicReleaseDate.Value
            if inv.OntologySourceReferences.Length <> 0 then
                "OntologySourceReferences", EncoderOntologySourceReferences inv.OntologySourceReferences
            if inv.Publications.Length <> 0 then
                "Publications", EncoderPublications inv.Publications
            if inv.Contacts.Length <> 0 then
                "Contacts", EncoderPersons inv.Contacts
            if inv.Assays.Count <> 0 then
                "Assays", Encode.seq (Seq.map ArcAssay.encoder inv.Assays) 
            if inv.Studies.Count <> 0 then
                "Studies", Encode.seq (Seq.map ArcStudy.encoder inv.Studies)
            if inv.RegisteredStudyIdentifiers.Count <> 0 then
                "RegisteredStudyIdentifiers", Encode.seq (Seq.map Encode.string inv.RegisteredStudyIdentifiers)
            if inv.Comments.Length <> 0 then
                "Comments", EncoderComments inv.Comments
            // remarks are ignored for whatever reason
        ]
  
    let decoder : Decoder<ArcInvestigation> =
        let DecodeAssays : Decoder<ArcAssay list> = Decode.list ArcAssay.decoder 
        let DecodeStudies : Decoder<ArcStudy list> = Decode.list ArcStudy.decoder
        let tryGetAssays (get:Decode.IGetters) (fieldName:string) = get.Optional.Field(fieldName) DecodeAssays |> Option.map ResizeArray |> Option.defaultValue (ResizeArray())
        let tryGetStudies (get:Decode.IGetters) (fieldName:string) = get.Optional.Field(fieldName) DecodeStudies |> Option.map ResizeArray |> Option.defaultValue (ResizeArray()) 
        Decode.object (fun get ->
        ArcInvestigation.make 
            (get.Required.Field("Identifier") Decode.string)
            (get.Optional.Field("Title") Decode.string)
            (get.Optional.Field("Description") Decode.string)
            (get.Optional.Field("SubmissionDate") Decode.string)
            (get.Optional.Field("PublicReleaseDate") Decode.string)
            (tryGetOntologySourceReferences get "OntologySourceReferences")
            (tryGetPublications get "Publications")
            (tryGetPersons get "Contacts")
            (tryGetAssays get "Assays")
            (tryGetStudies get "Studies")
            (tryGetStringResizeArray get "RegisteredStudyIdentifiers")
            (tryGetComments get "Comments")
            [||]
        )

    /// exports in json-ld format
    let toJsonldString (a:ArcInvestigation) = 
        Investigation.encoder (ConverterOptions(SetID=true,IsJsonLD=true)) (a.ToInvestigation())
        |> Encode.toJsonString 2

    let toJsonldStringWithContext (a:ArcInvestigation) = 
        Investigation.encoder (ConverterOptions(SetID=true,IsJsonLD=true)) (a.ToInvestigation())
        |> Encode.toJsonString 2

    let fromJsonString (s:string) = 
        GDecode.fromJsonString (Investigation.decoder (ConverterOptions())) s
        |> ArcInvestigation.fromInvestigation

    let toJsonString (a:ArcInvestigation) = 
        Investigation.encoder (ConverterOptions()) (a.ToInvestigation())
        |> Encode.toJsonString 2

    let toArcJsonString (a:ArcInvestigation) : string =
        let spaces = 0
        Encode.toJsonString spaces (encoder a)

    let fromArcJsonString (jsonString: string) =
        try GDecode.fromJsonString decoder jsonString with
        | e -> failwithf "Error. Unable to parse json string to ArcInvestigation: %s" e.Message

[<AutoOpen>]
module ArcInvestigationExtensions =

    type ArcInvestigation with
        static member fromArcJsonString (jsonString: string) : ArcInvestigation = 
            try GDecode.fromJsonString ArcInvestigation.decoder jsonString with
            | e -> failwithf "Error. Unable to parse json string to ArcInvestigation: %s" e.Message

        member this.ToArcJsonString(?spaces) : string =
            let spaces = defaultArg spaces 0
            Encode.toJsonString spaces (ArcInvestigation.encoder this)

        static member toArcJsonString(a:ArcInvestigation) = a.ToArcJsonString()