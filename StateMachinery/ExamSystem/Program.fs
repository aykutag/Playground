// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
module ExamMain

open System
open System.IO
open ExamSystem.StateManager
open ExamSystem.RoomConnections

let dispose (i:IDisposable) = i.Dispose()

let defaultRoomStates() = 
    let emptyRoom i = { RoomId = i; States = { PendingStates = []; ConsumedStates = []; CurrentState = State.Empty } }

    List.init 20 emptyRoom

let initializeAgentRepos() = 
    let agentRepoRef = 
                        ref { 
                            Global = new Agent<GlobalMsg>(fun _ -> async{return ()});
                            Control = new Agent<ControlInterfaceMsg>(fun _ -> async{return ()}); 
                            Rooms = (0, new Agent<RoomConnMsg>(fun _ -> async{return ()}))::[]
                        }

    let agentRepo() = !agentRepoRef

    let roomAgents = [for roomId in [1..100] -> (roomId, roomConnection agentRepo roomId)]

    let controlInterfaceAgent = controlInterface agentRepo (defaultRoomStates())

    let globalAgent = globalAgent agentRepo

    agentRepoRef := {
        Global  = globalAgent;
        Control = controlInterfaceAgent
        Rooms   = roomAgents;
    }

    !agentRepoRef

[<EntryPoint>]
let main argv =    
    
    let agentRepos = initializeAgentRepos()
    
    agentRepos.Rooms |> List.iter startRoom 

    use listener = listenForConnections agentRepos
    
    let (timer, disposable) = timer 1000 agentRepos

    timer |> Async.Start |> ignore

    printfn "press any key to stop..."
    
    Console.ReadKey() |> ignore

    dispose disposable
    
    0