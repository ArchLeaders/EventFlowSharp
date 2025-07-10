using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

[Reversable]
public partial struct ResAction
{
    public BinaryPointer<BinaryString<byte>> Name;
}