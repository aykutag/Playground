namespace ExamSystem

open ExamSystem
open ExamSystem.NetworkUtils
open ExamSystem.StateManager
open ExamSystem.Utils
open ExamSystem.CommunicationProtocol
open System.Net

module ControlInterface = 
    
    type private ControlInterfaceRepoData = 
        {
            Connections: Sockets.TcpClient list
            RoomStates : Room list
            AgentRepo : AgentRepo
            Inbox : Agent<ControlInterfaceMsg>
        }
        
    let updateRoomsWithState state room roomCollection = { room with States = state} :: List.filter ((<>) room) roomCollection
    
    let rec private listenForControlCommands (controlConn:Agent<ControlInterfaceMsg>) client =         
        async {
            do! Async.SwitchToNewThread() 
            try
                for message in client |> packets do
                    let action = 
                        match message with 
                            | AdvanceCmd roomNum -> ControlInterfaceMsg.Advance roomNum
                            | ReverseCmd roomNum -> ControlInterfaceMsg.Reverse roomNum
                            | QueryRoom  roomNum -> ControlInterfaceMsg.GetRoom roomNum
                            | _ -> ControlInterfaceMsg.Broadcast ("Unknown command: " + message)
                    
                    controlConn.Post action                    
            with
                | exn -> controlConn.Post (ControlInterfaceMsg.Disconnect client)
        }

    let private shutdown connections =         
        "Shutting down" |> strToBytes |> broadcast connections |> ignore
        List.iter closeClient connections          
              


    let private getRoom data roomNum = List.find (fun (r:Room) -> r.RoomId = roomNum) data.RoomStates
    
    let private broadcastRoomState data room = data.Inbox.Post (ControlInterfaceMsg.Broadcast (roomString room))                                    

    let private updateRoomState data roomNum description step = 
        let (room, newStates) = (roomNum, data.RoomStates) ||> findRoomAndApply step                    
                                
        postToRoom data.AgentRepo roomNum (RoomConnMsg.Broadcast <| description)

        data.RoomStates |> updateRoomsWithState newStates room

    let private addParticipant data roomNum participantId =         
        postToRoom data.AgentRepo roomNum (RoomConnMsg.Broadcast <| sprintf "particpiant %d add to room %d" participantId roomNum)
                            
    let private postDisconnect (inbox:Agent<ControlInterfaceMsg>) client = inbox.Post (ControlInterfaceMsg.Disconnect client)                            

    let private clientConnected data client = 
        monitor isConnected (postDisconnect data.Inbox) client |> Async.Start

        (data.Inbox, client) ||> listenForControlCommands |> Async.Start

    let private processControlMessage data = function        
        | ControlInterfaceMsg.Connect client ->     
            
            clientConnected data client

            { data with Connections = client::data.Connections }

        | ControlInterfaceMsg.Disconnect client -> 
            { data with Connections = data.Connections |> removeTcp <| client }

        | ControlInterfaceMsg.Broadcast str -> 
            { data with Connections = str |> broadcastStr data.Connections |> snd }

        | ControlInterfaceMsg.Shutdown -> 
            shutdown data.Connections

            { data with Connections =  [] }

        | ControlInterfaceMsg.GetRoom roomNum -> 
            roomNum |> getRoom data |> broadcastRoomState data
            data

        | ControlInterfaceMsg.Advance roomNum -> 
            { data with RoomStates = advance |> updateRoomState data roomNum (sprintf "room %d advnced" roomNum)  }

        | ControlInterfaceMsg.Reverse roomNum ->
            { data with RoomStates = reverse |> updateRoomState data roomNum (sprintf "room %d reversed" roomNum) }                                

        | ControlInterfaceMsg.AddParticipant (roomNum, participantId) -> 
            addParticipant data roomNum participantId 
            data

    /// The control interface agent.  Handles room state requests
    let controlInterface agentRepo (defaultRoomStates:Room list) =     
        Agent<ControlInterfaceMsg>.Start(fun inbox ->

            let rec loop state = 
               async {
                    let! msg = inbox.Receive()
                    
                    let newState = processControlMessage state msg

                    return! loop newState
               }
            loop { 
                AgentRepo = agentRepo()
                RoomStates = defaultRoomStates
                Connections = []
                Inbox = inbox
            }
        )


