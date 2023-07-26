﻿module ArcStudy.Tests

open ISA

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let private test_create =
    testList "create" [
        testCase "constructor" <| fun _ ->
            let identifier = "MyIdentifier"
            let title = "Study Title"
            let description = "Study Description"
            let submissionDate = "2023-07-19"
            let publicReleaseDate = "2023-12-31"
            let publications = [Publication.create("Publication 1")]
            let contacts = [Person.create(FirstName = "John", LastName = "Doe")]
            let studyDesignDescriptors = [OntologyAnnotation.fromString("Design Descriptor")]
            let tables = ResizeArray([ArcTable.init("Table 1")])
            let assays = ResizeArray([ArcAssay.init("Assay 1")])
            let factors = [Factor.create("Factor 1")]
            let comments = [Comment.create("Comment 1")]

            let actual =
                ArcStudy(
                    identifier = identifier,
                    title = title,
                    description = description,
                    submissionDate = submissionDate,
                    publicReleaseDate = publicReleaseDate,
                    publications = publications,
                    contacts = contacts,
                    studyDesignDescriptors = studyDesignDescriptors,
                    tables = tables,
                    assays = assays,
                    factors = factors,
                    comments = comments
                )

            Expect.equal actual.Identifier identifier "identifier"
            Expect.equal actual.Title (Some title) "Title"
            Expect.equal actual.Description (Some description) "Description"
            Expect.equal actual.SubmissionDate (Some submissionDate) "SubmissionDate"
            Expect.equal actual.PublicReleaseDate (Some publicReleaseDate) "PublicReleaseDate"
            Expect.equal actual.Publications publications "Publications"
            Expect.equal actual.Contacts contacts "Contacts"
            Expect.equal actual.StudyDesignDescriptors studyDesignDescriptors "StudyDesignDescriptors"
            Expect.equal actual.Tables tables "Tables"
            Expect.equal actual.Assays assays "Assays"
            Expect.equal actual.Factors factors "Factors"
            Expect.equal actual.Comments comments "Comments"

        testCase "create" <| fun _ ->
            let identifier = "MyIdentifier"
            let title = "Study Title"
            let description = "Study Description"
            let submissionDate = "2023-07-19"
            let publicReleaseDate = "2023-12-31"
            let publications = [Publication.create("Publication 1")]
            let contacts = [Person.create(FirstName = "John", LastName = "Doe")]
            let studyDesignDescriptors = [OntologyAnnotation.fromString("Design Descriptor")]
            let tables = ResizeArray([ArcTable.init("Table 1")])
            let assays = ResizeArray([ArcAssay.init("Assay 1")])
            let factors = [Factor.create("Factor 1")]
            let comments = [Comment.create("Comment 1")]

            let actual = ArcStudy.create(
                identifier = identifier,
                title = title,
                description = description,
                submissionDate = submissionDate,
                publicReleaseDate = publicReleaseDate,
                publications = publications,
                contacts = contacts,
                studyDesignDescriptors = studyDesignDescriptors,
                tables = tables,
                assays = assays,
                factors = factors,
                comments = comments
            )

            Expect.equal actual.Identifier identifier "identifier"
            Expect.equal actual.Title (Some title) "Title"
            Expect.equal actual.Description (Some description) "Description"
            Expect.equal actual.SubmissionDate (Some submissionDate) "SubmissionDate"
            Expect.equal actual.PublicReleaseDate (Some publicReleaseDate) "PublicReleaseDate"
            Expect.equal actual.Publications publications "Publications"
            Expect.equal actual.Contacts contacts "Contacts"
            Expect.equal actual.StudyDesignDescriptors studyDesignDescriptors "StudyDesignDescriptors"
            Expect.equal actual.Tables tables "Tables"
            Expect.equal actual.Assays assays "Assays"
            Expect.equal actual.Factors factors "Factors"
            Expect.equal actual.Comments comments "Comments"

        testCase "init" <| fun _ ->
            let identifier = "MyIdentifier"
            let actual = ArcStudy.init(identifier)
            Expect.equal actual.Identifier identifier "identifier"
            Expect.equal actual.Title None "Title"
            Expect.equal actual.Description None "Description"
            Expect.equal actual.SubmissionDate None "SubmissionDate"
            Expect.equal actual.PublicReleaseDate None "PublicReleaseDate"
            Expect.isEmpty actual.Publications "Publications"
            Expect.isEmpty actual.Contacts "Contacts"
            Expect.isEmpty actual.StudyDesignDescriptors "StudyDesignDescriptors"
            Expect.isEmpty actual.Tables "Tables"
            Expect.isEmpty actual.Assays "Assays"
            Expect.isEmpty actual.Factors "Factors"
            Expect.isEmpty actual.Comments "Comments"
        testCase "make" <| fun _ ->
            let identifier = "MyIdentifier"
            let title = Some "Study Title"
            let description = Some "Study Description"
            let submissionDate = Some "2023-07-19"
            let publicReleaseDate = Some "2023-12-31"
            let publications = [Publication.create("Publication 1")]
            let contacts = [Person.create(FirstName = "John", LastName = "Doe")]
            let studyDesignDescriptors = [OntologyAnnotation.fromString("Design Descriptor")]
            let tables = ResizeArray([ArcTable.init("Table 1")])
            let assays = ResizeArray([ArcAssay.init("Assay 1")])
            let factors = [Factor.create("Factor 1")]
            let comments = [Comment.create("Comment 1")]

            let actual = 
                ArcStudy.make
                    identifier
                    title
                    description
                    submissionDate
                    publicReleaseDate
                    publications
                    contacts
                    studyDesignDescriptors
                    tables
                    assays
                    factors
                    comments

            Expect.equal actual.Identifier identifier "Identifier"
            Expect.equal actual.Title title "Title"
            Expect.equal actual.Description description "Description"
            Expect.equal actual.SubmissionDate submissionDate "SubmissionDate"
            Expect.equal actual.PublicReleaseDate publicReleaseDate "PublicReleaseDate"
            Expect.equal actual.Publications publications "Publications"
            Expect.equal actual.Contacts contacts "Contacts"
            Expect.equal actual.StudyDesignDescriptors studyDesignDescriptors "StudyDesignDescriptors"
            Expect.equal actual.Tables tables "Tables"
            Expect.equal actual.Assays assays "Assays"
            Expect.equal actual.Factors factors "Factors"
            Expect.equal actual.Comments comments "Comments"
    ]

