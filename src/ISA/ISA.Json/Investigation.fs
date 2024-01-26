namespace ARCtrl.ISA.Json

#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif
open ARCtrl.ISA

module Investigation =
    
    
    let genID (i:Investigation) : string = 
        "./"
        // match i.ID with
        // | Some id -> URI.toString id
        // | None -> match i.FileName with
        //           | Some n -> "#Study_" + n.Replace(" ","_")
        //           | None -> match i.Identifier with
        //                     | Some id -> "#Study_" + id.Replace(" ","_")
        //                     | None -> match i.Title with
        //                               | Some t -> "#Study_" + t.Replace(" ","_")
        //                               | None -> "#EmptyStudy"
    let encoder (options : ConverterOptions) (oa : obj) = 
        [
            if options.SetID then 
                "@id",  GEncode.toJsonString (oa :?> Investigation |> genID)
            else 
                GEncode.tryInclude "@id"  GEncode.toJsonString (oa |> GEncode.tryGetPropertyValue "ID")
            if options.IncludeType then 
                "@type",  GEncode.toJsonString "Investigation"
            GEncode.tryInclude "filename"  GEncode.toJsonString (oa |> GEncode.tryGetPropertyValue "FileName")
            GEncode.tryInclude "identifier"  GEncode.toJsonString (oa |> GEncode.tryGetPropertyValue "Identifier")
            GEncode.tryInclude "title"  GEncode.toJsonString (oa |> GEncode.tryGetPropertyValue "Title")
            GEncode.tryInclude "description"  GEncode.toJsonString (oa |> GEncode.tryGetPropertyValue "Description")
            GEncode.tryInclude "submissionDate"  GEncode.toJsonString (oa |> GEncode.tryGetPropertyValue "SubmissionDate")
            GEncode.tryInclude "publicReleaseDate"  GEncode.toJsonString (oa |> GEncode.tryGetPropertyValue "PublicReleaseDate") 
            GEncode.tryInclude "ontologySourceReferences" (OntologySourceReference.encoder options) (oa |> GEncode.tryGetPropertyValue "OntologySourceReferences")
            GEncode.tryInclude "publications" (Publication.encoder options) (oa |> GEncode.tryGetPropertyValue "Publications")
            GEncode.tryInclude "people" (Person.encoder options) (oa |> GEncode.tryGetPropertyValue "Contacts")
            GEncode.tryInclude "studies" (Study.encoder options) (oa |> GEncode.tryGetPropertyValue "Studies")
            GEncode.tryInclude "comments" (Comment.encoder options) (oa |> GEncode.tryGetPropertyValue "Comments")
            if options.IncludeContext then
                "@context", ROCrateContext.Investigation.context_jsonvalue
            ]
        |> GEncode.choose
        |> Encode.object

    let encodeRoCrate (options : ConverterOptions) (oa : obj) = 
        [
            GEncode.tryInclude "@type"  GEncode.toJsonString (Some "CreativeWork")
            GEncode.tryInclude "@id"  GEncode.toJsonString (Some "ro-crate-metadata.json")
            GEncode.tryInclude "about" (encoder options) (Some oa)
            "conformsTo", ROCrateContext.ROCrate.conformsTo_jsonvalue
            if options.IncludeContext then
                "@context", ROCrateContext.ROCrate.context_jsonvalue
            ]
        |> GEncode.choose
        |> Encode.object

    let decoder (options : ConverterOptions) : Decoder<Investigation> =
        Decode.object (fun get ->
            {
                ID = get.Optional.Field "@id" Decode.string
                FileName = get.Optional.Field "filename" Decode.string
                Identifier = get.Optional.Field "identifier" Decode.string
                Title = get.Optional.Field "title" Decode.string
                Description = get.Optional.Field "description" Decode.string
                SubmissionDate = get.Optional.Field "submissionDate" Decode.string
                PublicReleaseDate = get.Optional.Field "publicReleaseDate" Decode.string
                OntologySourceReferences = get.Optional.Field "ontologySourceReferences" (Decode.list (OntologySourceReference.decoder options))
                Publications = get.Optional.Field "publications" (Decode.list (Publication.decoder options))
                Contacts = get.Optional.Field "people" (Decode.list (Person.decoder options))
                Studies = get.Optional.Field "studies" (Decode.list (Study.decoder options))
                Comments = get.Optional.Field "comments" (Decode.list (Comment.decoder options))
                Remarks = []
            }
        )

    let fromJsonString (s:string) = 
        GDecode.fromJsonString (decoder (ConverterOptions())) s

    let toJsonString (p:Investigation) = 
        encoder (ConverterOptions()) p
        |> Encode.toString 2

    /// exports in json-ld format
    let toJsonldString (i:Investigation) = 
        encoder (ConverterOptions(SetID=true,IncludeType=true)) i
        |> Encode.toString 2
    let toJsonldStringWithContext (i:Investigation) = 
        encoder (ConverterOptions(SetID=true,IncludeType=true,IncludeContext=true)) i
        |> Encode.toString 2
    let toRoCrateString (i:Investigation) = 
        encodeRoCrate (ConverterOptions(SetID=true,IncludeType=true,IncludeContext=true,IsRoCrate=true)) i
        |> Encode.toString 2