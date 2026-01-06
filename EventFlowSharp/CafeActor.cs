namespace EventFlowSharp;

public sealed class CafeActor
{
    public required string Name { get; set; }
    
    public string? SecondaryName { get; set; }
    
    public string? ArgumentName { get; set; }

    public CafeEntryPoint? EntryPoint { get; set; }

    public int CutNumber { get; set; }

    public List<CafeAction> ActionList { get; set; } = [];

    public List<CafeQuery> QueryList { get; set; } = [];

    public CafeUserData Parameters { get; set; } = new();
}