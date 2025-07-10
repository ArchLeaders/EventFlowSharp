using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

[Reversible]
public partial struct ResQuery
{
    public BinaryPointer<BinaryString<byte>> Name;
}