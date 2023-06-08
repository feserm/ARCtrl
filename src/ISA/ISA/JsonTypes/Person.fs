namespace ISA

open ISA.Aux
open Update

type Person = 
    {   
        ID : URI option
        LastName : string option
        FirstName : string option
        MidInitials : string option
        EMail : EMail option
        Phone : string option
        Fax : string option
        Address : string option
        Affiliation : string option
        Roles : OntologyAnnotation list option
        Comments : Comment list option  
    }

    static member make id lastName firstName midInitials email phone fax address affiliation roles comments : Person =
        {
            ID = id
            LastName = lastName
            FirstName = firstName
            MidInitials = midInitials
            EMail = email
            Phone = phone
            Fax = fax
            Address = address
            Affiliation = affiliation
            Roles = roles
            Comments = comments
        }

    static member create (?Id,?LastName,?FirstName,?MidInitials,?Email,?Phone,?Fax,?Address,?Affiliation,?Roles,?Comments) : Person =
        Person.make Id LastName FirstName MidInitials Email Phone Fax Address Affiliation Roles Comments

    static member empty =
        Person.create ()

    static member tryGetByFullName (firstName : string) (midInitials : string) (lastName : string) (persons : Person list) =
        List.tryFind (fun p -> 
            if midInitials = "" then 
                p.FirstName = Some firstName && p.LastName = Some lastName
            else 

                p.FirstName = Some firstName && p.MidInitials = Some midInitials && p.LastName = Some lastName
        ) persons

    ///// Returns true, if a person for which the predicate returns true exists in the investigation
    //static member exists (predicate : Person -> bool) (investigation:Investigation) =
    //    investigation.Contacts
    //    |> List.exists (predicate) 

    ///// Returns true, if the given person exists in the investigation
    //static member contains (person : Person) (investigation:Investigation) =
    //    exists ((=) person) investigation

    /// If an person with the given FirstName, MidInitials and LastName exists in the list, returns true
    static member existsByFullName (firstName : string) (midInitials : string) (lastName : string) (persons : Person list) =
        List.exists (fun p -> 
            if midInitials = "" then 
                p.FirstName = Some firstName && p.LastName = Some lastName
            else 

                p.FirstName = Some firstName && p.MidInitials = Some midInitials && p.LastName = Some lastName
        ) persons

    /// adds the given person to the persons  
    static member add (persons : Person list) (person : Person) =
        List.append persons [person]

    /// Updates all persons for which the predicate returns true with the given person values
    static member updateBy (predicate : Person -> bool) (updateOption:UpdateOptions) (person : Person) (persons : Person list) =
        if List.exists predicate persons 
        then
            persons
            |> List.map (fun p -> if predicate p then updateOption.updateRecordType p person else p) 
        else 
            persons

    /// Updates all persons with the same FirstName, MidInitials and LastName as the given person with its values
    static member updateByFullName (updateOption:UpdateOptions) (person : Person) (persons : Person list) =
        Person.updateBy (fun p -> p.FirstName = person.FirstName && p.MidInitials = person.MidInitials && p.LastName = person.LastName) updateOption person persons
    
    /// If a person with the given FirstName, MidInitials and LastName exists in the list, removes it
    static member removeByFullName (firstName : string) (midInitials : string) (lastName : string) (persons : Person list) =
        List.filter (fun p ->
            if midInitials = "" then
                (p.FirstName = Some firstName && p.LastName = Some lastName)
                |> not
            else
                (p.FirstName = Some firstName && p.MidInitials = Some midInitials && p.LastName = Some lastName)
                |> not
        ) persons

    // Roles
    
    /// Returns roles of a person
    static member getRoles (person : Person) =
        person.Roles
    
    /// Applies function f on roles of a person
    static member mapRoles (f : OntologyAnnotation list -> OntologyAnnotation list) (person : Person) =
        { person with 
            Roles = Option.mapDefault [] f person.Roles}
    
    /// Replaces roles of a person with the given roles
    static member setRoles (person : Person) (roles : OntologyAnnotation list) =
        { person with
            Roles = Some roles }

    // Comments
    
    /// Returns comments of a person
    static member getComments (person : Person) =
        person.Comments
    
    /// Applies function f on comments of a person
    static member mapComments (f : Comment list -> Comment list) (person : Person) =
        { person with 
            Comments = Option.mapDefault [] f person.Comments}
    
    /// Replaces comments of a person by given comment list
    static member setComments (person : Person) (comments : Comment list) =
        { person with
            Comments = Some comments }

