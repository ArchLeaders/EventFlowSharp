using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

[Swappable]
public partial struct ResEntryPoint
{
    public BinaryPointer<ushort> SubFlowEventIndices;
    public BinaryPointer<ResDic> VariableDefNames;
    public BinaryPointer<ResVariableDef> VariableDefs;
    public ushort SubFlowEventIndicesCount;
    public ushort VariableDefsCount;
    public ushort MainEventIndex;
}