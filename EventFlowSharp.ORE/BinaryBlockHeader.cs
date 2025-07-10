namespace EventFlowSharp.ORE;

[Reversible]
public partial struct BinaryBlockHeader
{
    public uint Magic;
    public int NextBlockOffset;
}