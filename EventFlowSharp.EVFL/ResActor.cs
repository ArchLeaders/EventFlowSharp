using System.Runtime.CompilerServices;
using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

[Reversable]
public partial struct ResActor
{
    public BinaryPointer<BinaryString<byte>> Name;
    public BinaryPointer<BinaryString<byte>> SecondaryName;
    public BinaryPointer<BinaryString<byte>> ArgumentName;
    public BinaryPointer<ResAction> Actions;
    public BinaryPointer<ResQuery> Queries;
    public BinaryPointer<ResMetaData> Params;
    public ushort ActionsCount;
    public ushort QueriesCount;
    
    /// <summary>
    /// Entry point index for associated entry point (0xffff if none)
    /// </summary>
    public ushort EntryPointIndex;
    
    // TODO: Cut number?
    // This is set to 1 for flowcharts but other
    // values have been seen for timeline actors
    public byte CutNumber;
    
    public bool HasArgumentName()
    {
        return !Unsafe.IsNullRef(ref ArgumentName.Get());
    }
}