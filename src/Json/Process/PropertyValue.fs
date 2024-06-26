﻿namespace ARCtrl.Json


open Thoth.Json.Core

open ARCtrl
open ARCtrl.Process
open System.IO

module PropertyValue = 
    
    module ROCrate =
        
        let genID (p : IPropertyValue<'T>) = 
            match p.GetCategory() with
            | Some t -> $"{p.GetAdditionalType}/{t}{p.GetValue()}{p.GetUnit()}"
            | None -> $"#Empty{p.GetAdditionalType}"

        let encoder<'T> (pv : IPropertyValue<'T>) = 
            let categoryName, categoryURL = 
                match pv.GetCategory() with
                | Some oa ->
                    oa.Name, oa.TermAccessionNumber
                | None -> None, None
            let value, valueCode = 
                match pv.GetValue() with
                | Some v -> 
                    match v with
                    | Value.Name t -> Some (Encode.string t), None
                    | Value.Int t -> Some (Encode.int t), None
                    | Value.Float t -> Some (Encode.float t), None
                    | Value.Ontology oa -> oa.Name |> Option.map Encode.string, oa.TermAccessionNumber |> Option.map Encode.string
                | None -> None, None
            let unit,unitCode = 
                match pv.GetUnit() with
                | Some oa -> 
                    oa.Name, oa.TermAccessionNumber
                | None -> None, None
            [
                "@id", Encode.string (pv |> genID)
                "@type", Encode.string "PropertyValue"
                "additionalType", Encode.string (pv.GetAdditionalType())

                Encode.tryInclude "category" Encode.string categoryName
                Encode.tryInclude "categoryCode" Encode.string categoryURL
                Encode.tryInclude "value" id value
                Encode.tryInclude "valueCode" id valueCode
                Encode.tryInclude "unit" Encode.string unit
                Encode.tryInclude "unitCode" Encode.string unitCode
                "@context", ROCrateContext.PropertyValue.context_jsonvalue
            ]
            |> Encode.choose
            |> Encode.object

        let decoder<'T> (create : createPVFunction<'T>) : Decoder<'T>= 
            Decode.object (fun get ->
                let category =
                    let name = get.Optional.Field "category" Decode.string
                    let code = get.Optional.Field "categoryCode" Decode.string
                    if name.IsNone && code.IsNone then None
                    else Some(OntologyAnnotation.create(?name = name, ?tan = code))
                let value = 
                    let value = get.Optional.Field "value" Decode.string
                    let code = get.Optional.Field "valueCode" Decode.string
                    if value.IsNone && code.IsNone then None
                    else Value.fromOptions value None code
                let unit = 
                    let name = get.Optional.Field "unit" Decode.string
                    let code = get.Optional.Field "unitCode" Decode.string
                    if name.IsNone && code.IsNone then None
                    else Some(OntologyAnnotation.create(?name = name, ?tan = code))
                create category value unit 
            )