using System.Runtime.CompilerServices;

namespace EventFlowSharp.ORE;

[Reversable]
public unsafe partial struct RelocationTable
{
    public uint Magic;
    public int TableStartOffset;
    public int SectionCount;
    private uint _padding;
    public Section FirstSection;

    public void Relocate()
    {
        byte* tableBase = GetTableBase();
        Section* sections = GetSections();
        Section.Entry* entries = GetEntries();

        for (int sectionIndex = 0; sectionIndex < SectionCount; sectionIndex++) {
            ref Section section = ref sections[sectionIndex];
            
            byte* @base = (byte*)section.GetBasePtr(tableBase);
            int globalEntryIndex = section.FirstEntryIndex;
            int end = globalEntryIndex + section.FirstEntryIndex;

            for (int entryIndex = globalEntryIndex; entryIndex < end; entryIndex++) {
                Section.Entry entry = entries[entryIndex];
                int pointersOffset = entry.PointersOffset;
                BitFlag32 mask = new(entry.Mask);
                
                ulong* pointerPtr = (ulong*)(tableBase + pointersOffset);
                for (int i = 0; i < 32; ++i, pointerPtr += sizeof(ulong)) {
                    if (!mask[i]) {
                        continue;
                    }
                    
                    int offset = (int)(*pointerPtr);
                    void* ptr = offset == 0 ? (void*)0 : @base + offset;
                    Buffer.MemoryCopy(ptr, pointerPtr, sizeof(ulong), sizeof(ulong));
                }
            }
        }
    }

    public void UnRelocate()
    {
        byte* tableBase = GetTableBase();
        Section* sections = GetSections();
        Section.Entry* entries = GetEntries();
        
        for (int sectionIndex = 0; sectionIndex < SectionCount; sectionIndex++) {
            ref Section section = ref sections[sectionIndex];
            
            byte* @base = (byte*)section.GetBasePtr(tableBase);
            section.ResetPointer();
            int globalEntryIndex = section.FirstEntryIndex;
            int end = globalEntryIndex + section.FirstEntryIndex;

            for (int entryIndex = globalEntryIndex; entryIndex < end; entryIndex++) {
                Section.Entry entry = entries[entryIndex];
                int pointersOffset = entry.PointersOffset;
                BitFlag32 mask = new(entry.Mask);
                
                var pointerPtr = (void**)(tableBase + pointersOffset);
                for (int i = 0; i < 32; ++i, pointerPtr += sizeof(ulong)) {
                    if (!mask[i]) {
                        continue;
                    }
                    
                    void* ptr = *pointerPtr;
                    long offset = ptr == null ? 0 : (int)((byte*)ptr - @base);
                    Buffer.MemoryCopy(&offset, pointerPtr, sizeof(ulong), sizeof(ulong));
                }
            }
        }
    }
    
    public static int CalcSize(int sectionCount, int entryCount)
    {
        int size = 0x4 * 0x4; // RelocationTable header fields
        size += sizeof(Section) * sectionCount;
        size += sizeof(Section.Entry) * entryCount;
        return size;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Section* GetSections()
    {
        fixed (Section* ptr = &FirstSection) {
            return ptr;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Section.Entry* GetEntries()
    {
        return (Section.Entry*)(
            GetSections() + sizeof(Section.Entry) * SectionCount
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte* GetTableBase()
    {
        fixed (RelocationTable* ptr = &this) {
            return (byte*)ptr - TableStartOffset;
        }
    }
    
    [Reversable]
    public unsafe partial struct Section
    {
        public ulong Pointer;
        public uint Offset;
        public int Size;
        public int FirstEntryIndex;
        public int EntryCount;

        public void ResetPointer() => Pointer = 0;

        public void* GetPtr()
        {
            return (void*)Pointer;
        }

        public void* GetPtrInFile(void* @base)
        {
            return (void*)((ulong)@base + Offset);
        }

        public void* GetBasePtr(void* @base)
        {
            if (Pointer > 0) {
                return (void*)(Pointer - (ulong)@base);
            }

            return @base;
        }
    
        [Reversable]
        public partial struct Entry
        {
            /// <summary>
            /// Offset to pointers to relocate
            /// </summary>
            public int PointersOffset;

            /// <summary>
            /// Bit field that determines which pointers need to be relocated
            /// (next to 32 contiguous pointers starting from the listed offset)
            /// </summary>
            public uint Mask;
        };
    };
}

file readonly struct BitFlag32(uint flags)
{
    private readonly uint _flags = flags;

    public bool this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_flags & (1 << index)) == 1;
    }
};