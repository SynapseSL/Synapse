namespace Synapse3.SynapseModule.Map.Rooms;

public enum RoomType : uint
{
    //Others
    None,
    Pocket,
    Surface,
    
    //LCZ
    LczStraight,
    LczAirlock,
    LczCurve,
    LczTCross,
    LczCrossing,
    
    ClassDSpawn,
    LczOffice,
    Toilets,
    LightArmory,
    Plants,
    LczCheckpointA,
    LczCheckpointB,
    
    Scp173,
    Scp330,
    Scp372,
    Scp914,
    
    //HCZ
    HczStraight,
    HczCurve,
    HczTCross,
    HczCrossing,
    
    HeavyCheckpoint,
    HeavyArmory,
    Nuke,
    Servers,
    TestingRoom,
    Tesla,
    MicroHid,
    HczElevatorA,
    HczElevatorB,

    Scp049,
    Scp079,
    Scp096,
    Scp106,
    Scp939,
    
    //EZ
    EzStraight,
    EzCurve,
    EzTCross,
    EzCrossing,
    EzDeadEnd,
    CollapsedTunnel,
    
    EntranceCheckpoint,
    Intercom,
    EzOffice,
    EzOfficeDownstairs,
    EzOfficeUpstairs,
    ConferenceRoomCorridor,
    Shelter,
    GateA,
    GateB
}