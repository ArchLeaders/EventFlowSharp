using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

[Reversible]
public partial struct ResAction
{
    public BinaryPointer<BinaryString<byte>> Name;
}