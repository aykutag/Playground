module RoomConnections

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Text
open System.Threading
open System.Runtime.Serialization
open ExamSystem

type Agent<'T> = MailboxProcessor<'T>

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
    | GetRoom of int * AsyncReplyChannel<Room>
    | Advance of int

type ConnectionType = 
    | Control
    | Room of int
    | Unknown of string

type GlobalMsg = 
    | Broadcast of string

type RoomId = int

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

/// Listens on a tcp client and returns a seq<byte[]> of all
/// found data
let rec listenOnClient (client:TcpClient) = 
    seq {            
        let stream = client.GetStream()

        let bytes = Array.create 4096 (byte 0)
        let read = stream.Read(bytes, 0, 4096)
        if read > 0 then 
            yield bytes.[0..read - 1]
        yield! listenOnClient client
    }    

/// Listens on a tcp client and returns a seq<byte[]> of all
/// found data
let rec readNBytes n (client:TcpClient) = 
    seq {            
        if n > 0 then 
            let stream = client.GetStream()

            let bytes = Array.create n (byte 0)
            let read = stream.Read(bytes, 0, n)
            if read > 0 then 
                yield bytes.[0..read - 1]
            yield! readNBytes (n - read) client
    } 

/// Sits on the client's socket stream and broadcasts its messages
/// to everyone else in the room
let rec processClientData (roomConn:Agent<RoomConnMsg>) client = 
    async{
        try
            for bytesRead in listenOnClient client do
                roomConn.Post (BroadcastExcept (client, System.Text.ASCIIEncoding.UTF8.GetString(bytesRead)))
        with
            | exn -> roomConn.Post (RoomConnMsg.Disconnect client)
    }

let rec listenForControlCommands (controlConn:Agent<ControlInterfaceMsg>) client = 
    async {
        try
            for bytesRead in listenOnClient client do
                ()
        with
            | exn -> controlConn.Post (ControlInterfaceMsg.Disconnect client)
    }

let removeTcp connections client = List.filter ((<>) client) connections

let broadcastStr connections msg = msg |> strToBytes |> broadcast connections 

/// An agent for a particular room
let rec roomConnection agentRepo roomId = 
    new Agent<RoomConnMsg>(
        fun inbox ->        
        let rec loop connections =
            async {   
                let! request = inbox.Receive()               
                let newConnections = 
                    match request with
                        | RoomConnMsg.Connect client    ->
                            agentRepo().Global |> post (GlobalMsg.Broadcast <| sprintf "Client connected to room %d" roomId)
                            processClientData inbox client |> Async.Start
                            client::connections

                        | RoomConnMsg.Disconnect client -> 
                            client.Close()
                            connections |> removeTcp <| client

                        | RoomConnMsg.Broadcast msg -> msg |> broadcastStr connections |> snd

                        | RoomConnMsg.BroadcastExcept (client, msg) ->                             
                            let (failed, success) = msg |> broadcastStr ((List.filter ((<>) client)) connections) 
                            client::success

                        | RoomConnMsg.Shutdown -> 
                            "Shutting down" |> strToBytes |> broadcast connections |> ignore
                            List.iter closeClient connections
                            []

                if newConnections <> connections then
                    printfn "total clients %d" <| List.length newConnections

                return! loop newConnections
            }
        loop [])

