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
        
    let private queryRoom roomNum client agentRepo  = 
        async{
            let! room = agentRepo.Control |> postAndAsyncReply (fun chan -> ControlInterfaceMsg.GetRoom(roomNum, chan)) 
            
            agentRepo.Control |> post (ControlInterfaceMsg.BroadcastTo (client, sprintf "%A" room))
        }

    let rec private listenForControlCommands (agentRepo:AgentRepo) client =         
        async {
            let postFlip mailbox msg = post msg mailbox
            let postToControl = postFlip agentRepo.Control

            do! Async.SwitchToNewThread() 
            try
                for message in client |> packets do                                       
                    match message with                         
                        | AdvanceCmd roomNum        -> postToControl <| ControlInterfaceMsg.Advance roomNum
                        | ReverseCmd roomNum        -> postToControl <| ControlInterfaceMsg.Reverse roomNum                                 
                        | StartPreview roomNum      -> postToControl <| ControlInterfaceMsg.StartPreview roomNum
                        | StartStreaming roomNum    -> postToControl <| ControlInterfaceMsg.StartStreaming roomNum
                        | Record roomNum            -> postToControl <| ControlInterfaceMsg.Record roomNum
                        | ResetRoom roomNum         -> postToControl <| ControlInterfaceMsg.Reset roomNum
                        | QueryRoom roomNum         -> do! agentRepo |> queryRoom roomNum client
                        | _                         -> postToControl <| ControlInterfaceMsg.Broadcast ("Unknown control sequence " + message)                 
            with
                | exn -> postToControl (ControlInterfaceMsg.Disconnect client)
        }

    let private shutdown connections =         
        "Shutting down" |> strToBytes |> broadcast connections |> ignore
        List.iter closeClient connections                       

    let private getRoom data roomNum = List.find (fun (r:Room) -> r.RoomId = roomNum) data.RoomStates
    
    let private updateRoomList rooms room = room::(List.filter ((<>) room) rooms)

    let private updateRoomsWithState state room roomCollection = { room with States = state} |> updateRoomList roomCollection 

    let private broadcastRoomState data room = 
        data.Inbox |> post (ControlInterfaceMsg.Broadcast (roomString room))

    let private updateRoomState data roomNum description step = 
        let (room, newStates) = (roomNum, data.RoomStates) ||> findRoomAndApply step                    
                                
        postToRoom data.AgentRepo roomNum (RoomConnMsg.Broadcast <| description)

        data.RoomStates |> updateRoomsWithState newStates room

    let private addParticipant data roomNum participantId =         
        postToRoom data.AgentRepo roomNum (RoomConnMsg.Broadcast <| sprintf "particpiant %d add to room %d" participantId roomNum)
                            
    let private postDisconnect (inbox:Agent<ControlInterfaceMsg>) client = 
        inbox |> post (ControlInterfaceMsg.Disconnect client)

    let private clientConnected data client = 
        monitor isConnected (postDisconnect data.Inbox) client |> Async.Start

        (data.AgentRepo, client) ||> listenForControlCommands |> Async.Start

    let private startRecording data roomNum = { (roomNum |> getRoom data) with RecorderStatus = Recording }
    let private reset data roomNum          = { (roomNum |> getRoom data) with RecorderStatus = NoStatus }
    let private startPreview data roomNum   = { (roomNum |> getRoom data) with RecorderStatus = Preview }
    let private startStreaming data roomNum = { (roomNum |> getRoom data) with RecorderStatus = Streaming }

    let private processControlMessageState data = function        
        | ControlInterfaceMsg.Connect client ->     
            
            clientConnected data client

            { data with Connections = client::data.Connections }

        | ControlInterfaceMsg.Disconnect client -> 
            { data with Connections = data.Connections |> removeTcp <| client }

        | ControlInterfaceMsg.Broadcast str -> 
            { data with Connections = str |> broadcastStr data.Connections |> snd }

        | ControlInterfaceMsg.BroadcastTo (client, str) ->
            str |> broadcastStr [client] |> ignore
            data

        | ControlInterfaceMsg.Shutdown -> 
            shutdown data.Connections

            { data with Connections =  [] }

        | ControlInterfaceMsg.GetRoom (roomNum, reply) -> 
            reply.Reply(findRoom roomNum data.RoomStates)
            data        

        | ControlInterfaceMsg.Advance roomNum -> 
            { data with RoomStates = advance |> updateRoomState data roomNum (sprintf "room %d advnced" roomNum)  }

        | ControlInterfaceMsg.Reverse roomNum ->
            { data with RoomStates = reverse |> updateRoomState data roomNum (sprintf "room %d reversed" roomNum) }                                

        | ControlInterfaceMsg.AddParticipant (roomNum, participantId) -> 
            addParticipant data roomNum participantId 
            data

        | ControlInterfaceMsg.Record roomNum -> 
            { data with RoomStates = startRecording data roomNum |> updateRoomList data.RoomStates }

        | ControlInterfaceMsg.Reset roomNum -> 
            { data with RoomStates = reset data roomNum |> updateRoomList data.RoomStates }
            
        | ControlInterfaceMsg.StartPreview roomNum -> 
            { data with RoomStates = startPreview data roomNum |> updateRoomList data.RoomStates }                      

        | ControlInterfaceMsg.StartStreaming roomNum -> 
            { data with RoomStates = startStreaming data roomNum |> updateRoomList data.RoomStates }
        
    /// The control interface agent.  Handles room state requests
    let controlInterface agentRepo (defaultRoomStates:Room list) =     
        Agent<ControlInterfaceMsg>.Start(fun inbox ->

            let rec loop state = 
               async {
                    let! msg = inbox.Receive()
                    
                    let newState = processControlMessageState state msg

                    return! loop newState
               }
            loop { 
                AgentRepo = agentRepo()
                RoomStates = defaultRoomStates
                Connections = []
                Inbox = inbox
            }
        )


