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
    open ExamSystem.ExamControlData
    open ExamSystem.StateManager
    open ExamSystem.Utils
    open ExamSystem.NetworkUtils
    open ExamSystem.CommunicationProtocol


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

    /// An agent for a particular room
    let roomConnection agentRepo roomId = 
        new Agent<RoomConnMsg>(
            fun inbox ->        
        
            let postDisconnect client = inbox.Post (RoomConnMsg.Disconnect client)

            let rec loop connections =
                async {   
                    //printfn "Executing room loop %d" roomId
                    let! request = inbox.Receive() 
                    let originalConnectionSize = List.length connections                              
                    let newConnections = 
                        match request with
                            | RoomConnMsg.Connect client    ->
                                agentRepo().Global |> post (GlobalMsg.Broadcast <| sprintf "Client connected to room %d" roomId)
                            
                                monitor isConnected postDisconnect client |> Async.Start

                                (inbox, client) |> applyTupleTo [processClientData] 
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
    
  