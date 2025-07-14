using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

[Swappable]
public partial struct ResQuery
{
    public BinaryPointer<BinaryString<byte>> Name;
}