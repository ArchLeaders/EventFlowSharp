namespace EventFlowSharp.ORE;

[Reversable]
public partial struct BinaryBlockHeader
{
    public uint Magic;
    public int NextBlockOffset;
}