let tests_copy = 
    testList "copy" [
        let _study_identifier = "My Study"
        let _study_description = Some "Lorem Ipsum"
        let _assay_identifier = "My Assay"
        let _assay_technologyPlatform = "Awesome Technology"
        let createTestStudy() =
            let s = ArcStudy(_study_identifier)
            s.Description <- _study_description
            let a = ArcAssay(_assay_identifier, technologyPlatform = _assay_technologyPlatform)
            s.AddAssay(a)
            s
        testCase "ensure test study" <| fun _ -> 
            let study = createTestStudy()
            Expect.equal study.Identifier _study_identifier "_study_identifier"
            Expect.equal study.Description _study_description "_study_description"
            Expect.equal study.AssayCount 1 "AssayCount"
            let assay = study.GetAssayAt(0)
            Expect.equal assay.Identifier _assay_identifier "_assay_identifier"
            Expect.equal assay.TechnologyPlatform (Some _assay_technologyPlatform) "_assay_technologyPlatform"
        testCase "test mutable fields" <| fun _ -> 
            let newDesciption = Some "New Description"
            let newPublicReleaseDate = Some "01.01.2000"
            let study = createTestStudy()
            let copy = study.Copy()
            copy.Description <- newDesciption
            copy.PublicReleaseDate <- newPublicReleaseDate
            let checkSourceStudy =
                Expect.equal study.Identifier _study_identifier "_study_identifier"
                Expect.equal study.Description _study_description "_study_description"
                Expect.equal study.PublicReleaseDate None "PublicReleaseDate"
                Expect.equal study.AssayCount 1 "AssayCount"
                let assay = study.GetAssayAt(0)
                Expect.equal assay.Identifier _assay_identifier "_assay_identifier"
                Expect.equal assay.TechnologyPlatform (Some _assay_technologyPlatform) "_assay_technologyPlatform"
            let checkCopy =
                Expect.equal copy.Identifier _study_identifier "copy _study_identifier"
                Expect.equal copy.Description newDesciption "copy _study_description"
                Expect.equal copy.PublicReleaseDate newPublicReleaseDate "copy PublicReleaseDate"
                Expect.equal copy.AssayCount 1 "copy AssayCount"
                let assay = copy.GetAssayAt(0)
                Expect.equal assay.Identifier _assay_identifier "copy _assay_identifier"
                Expect.equal assay.TechnologyPlatform (Some _assay_technologyPlatform) "copy _assay_technologyPlatform"
            ()
        testCase "test mutable fields on assay children" <| fun _ -> 
            let newTechnologyPlatform = Some "New TechnologyPlatform"
            let newTechnologyType = Some <| OntologyAnnotation.fromString("nice technology type")
            let study = createTestStudy()
            let copy = study.Copy()
            let copy_assay = copy.GetAssayAt(0)
            copy_assay.TechnologyType <- newTechnologyType
            copy_assay.TechnologyPlatform <- newTechnologyPlatform
            let checkSourceStudy =
                Expect.equal study.Identifier _study_identifier "_study_identifier"
                Expect.equal study.Description _study_description "_study_description"
                Expect.equal study.PublicReleaseDate None "PublicReleaseDate"
                Expect.equal study.AssayCount 1 "AssayCount"
                let assay = study.GetAssayAt(0)
                Expect.equal assay.Identifier _assay_identifier "_assay_identifier"
                Expect.equal assay.TechnologyPlatform (Some _assay_technologyPlatform) "_assay_technologyPlatform"
                Expect.equal assay.TechnologyType None "TechnologyType"
            let checkCopy =
                Expect.equal copy.Identifier _study_identifier "copy _study_identifier"
                Expect.equal copy.Description _study_description "copy _study_description"
                Expect.equal copy.AssayCount 1 "copy AssayCount"
                let assay = copy.GetAssayAt(0)
                Expect.equal assay.Identifier _assay_identifier "copy _assay_identifier"
                Expect.equal assay.TechnologyPlatform newTechnologyPlatform "copy _assay_technologyPlatform"
                Expect.equal assay.TechnologyType newTechnologyType "TechnologyType"
            ()
    ]

let main = 
    testList "ArcStudy" [
        tests_copy
        test_create
    ]



