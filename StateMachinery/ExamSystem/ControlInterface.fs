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

    let shutdown connections =         
        "Shutting down" |> strToBytes |> broadcast connections |> ignore
        List.iter closeClient connections
        ([], [])


    let getRoom connections rooms roomNum (inbox:Agent<ControlInterfaceMsg>) =         
        let room = List.find (fun (r:Room) -> r.RoomId = roomNum) rooms
                                
        inbox.Post (ControlInterfaceMsg.Broadcast (roomString room))

        (connections, rooms)

    let updateRoomState agentRepo connections rooms roomNum description step = 
        let (room, newStates) = (roomNum, rooms) ||> findRoomAndApply step                    
                                
        postToRoom (agentRepo()) roomNum (RoomConnMsg.Broadcast <| description)

        (connections, rooms |> updateRoomsWithState newStates room)

    let addParticipant agentRepo connections rooms roomNum participantId =         
        postToRoom (agentRepo()) roomNum (RoomConnMsg.Broadcast <| sprintf "particpiant %d add to room %d" participantId roomNum)
        (connections, rooms)                        

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

                            | ControlInterfaceMsg.Shutdown -> shutdown connections

                            | ControlInterfaceMsg.GetRoom roomNum -> inbox |> getRoom connections rooms roomNum

                            | ControlInterfaceMsg.Advance roomNum -> 
                                updateRoomState agentRepo connections rooms roomNum (sprintf "room %d advnced" roomNum) advance

                            | ControlInterfaceMsg.Reverse roomNum ->
                                updateRoomState agentRepo connections rooms roomNum (sprintf "room %d reversed" roomNum) reverse                               

                            | ControlInterfaceMsg.AddParticipant (roomNum, participantId) -> 
                                addParticipant agentRepo connections rooms roomNum participantId 

                    return! loop conn newRooms
               }
            loop [] defaultRoomStates
        )


