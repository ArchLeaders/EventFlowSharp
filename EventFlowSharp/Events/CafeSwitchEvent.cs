namespace EventFlowSharp.Events;

public sealed class CafeSwitchEvent : CafeEvent
{
    public required CafeQuery Query { get; set; }

    public List<CafeSwitchEventCase> Cases { get; set; } = [];
    
    public CafeUserData Parameters { get; set; } = new();
}