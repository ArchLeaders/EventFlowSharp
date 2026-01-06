using System.Diagnostics.CodeAnalysis;

namespace EventFlowSharp;

public class CafeFlowchart
{
    public required string Name { get; set; }
    
    public List<CafeActor> Actors { get; set; } = [];
    
    public List<CafeEntryPoint> EntryPoints { get; set; } = [];

    public CafeFlowchart()
    {
    }

    [SetsRequiredMembers]
    public CafeFlowchart(string name)
    {
        Name = name;
    }
}