namespace EventFlowSharp.ORE;

[Swappable]
public partial struct BinaryBlockHeader
{
    [NeverSwap]
    public uint Magic;
    public int NextBlockOffset;

    public unsafe BinaryBlockHeader* GetNextBlock()
    {
        if (NextBlockOffset == 0) {
            return null;
        }

        fixed (BinaryBlockHeader* ptr = &this) {
            return (BinaryBlockHeader*)((byte*)ptr + NextBlockOffset);
        }
    }

    public unsafe BinaryBlockHeader* FindNextBlock(uint magic)
    {
        BinaryBlockHeader* block;
        fixed (BinaryBlockHeader* ptr = &this) {
            block = ptr;
        }
        
        do {
            block = block->GetNextBlock();
        } while (block is not null && block->Magic != magic);

        return block;
    }
}