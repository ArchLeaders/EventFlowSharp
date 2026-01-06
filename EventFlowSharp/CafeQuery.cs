namespace EventFlowSharp;

public class CafeQuery
{
    public required string Query { get; set; }
    
    public required CafeActor Actor { get; set; }
}