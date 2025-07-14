using System.Runtime.CompilerServices;
using Entish;
using EventFlowSharp.ORE;

namespace EventFlowSharp.EVFL;

[Swappable]
public partial struct ResFlowchart
{
    public const uint ResFlowchartMagic = 0x4C465645;

    public uint Magic = ResFlowchartMagic;
    
    /// <summary>
    /// String pool offset (relative to this structure)
    /// </summary>
    public uint StringPoolOffset;
    private uint _reserved8;
    private uint _reservedC;
    public ushort ActorCount;
    public ushort ActionCount;
    public ushort QueryCount;
    public ushort EventCount;
    public ushort EntryPointCount;
    private ushort _reserved1A;
    private ushort _reserved1B;
    private ushort _reserved1C;
    public BinaryPointer<BinaryString<byte>> Name;
    public BinaryPointer<ResActor> Actors;
    public BinaryPointer<ResEvent> Events;
    public BinaryPointer<ResDic> EntryPointNames;
    public BinaryPointer<ResEntryPoint> EntryPoints;

    public ResFlowchart()
    {
    }
    
    public unsafe ref ResEntryPoint GetEntryPoint(StringView entryPointName)
    {
        int index = EntryPointNames.Get().FindIndex(entryPointName.Value);
        if (index == -1) {
            return ref Unsafe.NullRef<ResEntryPoint>();
        }

        return ref MemUtils.GetRelativeTo<ResEntryPoint, ResEntryPoint>(
            in EntryPoints.Get(), index * sizeof(ResEntryPoint)
        );
    }

    public unsafe StringView GetEntryPointName(int index)
    {
        return EntryPointNames.Get().GetEntries()[1 + index].GetKey();
    }
}