namespace EventFlowSharp.ORE;

[Swappable]
public partial struct BinaryBlockHeader
{
    public uint Magic;
    public int NextBlockOffset;
}