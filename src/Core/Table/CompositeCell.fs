﻿namespace ARCtrl

open Fable.Core

[<AttachMembers>]
[<RequireQualifiedAccess>]
type CompositeCell = 
    /// ISA-TAB term columns as ontology annotation.
    ///
    /// https://isa-specs.readthedocs.io/en/latest/isatab.html#ontology-annotations
    | Term of OntologyAnnotation
    /// Single columns like Input, Output, ProtocolREF, .. .
    | FreeText of string
    /// ISA-TAB unit columns, consisting of value and unit as ontology annotation.
    ///
    /// https://isa-specs.readthedocs.io/en/latest/isatab.html#unit
    | Unitized of string*OntologyAnnotation

    member this.isUnitized = match this with | Unitized _ -> true | _ -> false
    member this.isTerm = match this with | Term _ -> true | _ -> false
    member this.isFreeText = match this with | FreeText _ -> true | _ -> false

    /// <summary>
    /// This returns the default empty cell from an existing CompositeCell.
    /// </summary>
    member this.GetEmptyCell() =
        match this with
        | CompositeCell.Term _ -> CompositeCell.emptyTerm
        | CompositeCell.Unitized _ -> CompositeCell.emptyUnitized
        | CompositeCell.FreeText _ -> CompositeCell.emptyFreeText

    /// <summary>
    /// This function returns an array of all values as string
    ///
    /// ```fsharp
    /// match this with
    /// | FreeText s -> [|s|]
    /// | Term oa -> [| oa.NameText; defaultArg oa.TermSourceREF ""; defaultArg oa.TermAccessionNumber ""|]
    /// | Unitized (v,oa) -> [| v; oa.NameText; defaultArg oa.TermSourceREF ""; defaultArg oa.TermAccessionNumber ""|]
    /// ```
    /// </summary>
    member this.GetContent() = 
        match this with
        | FreeText s -> [|s|]
        | Term oa -> [| oa.NameText; defaultArg oa.TermSourceREF ""; defaultArg oa.TermAccessionNumber ""|]
        | Unitized (v,oa) -> [| v; oa.NameText; defaultArg oa.TermSourceREF ""; defaultArg oa.TermAccessionNumber ""|]

    /// FreeText string will be converted to unit term name.
    ///
    /// Term will be converted to unit term.
    member this.ToUnitizedCell() =
        match this with
        | Unitized _ -> this
        | FreeText text -> CompositeCell.Unitized ("", OntologyAnnotation.create(text))
        | Term term -> CompositeCell.Unitized ("", term)

    /// FreeText string will be converted to term name.
    ///
    /// Unit term will be converted to term and unit value is dropped.
    member this.ToTermCell() =
        match this with
        | Term _ -> this
        | Unitized (_,unit) -> CompositeCell.Term unit
        | FreeText text -> CompositeCell.Term(OntologyAnnotation.create(text))

    /// Will always keep `OntologyAnnotation.NameText` from Term or Unit.
    member this.ToFreeTextCell() =
        match this with
        | FreeText _ -> this
        | Term term -> FreeText(term.NameText)
        | Unitized (v,unit) -> FreeText(unit.NameText)

    // Suggest this syntax for easy "of-something" access
    member this.AsUnitized  =
        match this with
        | Unitized (v,u) -> v,u
        | _ -> failwith "Not a Unitized cell."

    member this.AsTerm =
        match this with
        | Term c -> c
        | _ -> failwith "Not a Swate TermCell."

    member this.AsFreeText =
        match this with
        | FreeText c -> c
        | _ -> failwith "Not a Swate TermCell."

    // TODO: i would really love to have an overload here accepting string input
    static member createTerm (oa:OntologyAnnotation) = Term oa

    static member createTermFromString (?name: string, ?tsr: string, ?tan: string) =
        Term <| OntologyAnnotation.create(?name = name, ?tsr = tsr, ?tan = tan)

    static member createUnitized (value: string, ?oa:OntologyAnnotation) = Unitized (value, Option.defaultValue (OntologyAnnotation()) oa)

    static member createUnitizedFromString (value: string, ?name: string, ?tsr: string, ?tan: string) = 
        Unitized <| (value, OntologyAnnotation.create(?name = name, ?tsr = tsr, ?tan = tan))

    static member createFreeText (value: string) = FreeText value
    
    static member emptyTerm = Term (OntologyAnnotation())
    static member emptyFreeText = FreeText ""
    static member emptyUnitized = Unitized ("", OntologyAnnotation())

    /// <summary>
    /// Updates current CompositeCell with information from OntologyAnnotation.
    ///
    /// For `Term`, OntologyAnnotation (oa) is fully set. For `Unitized`, oa is set as unit while value is untouched.
    /// For `FreeText` oa.NameText is set.
    /// </summary>
    /// <param name="oa"></param>
    member this.UpdateWithOA (oa:OntologyAnnotation) =
        match this with
        | CompositeCell.Term _ -> CompositeCell.createTerm oa
        | CompositeCell.Unitized (v,_) -> CompositeCell.createUnitized (v,oa)
        | CompositeCell.FreeText _ -> CompositeCell.createFreeText oa.NameText

    /// <summary>
    /// Updates current CompositeCell with information from OntologyAnnotation.
    ///
    /// For `Term`, OntologyAnnotation (oa) is fully set. For `Unitized`, oa is set as unit while value is untouched.
    /// For `FreeText` oa.NameText is set.
    /// </summary>
    /// <param name="oa"></param>
    /// <param name="cell"></param>
    static member updateWithOA (oa:OntologyAnnotation) (cell: CompositeCell) =
        cell.UpdateWithOA oa

    override this.ToString() = 
        match this with
        | Term oa -> $"{oa.NameText}"
        | FreeText s -> s
        | Unitized (v,oa) -> $"{v} {oa.NameText}"

#if FABLE_COMPILER
    //[<CompiledName("Term")>]
    static member term (oa:OntologyAnnotation) = CompositeCell.Term(oa)

    //[<CompiledName("FreeText")>]
    static member freeText (s:string) = CompositeCell.FreeText(s)

    //[<CompiledName("Unitized")>]
    static member unitized (v:string, oa:OntologyAnnotation) = CompositeCell.Unitized(v, oa)

#else
#endif