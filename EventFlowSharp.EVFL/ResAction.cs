using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

[Swappable]
public partial struct ResAction
{
    public BinaryPointer<BinaryString<byte>> Name;
}