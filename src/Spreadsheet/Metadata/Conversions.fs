namespace ARCtrl.Spreadsheet

open ARCtrl
open ARCtrl.Helper
open ARCtrl.Process

module internal Option =
 
    /// If the value matches the default, a None is returned, else a Some is returned
    let fromValueWithDefault d v =
        if d = v then None
        else Some v

    /// Applies the function f on the value of the option if it exists, else applies it on the default value. If the result value matches the default, a None is returned
    let mapDefault (d : 'T) (f: 'T -> 'T) (o : 'T option) =
        match o with
        | Some v -> f v
        | None   -> f d
        |> fromValueWithDefault d


module OntologyAnnotation =

    /// Returns the length of a subpropertylist from the aggregated strings
    ///
    /// In ISATab format, some subproperties which are stored as lists in ISAJson are stored as semicolon delimited tables 
    /// 
    /// These strings should either contain the same number of semicolon delimited elements or be empty.
    let getLengthOfAggregatedStrings (separator:char) (strings: string []) =
        strings
        |> Array.fold (fun l s ->
            if s = "" then l
            elif l = 0 then s.Split(separator).Length
            else 
                let sl = s.Split(separator).Length
                if l = sl then l
                else failwithf "The length of the aggregated string %s does not match the length of the others" s
        ) 0

    /// Returns a list of ISAJson OntologyAnnotation objects from ISATab aggregated strings
    let fromAggregatedStrings (separator:char) (terms:string) (source:string) (accessions:string) : ResizeArray<OntologyAnnotation> =
        let l = getLengthOfAggregatedStrings separator [|terms;source;accessions|]
        if l = 0 then ResizeArray()
        else 
            let terms : string [] = if terms = "" then Array.create l "" else terms.Split(separator)
            let sources : string [] = if source = "" then Array.create l "" else source.Split(separator)
            let accessions : string [] = if accessions = "" then Array.create l "" else accessions.Split(separator)
            Array.map3 (fun a b c -> OntologyAnnotation.create(a,b,c)) terms sources accessions
            |> ResizeArray

    /// Returns the aggregated ISATab OntologyAnnotation Name, ontology source and Accession number from a list of ISAJson OntologyAnnotation objects
    let toAggregatedStrings (separator:char) (oas : OntologyAnnotation []) =
        let mutable first = true
        if oas = [||] then {|TermNameAgg = ""; TermAccessionNumberAgg = ""; TermSourceREFAgg = ""|}       
        else
            oas
            |> Array.fold (fun (nameAgg,tsrAgg,tanAgg) term -> 
                let name,tsr,tan = Option.defaultValue "" term.Name, Option.defaultValue "" term.TermSourceREF, Option.defaultValue "" term.TermAccessionNumber
                if first then 
                    first <- false
                    name,tsr,tan
                else 
                    sprintf "%s%c%s" nameAgg    separator name,
                    sprintf "%s%c%s" tsrAgg     separator tsr,
                    sprintf "%s%c%s" tanAgg     separator tan
            ) ("","","")
            |> fun (nameAgg,tsrAgg,tanAgg) -> {|TermNameAgg = nameAgg; TermAccessionNumberAgg = tanAgg; TermSourceREFAgg = tsrAgg|}

module Component = 
        
    /// Returns a list of ISAJson Component objects from ISATab aggregated strings
    let fromAggregatedStrings (separator:char) (names:string) (terms:string) (source:string) (accessions:string) =
        let l = OntologyAnnotation.getLengthOfAggregatedStrings separator [|names;terms;source;accessions|]
        if l = 0 then []
        else 
            let names : string [] = if names = "" then Array.create l "" else names.Split(separator)
            let terms : string [] = if terms = "" then Array.create l "" else terms.Split(separator)
            let sources : string [] = if source = "" then Array.create l "" else source.Split(separator)
            let accessions : string [] = if accessions = "" then Array.create l "" else accessions.Split(separator)
            Array.map4 (fun a b c d -> Component.fromISAString(a,b,c,d)) names terms sources accessions
            |> List.ofArray

    /// Returns the aggregated ISATAb Component Name, Ontology Annotation value, Accession number and ontology source from a list of ISAJson Component objects
    let toAggregatedStrings (separator:char) (cs : Component list) =
        let mutable first = true
        if cs = [] then {|NameAgg = ""; TermNameAgg = "";  TermAccessionNumberAgg = "";  TermSourceREFAgg = ""; |}
        else
            cs
            |> List.map Component.toStringObject
            |> List.fold (fun (nameAgg,termAgg,tsrAgg,tanAgg) (name,term) ->     
                if first then 
                    first <- false
                    name,term.TermName,term.TermSourceREF,term.TermAccessionNumber
                else 
                sprintf "%s%c%s" nameAgg    separator name,
                sprintf "%s%c%s" termAgg    separator term.TermName,
                sprintf "%s%c%s" tsrAgg     separator term.TermSourceREF,
                sprintf "%s%c%s" tanAgg     separator term.TermAccessionNumber
            ) ("","","","")
            |> fun (nameAgg,termAgg,tsrAgg,tanAgg) -> {|NameAgg = nameAgg; TermNameAgg = termAgg; TermAccessionNumberAgg = tanAgg; TermSourceREFAgg = tsrAgg|}

module ProtocolParameter =

    /// Returns a list of ISAJson ProtocolParameter objects from ISATab aggregated strings
    let fromAggregatedStrings (separator:char) (terms:string) (source:string) (accessions:string) =
        OntologyAnnotation.fromAggregatedStrings separator terms source accessions
        |> ResizeArray.map (Some >> (ProtocolParameter.make None))

    /// Returns the aggregated ISATAb Ontology Annotation value, Accession number and ontology source from a list of ISAJson ProtocolParameter objects
    let toAggregatedStrings (separator:char) (oas : ProtocolParameter list) =
        let mutable first = true
        if oas = [] then {|TermNameAgg = ""; TermAccessionNumberAgg = ""; TermSourceREFAgg = ""|}
        else
            oas
            |> List.map ProtocolParameter.toStringObject
            |> List.fold (fun (nameAgg,tsrAgg,tanAgg) term -> 
                if first then 
                    first <- false
                    term.TermName,term.TermSourceREF,term.TermAccessionNumber
                else 
                    sprintf "%s%c%s" nameAgg    separator term.TermName,
                    sprintf "%s%c%s" tsrAgg     separator term.TermSourceREF,
                    sprintf "%s%c%s" tanAgg     separator term.TermAccessionNumber
            ) ("","","")
            |> fun (nameAgg,tsrAgg,tanAgg) -> {|TermNameAgg = nameAgg; TermAccessionNumberAgg = tanAgg; TermSourceREFAgg = tsrAgg|}

    