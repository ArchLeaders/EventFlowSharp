using System.Diagnostics;

namespace EventFlowSharp;

[DebuggerDisplay("{Action}")]
public class CafeAction
{
    public required string Action { get; set; }
    
    public required CafeActor Actor { get; set; }

    public override string ToString() => Action;
}