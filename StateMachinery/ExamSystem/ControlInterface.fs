namespace ExamSystem

open ExamSystem
open ExamSystem.NetworkUtils
open ExamSystem.StateManager
open ExamSystem.Utils
open ExamSystem.CommunicationProtocol
open System.Net

module ControlInterface = 
    
    type ControlInterfaceRepoData = 
        {
            Connections: Sockets.TcpClient list
            RoomStates : Room list 
        }
        
    let updateRoomsWithState state room roomCollection = { room with States = state} :: List.filter ((<>) room) roomCollection
    
    let rec listenForControlCommands (controlConn:Agent<ControlInterfaceMsg>) client =         
        async {
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


    /// The control interface agent.  Handles room state requests
    let controlInterface agentRepo (defaultRoomStates:Room list) =     
        Agent<ControlInterfaceMsg>.Start(fun inbox ->

            let postDisconnect client = inbox.Post (ControlInterfaceMsg.Disconnect client)

            let rec loop connections rooms = 
               async {
                    let! msg = inbox.Receive()

                    let (conn, newRooms) = 
                        match msg with 
                            | ControlInterfaceMsg.Connect client ->     

                                monitor isConnected postDisconnect client |> Async.Start

                                (inbox, client) |> applyTupleTo [listenForControlCommands] 
                                                |> List.iter Async.Start

                                (client::connections, rooms)

                            | ControlInterfaceMsg.Disconnect client -> (connections |> removeTcp <| client, rooms)

                            | ControlInterfaceMsg.Broadcast str -> (str |> broadcastStr connections |> snd, rooms)

                            | ControlInterfaceMsg.Shutdown -> 
                                "Shutting down" |> strToBytes |> broadcast connections |> ignore
                                List.iter closeClient connections
                                ([], [])

                            | ControlInterfaceMsg.GetRoom roomNum ->
                                let room = List.find (fun (r:Room) -> r.RoomId = roomNum) rooms
                                
                                inbox.Post (ControlInterfaceMsg.Broadcast (roomString room))

                                (connections, rooms)

                            | ControlInterfaceMsg.Advance roomNum ->
                                let (room, newStates) = (roomNum, rooms) ||> findRoomAndApply advance                    
                                
                                postToRoom (agentRepo()) roomNum (RoomConnMsg.Broadcast <| sprintf "room %d advnced" roomNum)

                                (connections, rooms |> updateRoomsWithState newStates room)

                            | ControlInterfaceMsg.Reverse roomNum ->
                                let (room, newStates) = (roomNum, rooms) ||> findRoomAndApply reverse   
                                                  
                                postToRoom (agentRepo()) roomNum (RoomConnMsg.Broadcast <| sprintf "room %d reversed" roomNum)
                                
                                (connections, rooms |> updateRoomsWithState newStates room)

                            | ControlInterfaceMsg.AddParticipant (roomNum, participantId) ->
                                postToRoom (agentRepo()) roomNum (RoomConnMsg.Broadcast <| sprintf "particpiant %d add to room %d" participantId roomNum)
                                (connections, rooms)                        

                    return! loop conn newRooms
               }
            loop [] defaultRoomStates
        )


