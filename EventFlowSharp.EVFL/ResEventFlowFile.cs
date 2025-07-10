using System.Runtime.CompilerServices;
using EventFlowSharp.ORE;
using Revrs.Extensions;

namespace EventFlowSharp.EVFL;

public struct ResEventFlowFile
{
    public const ulong Magic = 0x4C4656454642;
    public static readonly BinaryFileVersion Version = new(0, 3, 0, 0);

    /// data must be a pointer to a buffer of size >= 0x20.
    public static bool IsValid(Span<byte> data)
    {
        return data.Read<ulong>() == Magic &&
               data[sizeof(ulong)..].Read<BinaryFileVersion>() == Version;
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

    public BinaryFileHeader Header;
    public ushort FlowchartCount;
    public ushort TimelineCount;
    public BinaryPointer<BinaryPointer<ResFlowchart>> Flowcharts;
    public BinaryPointer<ResDic> FlowchartNames;
    public BinaryPointer<BinaryPointer<ResTimeline>> Timelines;
    public BinaryPointer<ResDic> TimelineNames;
}