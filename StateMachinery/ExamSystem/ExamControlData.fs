namespace ExamSystem

open System
open System.Net.Sockets

[<AutoOpen>]
module ExamControlData =
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

    and ReplyType = 
        | RoomReply

    type ControlRequest = ControlInterfaceMsg * AsyncReplyChannel<ReplyType option>

    type GlobalMsg = 
        | Broadcast of string


    type AgentRepo = {
        Global: Agent<GlobalMsg>;
        Rooms: (RoomId * Agent<RoomConnMsg>) list;
        Control: Agent<ControlInterfaceMsg>
    }

    
    type FailedClients = TcpClient list
    type SucceedClients = FailedClients
    
