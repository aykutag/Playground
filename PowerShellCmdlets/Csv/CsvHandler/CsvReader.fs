namespace CsvHandler

open System
open System.Reflection
open System.IO
open DataEmitter
open FSharp.Data.Csv

module CsvReader = 
    let rand = System.Random()

    let randomName() = rand.Next (0, 999999) |> string

    let load (stream : Stream) = 
        let csv = CsvFile.Load(stream).Cache()

        let headers = match csv.Headers with 
                        | Some(h) -> h |> Array.toList
                        | None -> [0..csv.NumberOfColumns] |> List.map (fun i -> "Unknown Header " + (string i))

        let typeData = make (randomName()) (headers |> List.map (fun i -> (i, typeof<string>)))

        [for item in csv.Data do       
            let data = Activator.CreateInstance(typeData, item.Columns |> Array.map (fun i -> i :> obj))         
            yield data]