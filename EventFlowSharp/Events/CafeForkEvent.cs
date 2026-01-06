namespace EventFlowSharp.Events;

public sealed class CafeForkEvent : CafeEvent
{
    public List<CafeEvent> Branches { get; set; } = [];
    
    public required CafeJoinEvent JoinEvent { get; set; }
}