using System.Runtime.CompilerServices;
using Revrs;

namespace EventFlowSharp.ORE;

public struct BinaryString<T> : IStructReverser where T : unmanaged
{
    public ushort Length;
    public T Chars;
    
    public unsafe Span<T> Data {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            fixed (T* ptr = &Chars) {
                return new Span<T>(ptr, Length);
            }
        }
    }

    public bool IsEmpty {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Length == 0;
    }

    public ref T this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Data[index];
    }

    public unsafe BinaryString<T> NextString()
    {
        return MemUtils.GetRelativeTo<BinaryString<T>, BinaryString<T>>(this, (sizeof(T) * (Length + 1)).AlignUp(2));
    }

    public static void Reverse(in Span<byte> slice)
    {
        slice[..2].Reverse();
    }
}