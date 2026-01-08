using System.Diagnostics;

namespace EventFlowSharp;

[DebuggerDisplay("{Query}")]
public class CafeQuery
{
    public required string Query { get; set; }
    
    public required CafeActor Actor { get; set; }
    
    public override string ToString() => Query;
}