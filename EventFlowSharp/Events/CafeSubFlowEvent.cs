namespace EventFlowSharp.Events;

public sealed class CafeSubFlowEvent : CafeEvent, ILinearEvent
{
    public required string Flowchart { get; set; }
    
    public required string EntryPoint { get; set; }
    
    public CafeEvent? NextEvent { get; set; }

    public CafeUserData Parameters { get; set; } = new();
}