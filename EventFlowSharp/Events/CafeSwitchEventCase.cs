namespace EventFlowSharp.Events;

public class CafeSwitchEventCase
{
    public required int Value { get; set; }
    
    public required CafeEvent Event { get; set; }
}