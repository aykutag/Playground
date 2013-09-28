namespace ExamSystem 

module RoomConnections = 

    open System
    open System.IO
    open System.Net
    open System.Net.Sockets
    open System.Text
    open System.Threading
    open System.Runtime.Serialization
    open ExamSystem
    open ExamSystem.StateManager
    open ExamSystem.Utils
    open ExamSystem.CommunicationProtocol

    type Agent<'T> = MailboxProcessor<'T>

    type RoomId = int
    type ParticipantId = int

    type RoomConnMsg = 
        | Connect of TcpClient
        | Disconnect of TcpClient
        | Broadcast of string
        | BroadcastExcept of TcpClient * String    
        | Shutdown

    type ControlInterfaceMsg = 
        | Connect of TcpClient
        | Disconnect of TcpClient
        | Broadcast of string     
        | Shutdown
        | GetRoom of RoomId
        | Advance of RoomId
        | Reverse of RoomId
        | AddParticipant of (RoomId * ParticipantId)

    type GlobalMsg = 
        | Broadcast of string



    type AgentRepo = {
        Global: Agent<GlobalMsg>;
        Rooms: (RoomId * Agent<RoomConnMsg>) list;
        Control: Agent<ControlInterfaceMsg>
    }

    let post msg (mailBox:Agent<_>) = mailBox.Post msg
    let start  (mailBox:Agent<_>) = mailBox.Start()
    let startRoom mailbox = snd >> start <| mailbox

    let private closeClient (client:TcpClient) = client.Close()

    let private writeToSocket (tcp:TcpClient) msg =  
        try          
            let stream = tcp.GetStream()

            stream.Write (msg, 0, Array.length msg)

            true
        with
            | exn -> 
                tcp |> closeClient
                false

    let private strToBytes (str:string) = System.Text.ASCIIEncoding.ASCII.GetBytes str

    type FailedClients = TcpClient list
    type SucceedClients = FailedClients

    let private broadcast clients msg : (FailedClients * SucceedClients) = 
        List.fold(fun (failed, succeeded) client ->                     
                        match writeToSocket client msg with
                            | true -> (failed, client::succeeded)
                            | false -> (client::failed, succeeded)) ([], []) clients


    /// Sits on the client's socket stream and broadcasts its messages
    /// to everyone else in the room
    let rec processClientData (roomConn:Agent<RoomConnMsg>) client = 
        async{
            try
                for message in client |> packets do
                    roomConn.Post (BroadcastExcept (client, message))
            with
                | exn -> roomConn.Post (RoomConnMsg.Disconnect client)
        }

    let rec monitorRoomConnectivity (roomConn:Agent<RoomConnMsg>) client  = 
        async {
            let! isConnected = isConnected client
            if not isConnected then
                roomConn |> post (RoomConnMsg.Disconnect client)
            else 
                return! monitorRoomConnectivity roomConn client
        }

    let rec monitorCntrlConnectivity (cntrl:Agent<ControlInterfaceMsg>) client = 
        async {
            let! isConnected = isConnected client
            if not isConnected then
                cntrl |> post (ControlInterfaceMsg.Disconnect client)
            else 
                return! monitorCntrlConnectivity cntrl client
        }

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

    let removeTcp connections client = List.filter ((<>) client) connections

    let broadcastStr connections msg = msg + Environment.NewLine |> strToBytes |> broadcast connections 

    let postToRoom agentRepo roomId msg = 
        List.tryFind (fst >> (=) roomId) agentRepo.Rooms
            |> Option.bindDo (snd >> post msg)
    

    /// An agent for a particular room
    let rec roomConnection agentRepo roomId = 
        new Agent<RoomConnMsg>(
            fun inbox ->        
        
            let rec loop connections =
                async {   
                    //printfn "Executing room loop %d" roomId
                    let! request = inbox.Receive() 
                    let originalConnectionSize = List.length connections                              
                    let newConnections = 
                        match request with
                            | RoomConnMsg.Connect client    ->
                                agentRepo().Global |> post (GlobalMsg.Broadcast <| sprintf "Client connected to room %d" roomId)
                            
                                (inbox, client) |> applyTupleTo [processClientData; monitorRoomConnectivity] 
                                                |> List.iter Async.Start

                                client::connections

                            | RoomConnMsg.Disconnect client -> 
                                client.Close()
                                
                                printfn "Client disconnected from room %d" roomId

                                connections |> removeTcp <| client

                            | RoomConnMsg.Broadcast msg -> msg |> broadcastStr connections |> snd

                            | RoomConnMsg.BroadcastExcept (client, msg) ->                             
                                let successFull = msg |> broadcastStr ((List.filter ((<>) client)) connections) |> snd 
                                client::successFull

                            | RoomConnMsg.Shutdown -> 
                                "Shutting down" |> strToBytes |> broadcast connections |> ignore
                                List.iter closeClient connections
                                []

                    if originalConnectionSize <> List.length newConnections then
                        printfn "total clients %d" <| List.length newConnections

                    return! loop newConnections
                }
            loop [])

    /// The control interface agent.  Handles room state requests
    and controlInterface agentRepo (defaultRoomStates:Room list) =     
        Agent<ControlInterfaceMsg>.Start(fun inbox ->
            let rec loop connections rooms = 
               async {
                    let! msg = inbox.Receive()

                    let (conn, newRooms) = 
                        match msg with 
                            | ControlInterfaceMsg.Connect client ->     
                                (inbox, client) |> applyTupleTo [listenForControlCommands; monitorCntrlConnectivity] 
                                                |> List.iter Async.Start

                                (client::connections, rooms)

                            | ControlInterfaceMsg.Disconnect client -> (connections |> removeTcp <| client, rooms)

                            | ControlInterfaceMsg.Broadcast str -> (str |> broadcastStr connections |> snd, rooms)

                            | ControlInterfaceMsg.Shutdown -> 
                                "Shutting down" |> strToBytes |> broadcast connections |> ignore
                                List.iter closeClient connections
                                ([], [])

                            | ControlInterfaceMsg.GetRoom (roomNum) ->
                                let room = List.find (fun (r:Room) -> r.RoomId = roomNum) rooms
                                
                                inbox.Post (ControlInterfaceMsg.Broadcast (roomString room))

                                (connections, rooms)

                            | ControlInterfaceMsg.Advance roomNum ->
                                let (room, newStates) = rooms |> applyToRoomStates advance roomNum                    
                                postToRoom (agentRepo()) roomNum (RoomConnMsg.Broadcast <| sprintf "room %d advnced" roomNum)
                                (connections, { room with States = newStates} :: List.filter ((<>) room) rooms)

                            | ControlInterfaceMsg.Reverse roomNum ->
                                let (room, newStates) = rooms |> applyToRoomStates reverse roomNum                    
                                postToRoom (agentRepo()) roomNum (RoomConnMsg.Broadcast <| sprintf "room %d reversed" roomNum)
                                (connections, { room with States = newStates} :: List.filter ((<>) room) rooms)

                            | ControlInterfaceMsg.AddParticipant (roomNum, participantId) ->
                                postToRoom (agentRepo()) roomNum (RoomConnMsg.Broadcast <| sprintf "particpiant %d add to room %d" participantId roomNum)
                                (connections, rooms)                        

                    return! loop conn newRooms
               }
            loop [] defaultRoomStates
        )

    /// The global agent that can rebroadcast to all room controllers
    /// as well as the control interfaces (basically anyone connected)
    and globalAgent agentRepo = 
        Agent<GlobalMsg>.Start(fun inbox ->

            let applyToRooms msg = agentRepo().Rooms   |> List.map snd |> List.iter msg

            let rec loop () = 
                async{
                    let! msg = inbox.Receive()
                    match msg with
                        | GlobalMsg.Broadcast str -> 
                            agentRepo().Control |> post (ControlInterfaceMsg.Broadcast str)
                            applyToRooms (post (RoomConnMsg.Broadcast str)) 

                    return! loop()
                }
            loop()
        )

    let findControllerForRoom roomId roomControllers = List.tryFind (fst >> (=) roomId) roomControllers

    /// Accepts sockets and hands off the connected client
    /// To the right agent based on their handshake
    let listenForConnections agentRepo = 
        let listener = new TcpListener(IPAddress.Any, 81)
        let cts = new CancellationTokenSource()
        let token = cts.Token
     
        let main = async {
            try
                listener.Start(10)
                while not cts.IsCancellationRequested do
                    let! client = Async.FromBeginEnd(listener.BeginAcceptTcpClient, listener.EndAcceptTcpClient)
                    printfn "Got client %s" <| client.Client.RemoteEndPoint.ToString()
                
                    match connectionType client with
                        | Control -> 
                            agentRepo.Control |> post (ControlInterfaceMsg.Connect client)
                            agentRepo.Control |> post (ControlInterfaceMsg.Broadcast "control connnected")    
                                        
                        | Room roomId ->
                            agentRepo.Rooms
                                |> findControllerForRoom roomId
                                |> function
                                    | Some room -> room |> snd |> post (RoomConnMsg.Connect client)
                                    | None -> agentRepo.Global |> post (GlobalMsg.Broadcast <| sprintf "Unknown room requested!")
                                              client.Close()

                        | Unknown str ->
                            client.Close()
                            agentRepo.Global |> post (GlobalMsg.Broadcast <| sprintf "Unknown connection found, closing: %s" str)

                   
            finally
                printfn "Listener stopping"
                listener.Stop()        
        }
 
        Async.Start(main, token)
 
        { 
            new IDisposable 
            with member x.Dispose() = 
                    cts.Cancel() |> ignore
                    cts.Dispose()
                    agentRepo.Rooms   |> List.iter (snd >> post RoomConnMsg.Shutdown) 
                    agentRepo.Control |> post ControlInterfaceMsg.Shutdown
        }
 
    /// Sets up a timer to broadcast the current time to the room agent
    let timer interval (ctrl : AgentRepo)  = 
        let cts = new CancellationTokenSource()
        let token = cts.Token
        let workflow = 
            async {        
                while not <| token.IsCancellationRequested do
                    do! Async.Sleep interval
                    //ctrl.Global |> post GlobalMsg.Ping
            }

        let dispose = 
            { new IDisposable 
                with member x.Dispose() = cts.Cancel()
            }

        (workflow, dispose)    