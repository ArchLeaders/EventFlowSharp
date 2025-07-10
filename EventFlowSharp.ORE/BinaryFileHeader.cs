using System.Runtime.CompilerServices;
using Revrs;
using Revrs.Primitives;

namespace EventFlowSharp.ORE;

[Flags]
public enum RelocationFlags : ushort
{
    IsRelocated = 1 << 0
}

[Reversible]
public partial struct BinaryFileHeader
{
    [DoNotReverse]
    public ulong Magic;

    [DoNotReverse]
    public BinaryFileVersion Version;

    public Endianness ByteOrder;
    public byte Alignment;
    public byte Padding1;
    public int FileNameOffset;
    public RelocationFlags RelocationFlags;
    public ushort FirstBlockOffset;
    public int RelocationTableOffset;
    public int FileSize;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRelocated() => RelocationFlags.HasFlag(RelocationFlags.IsRelocated);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetRelocated() => RelocationFlags |= RelocationFlags.IsRelocated;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetUnRelocated() => RelocationFlags &= ~RelocationFlags.IsRelocated;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe StringView GetFileName()
    {
        fixed (void* ptr = &this) {
            return new StringView(in MemUtils.GetRelativeTo<byte>(ptr, FileNameOffset));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ref RelocationTable GetRelocationTable()
    {
        if (RelocationTableOffset == 0) {
            return ref Unsafe.NullRef<RelocationTable>();
        }

        fixed (BinaryFileHeader* ptr = &this) {
            return ref MemUtils.GetRelativeTo<RelocationTable>(ptr, RelocationTableOffset);
        }
    }
}