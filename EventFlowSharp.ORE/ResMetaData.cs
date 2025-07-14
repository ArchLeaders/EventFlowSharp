using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EventFlowSharp.ORE;

[Swappable]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public partial struct ResMetaData
{
    public DataType Type;
    public ushort ItemCount;
    public BinaryPointer<ResDic> Dictionary;
    public ResMetaDataValue Value;
    
    /// <summary>
    /// Only usable if <see cref="Type"/> == <see cref="DataType.Container"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expectedType"></param>
    /// <returns></returns>
    public unsafe ref ResMetaData Get(StringView key, DataType expectedType)
    {
        int index = Dictionary.Get().FindIndex(key);
        if (index == -1) {
            return ref Unsafe.NullRef<ResMetaData>();
        }

        fixed (BinaryPointer<ResMetaData>* ptr = &Value.Container) {
            ref ResMetaData meta = ref (ptr + index)->Get();
            if (meta.Type != expectedType) {
                return ref Unsafe.NullRef<ResMetaData>();
            }

            return ref meta;
        }
    }
    
    public enum DataType : byte
    {
        Argument,
        Container,
        Int,
        Bool,
        Float,
        String,
        WString,
        IntArray,
        BoolArray,
        FloatArray,
        StringArray,
        WStringArray,
        ActorIdentifier
    }
}

[Swappable]
[StructLayout(LayoutKind.Explicit)]
public partial struct ResMetaDataValue
{
    [FieldOffset(0)]
    private ulong _container;

    public unsafe ref BinaryPointer<ResMetaData> Container {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            fixed (ulong* ptr = &_container) {
                return ref Unsafe.AsRef<BinaryPointer<ResMetaData>>(ptr);
            }
        }
    }
        
    /// <summary>
    /// Also used for booleans. Anything that is != 0 is treated as true.
    /// </summary>
    [FieldOffset(0)]
    public int Int;
        
    [FieldOffset(0)]
    public float Float;
        
    [FieldOffset(0)]
    public BinaryPointer<BinaryString<byte>> String;
        
    [FieldOffset(0)]
    public BinaryPointer<BinaryString<char>> WideString;
        
    [FieldOffset(0)]
    public ResMetaDataActorIdentifier Actor;
}

[Swappable]
public partial struct ResMetaDataActorIdentifier
{
    public BinaryPointer<BinaryString<byte>> Name;
    public BinaryPointer<BinaryString<byte>> SubName;
} 