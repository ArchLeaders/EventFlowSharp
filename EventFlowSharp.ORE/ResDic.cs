using System.Runtime.CompilerServices;

namespace EventFlowSharp.ORE;

[Swappable]
public unsafe partial struct ResDic
{
    public uint Magic;
    public int EntryCount;
    public ResDicEntry FirstEntry;
    
    public ref ResDicEntry FindEntry(in StringView key) => ref FindEntry(key.Value);
    
    public ref ResDicEntry FindEntry(in ReadOnlySpan<byte> key)
    {
        ResDicEntry* entries = GetEntries();
        ResDicEntry* prev = &entries[0];
        ResDicEntry* entry = &entries[prev->NextIndices[0]];
        
        while (prev->CompactBitIndex < entry->CompactBitIndex) {
            int bitIndex = entry->CompactBitIndex;
            long bit = 0;

            if ((uint)key.Length > (uint)(bitIndex >> 3)) {
                bit = key[key.Length + -((bitIndex >> 3) + 1)] >> (bitIndex & 7) & 1;
            }

            prev = entry;
            entry = &entries[prev->NextIndices[bit]];
        }
        
        return ref Unsafe.AsRef<ResDicEntry>(entry);
    }

    /// <summary>
    /// Returns the index for the specified key or -1 if it cannot be found.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int FindIndex(in StringView key) => FindIndex(key.Value);

    /// <inheritdoc cref="FindIndex(in StringView)"/>
    public int FindIndex(in ReadOnlySpan<byte> key)
    {    
        ref ResDicEntry entry = ref FindEntry(key);
        StringView entryName = entry.GetKey();
        if (!entryName.Value.SequenceEqual(key)) {
            return -1;
        }
        
        return (int)((ulong)Unsafe.AsPointer(ref entry) - (ulong)&GetEntries()[1]);
    }

    /// <summary>
    /// Entry 0 is the root entry.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResDicEntry* GetEntries()
    {
        fixed (ResDicEntry* ptr = &FirstEntry) {
            return ptr;
        }
    }

    public static int FindRefBit(in ReadOnlySpan<byte> str1, in ReadOnlySpan<byte> str2)
    {
        int len1 = str1.Length;
        int len2 = str2.Length;
        int len = Math.Max(len1, len2);

        for (int bitIndex = 0; bitIndex < 8 * len; ++bitIndex) {
            int idx = bitIndex >> 3;

            int bit1 = 0;
            if (len1 > idx) {
                bit1 = str1[len1 + -(idx + 1)] >> (bitIndex % 8) & 1;
            }

            int bit2 = 0;
            if (len2 > idx) {
                bit2 = str2[len2 + -(idx + 1)] >> (bitIndex % 8) & 1;
            }

            if (bit1 != bit2) {
                return bitIndex;
            }
        }

        return -1;
    }
}

[Swappable]
public unsafe partial struct ResDicEntry
{
    // Bits 3-7: index of the byte that should be checked
    // Bits 0-2: index of the bit in that byte
    public int CompactBitIndex;
    public fixed ushort NextIndices[2];
    public BinaryPointer<BinaryString<byte>> Name;
    
    public StringView GetKey() => Name.Get().Data;
}
