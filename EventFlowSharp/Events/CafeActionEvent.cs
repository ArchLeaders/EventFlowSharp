namespace EventFlowSharp.Events;

public sealed class CafeActionEvent : CafeEvent, ILinearEvent
{
    public required CafeAction Action { get; set; }

    public CafeEvent? NextEvent { get; set; }
    
    public CafeUserData Parameters { get; set; } = new();
}