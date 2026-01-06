namespace EventFlowSharp.Events;

public sealed class CafeJoinEvent : CafeEvent, ILinearEvent
{
    public CafeEvent? NextEvent { get; set; }
}