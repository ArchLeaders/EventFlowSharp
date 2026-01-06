namespace EventFlowSharp;

public class CafeAction
{
    public required string Action { get; set; }
    
    public required CafeActor Actor { get; set; }
}