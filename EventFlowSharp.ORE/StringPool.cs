using Entish;

namespace EventFlowSharp.ORE;

public struct StringPool : ISwappable<StringPool>
{
    public const uint Magic = 0x20525453;
    
    public BinaryBlockHeader Header;
    private uint _reserved8;
    private uint _reservedC;
    public int Length;
    private BinaryString<byte> _dummyString;

    public unsafe BinaryString<byte>* GetFirstString()
    {
        return _dummyString.NextString();
    }
    
    public static unsafe void Swap(StringPool* value)
    {
        BinaryBlockHeader.Swap(&value->Header);
        EndianUtils.Swap(&value->Length);

        BinaryString<byte>* str = value->GetFirstString();
        for (int i = 0; i < value->Length; i++) {
            EndianUtils.Swap(&str->Length);
            str = str->NextString();
        }
    }
}