using System.Runtime.CompilerServices;
using Entish;
using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

public unsafe struct ResEventFlowFile
{
    public const ulong Magic = 0x4C4656454642;
    public static readonly BinaryFileVersion Version = new(0, 3, 0, 0);
    
    public BinaryFileHeader Header;
    public ushort FlowchartCount;
    public ushort TimelineCount;
    public BinaryPointer<BinaryPointer<ResFlowchart>> Flowcharts;
    public BinaryPointer<ResDic> FlowchartNames;
    public BinaryPointer<BinaryPointer<ResTimeline>> Timelines;
    public BinaryPointer<ResDic> TimelineNames;

    /// <summary>
    /// Data must be a pointer to a buffer of size >= 0x20.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static bool IsValid(Span<byte> data)
    {
        byte* ptr = (byte*)Unsafe.AsPointer(ref data.GetPinnableReference());
        return *(ulong*)ptr == Magic &&
               *(BinaryFileVersion*)(ptr + sizeof(ulong)) == Version;
    }

    /// data must be a valid ResEventFlowFile.
    public static ref ResEventFlowFile ResCast(Span<byte> data)
    {
        ref ResEventFlowFile file = ref Unsafe.As<byte, ResEventFlowFile>(ref data[0]);
        file.Relocate();
        return ref file;
    }

    public void Relocate()
    {
        if (Header.IsRelocated()) {
            return;
        }
        
        ref RelocationTable table = ref Header.GetRelocationTable();
        table.Relocate();
        Header.SetRelocated();
    }

    public void UnRelocate()
    {
        if (!Header.IsRelocated()) {
            return;
        }

        ref RelocationTable table = ref Header.GetRelocationTable();
        table.UnRelocate();
        Header.SetUnRelocated();
    }
    
    public static void Swap(ResEventFlowFile* target)
    {
        BinaryFileHeader.Swap(&target->Header);
        EndianUtils.Swap(&target->FlowchartCount);
        EndianUtils.Swap(&target->TimelineCount);
        BinaryPointer<BinaryPointer<ResFlowchart>>.Swap(&target->Flowcharts);
        BinaryPointer<ResDic>.Swap(&target->FlowchartNames);
        BinaryPointer<BinaryPointer<ResTimeline>>.Swap(&target->Timelines);
        BinaryPointer<ResDic>.Swap(&target->TimelineNames);
        
        // Stores the base pointer
        // for swapping BinTPointers (BinaryPointer<T>)
        ResEndian endian = new(target);

        for (int i = 0; i < target->FlowchartCount; i++) {
            BinaryPointer<ResFlowchart>* ptr = target->Flowcharts.ToPtr(endian.Base);
            BinaryPointer<ResFlowchart>.Swap(ptr);
            ResFlowchart* flowchart = ptr->ToPtr(endian.Base);
            ResFlowchart.Swap(flowchart);
        }
        
        ResDic.Swap(target->FlowchartNames.ToPtr(endian.Base));

        for (int i = 0; i < target->TimelineCount; i++) {
            BinaryPointer<ResTimeline>* ptr = target->Timelines.ToPtr(endian.Base);
            BinaryPointer<ResTimeline>.Swap(ptr);
            ResTimeline* timeline = ptr->ToPtr(endian.Base);
            ResTimeline.Swap(timeline);
        }
        
        ResDic.Swap(target->TimelineNames.ToPtr(endian.Base));
        
        StringPool.Swap(target->Header.FindFirstBlock<StringPool>(StringPool.Magic));
    }
}