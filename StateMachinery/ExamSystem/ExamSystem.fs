module ExamSystem

open System

type Evaluation = { UserId : int; ChecklistId :int }

type StateRequest = 
    | StudentExam of Evaluation list
    | FacultyExam of Evaluation list
    | Recording

type State = 
    | DoorNotes of StateRequest list
    | Encounter of StateRequest list
    | PostEncounter of StateRequest list
    | Followup of StateRequest list
    | Empty

type StateMessage = 
    | Forward
    | Reverse

type RoomStates = { 
                    ConsumedStates: State list;  
                    PendingStates: State list;
                    CurrentState: State
                  }

type Room = { RoomId: int; States: RoomStates }

let advance states =
    match states.PendingStates with 
        | h::t -> 
            { 
                ConsumedStates = states.CurrentState::states.ConsumedStates; 
                PendingStates = t;
                CurrentState = h
            }       
        | [] -> { states with CurrentState = State.Empty }            

let reverse states = 
    match states.ConsumedStates with 
       | h::t -> 
            { 
                ConsumedStates = t; 
                PendingStates = states.CurrentState::states.PendingStates;
                CurrentState = h
            }        
        | [] -> states
