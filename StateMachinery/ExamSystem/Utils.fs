namespace ExamSystem

module Utils = 
    let applyTo funcs elem  = List.map(fun f -> f elem) funcs
    let applyTupleTo funcs elem = List.map (fun f -> f <|| elem) funcs

module Option = 
    let bindDo f = 
        function
            | Some(x) -> f x |> ignore
            | None ->  ()
    

