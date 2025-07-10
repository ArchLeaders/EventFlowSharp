using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

[Reversable]
public partial struct ResQuery
{
    public BinaryPointer<BinaryString<byte>> Name;
}