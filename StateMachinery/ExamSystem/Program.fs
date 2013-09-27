// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
module ExamMain

open System
open System.IO
open ExamSystem
open RoomConnections

[<EntryPoint>]
let main argv =    

    
    let emptyRoom i = { RoomId = i; States = { PendingStates = []; ConsumedStates = []; CurrentState = State.Empty } }

    let defaultRoomStates = List.init 20 emptyRoom

   
    let agentRepoRef = 
                        ref { 
                            Global = new Agent<GlobalMsg>(fun _ -> async{return ()});
                            Control = new Agent<ControlInterfaceMsg>(fun _ -> async{return ()}); 
                            Rooms = (0, new Agent<RoomConnMsg>(fun _ -> async{return ()}))::[]
                        }

    let agentRepo() = !agentRepoRef

    let roomAgents = [for roomId in [1..100] -> (roomId, roomConnection agentRepo roomId)]

    let controlInterfaceAgent = controlInterface agentRepo defaultRoomStates

    let globalAgent = globalAgent agentRepo

    agentRepoRef := {
        Global  = globalAgent;
        Control = controlInterfaceAgent
        Rooms   = roomAgents;
    }

    List.iter startRoom roomAgents

    use listener = listenForConnections agentRepo
    
    let timers = roomAgents |> List.map (snd >> (timer 1000))    

    let disposes = List.map snd timers

    let startTimers = List.map (fst >> Async.Start) timers

    printfn "press any key to stop..."
    
    Console.ReadKey() |> ignore

    List.iter (fun (i:IDisposable) -> i.Dispose()) disposes       
    
    0