/// The control interface agent.  Handles room state requests
and controlInterface roomControllers (defaultRoomStates:Room list) =     
    Agent<ControlInterfaceMsg>.Start(fun inbox ->
        let rec loop connections roomStates = 
           async {
                let! msg = inbox.Receive()

                let (conn, newStates) = 
                    match msg with 
                        | ControlInterfaceMsg.Connect client ->                            
                            listenForControlCommands inbox client |> ignore

                            (client::connections, roomStates)

                        | ControlInterfaceMsg.Disconnect client -> (connections |> removeTcp <| client, roomStates)

                        | ControlInterfaceMsg.Broadcast str -> (str |> broadcastStr connections |> snd, roomStates)

                        | ControlInterfaceMsg.Shutdown -> 
                            "Shutting down" |> strToBytes |> broadcast connections |> ignore
                            List.iter closeClient connections
                            ([], [])

                        | ControlInterfaceMsg.GetRoom (roomNum, channel) ->
                            let room = List.find (fun (r:Room) -> r.RoomId = roomNum) roomStates
                            channel.Reply room
                            (connections, roomStates)

                        | ControlInterfaceMsg.Advance roomNum ->
                            let room = List.find (fun (r:Room) -> r.RoomId = roomNum) roomStates
                            let newStates = advance room.States                           
                            inbox.Post (ControlInterfaceMsg.Broadcast <| sprintf "room %d advnced" roomNum)
                            (connections, { room with States = newStates} :: roomStates)

                return! loop conn newStates
           }
        loop [] defaultRoomStates
    )

/// The global agent that can rebroadcast to all room controllers
/// as well as the control interfaces (basically anyone connected)
and globalAgent agentRepo = 
    Agent<GlobalMsg>.Start(fun inbox ->
        let rec loop () = 
            async{
                let! msg = inbox.Receive()
                match msg with
                    | GlobalMsg.Broadcast str -> 
                        agentRepo().Control |> post (ControlInterfaceMsg.Broadcast str)
                        agentRepo().Rooms   |> List.map snd |> List.iter (post (RoomConnMsg.Broadcast str)) 

                return! loop()
            }
        loop()
    )

let findControllerForRoom roomId roomControllers = List.tryFind (fst >> (=) roomId) roomControllers

let (|IsControl|_|) str = if str = "control//" then Some(IsControl) else None
let (|IsRoom|_|) (str:string) = 
    try
        if str.StartsWith("room/") then 
            str.Replace("room/","").Trim() |> System.Convert.ToInt32 |> Some
        else None
    with 
        | exn -> None

/// Checks the first 5 bytes of the socket sequence 
/// to see if this client should be the control
let connectionType client =     
    client  |> readNBytes 9
            |> Seq.concat             
            |> Seq.toArray 
            |> System.Text.ASCIIEncoding.UTF8.GetString
            |> function
                | IsControl -> ConnectionType.Control
                | IsRoom num -> ConnectionType.Room num 
                | str -> ConnectionType.Unknown str

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
                        agentRepo().Control |> post (ControlInterfaceMsg.Connect client)
                        agentRepo().Control |> post (ControlInterfaceMsg.Broadcast "control connnected")                    
                    | Room roomId ->
                        agentRepo().Rooms
                            |> findControllerForRoom roomId
                            |> function
                                | Some room -> room |> snd |> post (RoomConnMsg.Connect client)
                                | None -> agentRepo().Global |> post (GlobalMsg.Broadcast <| sprintf "Unknown room requested!")
                                          client.Close()
                    | Unknown str ->
                        client.Close()
                        agentRepo().Global |> post (GlobalMsg.Broadcast <| sprintf "Unknown connection found, closing: %s" str)

                   
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
                agentRepo().Rooms   |> List.iter (snd >> post RoomConnMsg.Shutdown) 
                agentRepo().Control |> post ControlInterfaceMsg.Shutdown
    }
 
/// Sets up a timer to broadcast the current time to the room agent
let timer interval (ctrl : Agent<RoomConnMsg>)  = 
    let cts = new CancellationTokenSource()
    let token = cts.Token
    let workflow = 
        async {        
            while not <| token.IsCancellationRequested do
                do! Async.Sleep interval
                ctrl.Post(RoomConnMsg.Broadcast <| DateTime.Now.ToString() + Environment.NewLine)
        }

    let dispose = 
        { new IDisposable 
            with member x.Dispose() = cts.Cancel()
        }

    (workflow, dispose)    