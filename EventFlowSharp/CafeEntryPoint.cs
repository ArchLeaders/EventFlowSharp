using System.Diagnostics.CodeAnalysis;

namespace EventFlowSharp;

public class CafeEntryPoint
{
    public required string Name { get; set; }

    public Dictionary<string, CafeVariableDef> VariableDefs { get; set; } = new();

    public CafeEvent? Event { get; set; }

    public CafeEntryPoint()
    {
    }

    [SetsRequiredMembers]
    public CafeEntryPoint(string name)
    {
        Name = name;
    }
}